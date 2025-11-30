using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinTech.Core.Application.Services;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Core.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace FinTech.Infrastructure.Services
{
    /// <summary>
    /// Implementation of guarantor service
    /// </summary>
    public class GuarantorService : IGuarantorService
    {
        private readonly IRepository<Member> _memberRepository;
        private readonly IRepository<GuarantorConsent> _consentRepository;
        private readonly IRepository<LoanApplication> _applicationRepository;
        private readonly IRepository<Loan> _loanRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GuarantorService> _logger;
        
        // Configuration constants
        private const int MAX_ACTIVE_GUARANTEES = 5;
        private const decimal MIN_FREE_EQUITY_RATIO = 1.0m; // 100% of guaranteed amount
        
        public GuarantorService(
            IRepository<Member> memberRepository,
            IRepository<GuarantorConsent> consentRepository,
            IRepository<LoanApplication> applicationRepository,
            IRepository<Loan> loanRepository,
            IUnitOfWork unitOfWork,
            ILogger<GuarantorService> logger)
        {
            _memberRepository = memberRepository ?? throw new ArgumentNullException(nameof(memberRepository));
            _consentRepository = consentRepository ?? throw new ArgumentNullException(nameof(consentRepository));
            _applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Validate if member is eligible to be a guarantor
        /// </summary>
        public async Task<GuarantorEligibilityResult> ValidateGuarantorEligibilityAsync(Guid memberId, decimal guaranteedAmount)
        {
            var member = await _memberRepository.GetByIdAsync(memberId.ToString());
            if (member == null)
                throw new ArgumentException($"Member not found: {memberId}", nameof(memberId));
            
            var result = new GuarantorEligibilityResult
            {
                IsEligible = true,
                FreeEquity = member.FreeEquity,
                LockedEquity = member.LockedEquity,
                RequiredEquity = guaranteedAmount * MIN_FREE_EQUITY_RATIO
            };
            
            // Check if member is active
            if (!member.IsActive)
            {
                result.AddReason("Member account is not active");
            }
            
            // Check if member has sufficient free equity
            decimal requiredEquity = guaranteedAmount * MIN_FREE_EQUITY_RATIO;
            if (member.FreeEquity < requiredEquity)
            {
                result.AddReason(
                    $"Insufficient free equity. Required: ₦{requiredEquity:N2}, Available: ₦{member.FreeEquity:N2}");
            }
            
            // Check active guarantee count
            var activeGuarantees = member.GuarantorObligations
                .Count(g => g.Status == GuarantorStatus.Verified);
            
            result.ActiveGuaranteeCount = activeGuarantees;
            
            if (activeGuarantees >= MAX_ACTIVE_GUARANTEES)
            {
                result.AddReason(
                    $"Maximum active guarantees reached. Current: {activeGuarantees}, Maximum: {MAX_ACTIVE_GUARANTEES}");
            }
            
            _logger.LogInformation(
                "Guarantor eligibility check: Member={MemberId}, Amount={Amount}, Eligible={IsEligible}",
                memberId, guaranteedAmount, result.IsEligible);
            
            return result;
        }
        
        /// <summary>
        /// Request consent from a guarantor
        /// </summary>
        public async Task<GuarantorConsent> RequestConsentAsync(GuarantorConsentRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            
            // Validate guarantor eligibility
            var eligibility = await ValidateGuarantorEligibilityAsync(
                request.GuarantorMemberId, 
                request.GuaranteedAmount);
            
            if (!eligibility.IsEligible)
            {
                throw new InvalidOperationException(
                    $"Guarantor is not eligible: {string.Join(", ", eligibility.Reasons)}");
            }
            
            // Check if there's already a pending consent for this application and guarantor
            var existingConsent = (await _consentRepository.GetAllAsync())
                .FirstOrDefault(c => 
                    c.ApplicationId == request.ApplicationId && 
                    c.GuarantorMemberId == request.GuarantorMemberId &&
                    c.Status == GuarantorConsentStatus.Pending);
            
            if (existingConsent != null)
            {
                _logger.LogWarning(
                    "Consent request already exists: Application={ApplicationId}, Guarantor={GuarantorId}",
                    request.ApplicationId, request.GuarantorMemberId);
                return existingConsent;
            }
            
            // Create new consent request
            var consent = new GuarantorConsent(
                request.ApplicationId,
                request.GuarantorMemberId,
                request.ApplicantMemberId,
                request.GuaranteedAmount);
            
            await _consentRepository.AddAsync(consent);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation(
                "Guarantor consent requested: Token={Token}, Application={ApplicationId}, Guarantor={GuarantorId}",
                consent.ConsentToken, request.ApplicationId, request.GuarantorMemberId);
            
            // TODO: Send notification to guarantor (SMS/Email)
            
            return consent;
        }
        
        /// <summary>
        /// Approve guarantor consent
        /// </summary>
        public async Task<GuarantorConsent> ApproveConsentAsync(string consentToken, string? notes = null)
        {
            var consent = (await _consentRepository.GetAllAsync())
                .FirstOrDefault(c => c.ConsentToken == consentToken);
            
            if (consent == null)
                throw new ArgumentException($"Consent not found: {consentToken}", nameof(consentToken));
            
            consent.Approve(notes);
            
            await _consentRepository.UpdateAsync(consent);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation(
                "Guarantor consent approved: Token={Token}, Application={ApplicationId}",
                consentToken, consent.ApplicationId);
            
            // TODO: Send notification to applicant
            
            return consent;
        }
        
        /// <summary>
        /// Decline guarantor consent
        /// </summary>
        public async Task<GuarantorConsent> DeclineConsentAsync(string consentToken, string reason)
        {
            var consent = (await _consentRepository.GetAllAsync())
                .FirstOrDefault(c => c.ConsentToken == consentToken);
            
            if (consent == null)
                throw new ArgumentException($"Consent not found: {consentToken}", nameof(consentToken));
            
            consent.Decline(reason);
            
            await _consentRepository.UpdateAsync(consent);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation(
                "Guarantor consent declined: Token={Token}, Application={ApplicationId}, Reason={Reason}",
                consentToken, consent.ApplicationId, reason);
            
            // TODO: Send notification to applicant
            
            return consent;
        }
        
        /// <summary>
        /// Lock guarantor equity when loan is approved
        /// </summary>
        public async Task LockGuarantorEquityAsync(Guid guarantorId, decimal amount, Guid loanId)
        {
            var member = await _memberRepository.GetByIdAsync(guarantorId.ToString());
            if (member == null)
                throw new ArgumentException($"Member not found: {guarantorId}", nameof(guarantorId));
            
            member.LockEquity(amount);
            
            await _memberRepository.UpdateAsync(member);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation(
                "Guarantor equity locked: Member={MemberId}, Amount={Amount}, Loan={LoanId}",
                guarantorId, amount, loanId);
        }
        
        /// <summary>
        /// Release guarantor equity when loan is closed
        /// </summary>
        public async Task ReleaseGuarantorEquityAsync(Guid guarantorId, decimal amount, Guid loanId)
        {
            var member = await _memberRepository.GetByIdAsync(guarantorId.ToString());
            if (member == null)
                throw new ArgumentException($"Member not found: {guarantorId}", nameof(guarantorId));
            
            member.ReleaseEquity(amount);
            
            await _memberRepository.UpdateAsync(member);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation(
                "Guarantor equity released: Member={MemberId}, Amount={Amount}, Loan={LoanId}",
                guarantorId, amount, loanId);
        }
        
        /// <summary>
        /// Get all guarantor obligations for a member
        /// </summary>
        public async Task<List<GuarantorObligation>> GetGuarantorObligationsAsync(Guid memberId)
        {
            var member = await _memberRepository.GetByIdAsync(memberId.ToString());
            if (member == null)
                throw new ArgumentException($"Member not found: {memberId}", nameof(memberId));
            
            var obligations = new List<GuarantorObligation>();
            
            foreach (var guarantor in member.GuarantorObligations)
            {
                var loan = guarantor.Loan;
                if (loan == null) continue;
                
                obligations.Add(new GuarantorObligation
                {
                    LoanId = Guid.Parse(loan.Id),
                    LoanNumber = loan.LoanNumber,
                    BorrowerName = $"{loan.CustomerId}", // TODO: Get actual borrower name
                    GuaranteedAmount = guarantor.GuaranteedAmount,
                    OutstandingAmount = loan.OutstandingPrincipal,
                    LoanStatus = loan.Status,
                    GuaranteeDate = guarantor.GuaranteeDate,
                    IsActive = loan.Status == "ACTIVE"
                });
            }
            
            return obligations;
        }
        
        /// <summary>
        /// Get pending consent requests for a guarantor
        /// </summary>
        public async Task<List<GuarantorConsent>> GetPendingConsentRequestsAsync(Guid guarantorMemberId)
        {
            var consents = (await _consentRepository.GetAllAsync())
                .Where(c => 
                    c.GuarantorMemberId == guarantorMemberId &&
                    c.Status == GuarantorConsentStatus.Pending &&
                    c.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(c => c.RequestedAt)
                .ToList();
            
            return consents;
        }
        
        /// <summary>
        /// Check and expire old consent requests
        /// </summary>
        public async Task ExpireOldConsentRequestsAsync()
        {
            var expiredConsents = (await _consentRepository.GetAllAsync())
                .Where(c => 
                    c.Status == GuarantorConsentStatus.Pending &&
                    c.ExpiresAt <= DateTime.UtcNow)
                .ToList();
            
            foreach (var consent in expiredConsents)
            {
                consent.MarkAsExpired();
                await _consentRepository.UpdateAsync(consent);
            }
            
            if (expiredConsents.Any())
            {
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation(
                    "Expired {Count} old consent requests",
                    expiredConsents.Count);
            }
        }
    }
}
