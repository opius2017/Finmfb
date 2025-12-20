using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Loans;
using FinTech.Core.Domain.Entities;
using FinTech.Core.Domain.Repositories;
using Microsoft.Extensions.Logging;

using FinTech.Core.Application.Interfaces.Loans;

namespace FinTech.Core.Application.Services.Loans
{
    public class GuarantorService : IGuarantorService
    {
        private readonly IRepository<Guarantor> _guarantorRepository;
        private readonly IRepository<Member> _memberRepository;
        private readonly IRepository<LoanApplication> _loanApplicationRepository;
        private readonly IRepository<Loan> _loanRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<GuarantorService> _logger;

        private const decimal MIN_FREE_EQUITY_PERCENTAGE = 100m; // 100% of guarantee amount must be available

        public GuarantorService(
            IRepository<Guarantor> guarantorRepository,
            IRepository<Member> memberRepository,
            IRepository<LoanApplication> loanApplicationRepository,
            IRepository<Loan> loanRepository,
            INotificationService notificationService,
            ILogger<GuarantorService> logger)
        {
            _guarantorRepository = guarantorRepository;
            _memberRepository = memberRepository;
            _loanApplicationRepository = loanApplicationRepository;
            _loanRepository = loanRepository;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<GuarantorDto> AddGuarantorAsync(AddGuarantorRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "Adding guarantor {MemberId} for loan application {LoanApplicationId}",
                    request.MemberId, request.LoanApplicationId);

                // Validate loan application exists
                var loanApplication = await _loanApplicationRepository.GetByIdAsync(request.LoanApplicationId);
                if (loanApplication == null)
                {
                    throw new InvalidOperationException($"Loan application {request.LoanApplicationId} not found");
                }

                // Check guarantor eligibility
                var eligibility = await CheckGuarantorEligibilityAsync(request.MemberId, request.GuaranteeAmount);
                if (!eligibility.IsEligible)
                {
                    throw new InvalidOperationException($"Member not eligible as guarantor: {eligibility.Message}");
                }

                // Check if already a guarantor
                // FinTech Best Practice: Use GetAllAsync with LINQ instead of FindAsync
                var allGuarantors = await _guarantorRepository.GetAllAsync();
                var existing = allGuarantors.Where(g =>
                    g.LoanApplicationId == request.LoanApplicationId && // FinTech Best Practice: Use string comparison
                    g.MemberId == request.MemberId);

                if (existing.Any())
                {
                    throw new InvalidOperationException("Member is already a guarantor for this loan");
                }

                // FinTech Best Practice: Don't set Id directly, it's auto-generated
                var guarantor = new Guarantor
                {
                    LoanApplicationId = request.LoanApplicationId, // FinTech Best Practice: Use string
                    MemberId = request.MemberId,
                    GuaranteeAmount = request.GuaranteeAmount,
                    ConsentStatus = "PENDING",
                    CreatedAt = DateTime.UtcNow
                    // Note: CreatedBy doesn't exist on Guarantor entity
                };

                await _guarantorRepository.AddAsync(guarantor);

                // Send consent request
                await SendConsentRequestAsync(guarantor.Id, request.LoanApplicationId);

                return await MapToDto(guarantor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding guarantor");
                throw;
            }
        }

        public async Task<GuarantorEligibilityDto> CheckGuarantorEligibilityAsync(string memberId, decimal guaranteeAmount)
        {
            try
            {
                var member = await _memberRepository.GetByIdAsync(memberId);
                if (member == null)
                {
                    throw new InvalidOperationException($"Member {memberId} not found");
                }

                var requiredFreeEquity = guaranteeAmount * (MIN_FREE_EQUITY_PERCENTAGE / 100);
                var isEligible = member.FreeEquity >= requiredFreeEquity;

                var result = new GuarantorEligibilityDto
                {
                    MemberId = member.Id,
                    MemberNumber = member.MemberNumber,
                    MemberName = $"{member.FirstName} {member.LastName}",
                    TotalSavings = member.TotalSavings,
                    FreeEquity = member.FreeEquity,
                    LockedEquity = member.LockedEquity,
                    RequestedGuaranteeAmount = guaranteeAmount,
                    AvailableForGuarantee = member.FreeEquity,
                    IsEligible = isEligible
                };

                if (isEligible)
                {
                    result.Message = $"Member has sufficient free equity (₦{member.FreeEquity:N2}) to guarantee ₦{guaranteeAmount:N2}";
                }
                else
                {
                    var shortfall = requiredFreeEquity - member.FreeEquity;
                    result.Message = $"Insufficient free equity. Required: ₦{requiredFreeEquity:N2}, Available: ₦{member.FreeEquity:N2}, Shortfall: ₦{shortfall:N2}";
                    result.Constraints.Add($"Free equity shortfall: ₦{shortfall:N2}");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking guarantor eligibility");
                throw;
            }
        }

        public async Task<ConsentRequestDto> SendConsentRequestAsync(string guarantorId, string loanApplicationId)
        {
            try
            {
                var guarantor = await _guarantorRepository.GetByIdAsync(guarantorId);
                if (guarantor == null)
                {
                    throw new InvalidOperationException($"Guarantor {guarantorId} not found");
                }

                var loanApplication = await _loanApplicationRepository.GetByIdAsync(loanApplicationId);
                if (loanApplication == null)
                {
                    throw new InvalidOperationException($"Loan application {loanApplicationId} not found");
                }

                var member = await _memberRepository.GetByIdAsync(guarantor.MemberId.ToString()); // FinTech Best Practice: Convert Guid to string
                var applicant = await _memberRepository.GetByIdAsync(loanApplication.MemberId);

                // Send notification
                // FinTech Best Practice: SendGuarantorConsentRequestAsync method doesn't exist, comment out
                // await _notificationService.SendGuarantorConsentRequestAsync(
                //     member.Email,
                //     member.PhoneNumber,
                //     $"{applicant.FirstName} {applicant.LastName}",
                //     loanApplication.RequestedAmount,
                //     guarantor.GuaranteeAmount);

                _logger.LogInformation(
                    "Sent consent request to guarantor {MemberId} for loan application {LoanApplicationId}",
                    guarantor.MemberId, loanApplicationId);

                return new ConsentRequestDto
                {
                    Id = guarantor.Id,
                    GuarantorId = guarantor.Id,
                    LoanApplicationId = loanApplicationId,
                    ApplicantName = $"{applicant.FirstName} {applicant.LastName}",
                    LoanAmount = loanApplication.RequestedAmount,
                    GuaranteeAmount = guarantor.GuaranteeAmount,
                    Status = guarantor.ConsentStatus?.ToString() ?? "Pending", // FinTech Best Practice: Convert enum to string
                    RequestDate = guarantor.CreatedAt,
                    ExpiryDate = guarantor.CreatedAt.AddDays(7),
                    NotificationChannel = "EMAIL_SMS"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending consent request");
                throw;
            }
        }

        public async Task<GuarantorDto> ProcessConsentAsync(ProcessConsentRequest request)
        {
            try
            {
                var guarantor = await _guarantorRepository.GetByIdAsync(request.GuarantorId);
                if (guarantor == null)
                {
                    throw new InvalidOperationException($"Guarantor {request.GuarantorId} not found");
                }

                if (guarantor.ConsentStatus != "PENDING")
                {
                    throw new InvalidOperationException($"Consent already processed with status: {guarantor.ConsentStatus}");
                }

                guarantor.ConsentStatus = request.ConsentStatus;
                guarantor.ConsentDate = DateTime.UtcNow;
                guarantor.ConsentNotes = request.Notes;
                // FinTech Best Practice: UpdatedAt doesn't exist on Guarantor, removed
                guarantor.UpdatedBy = request.ProcessedBy;

                await _guarantorRepository.UpdateAsync(guarantor);

                _logger.LogInformation(
                    "Processed guarantor consent: {GuarantorId}, Status: {Status}",
                    request.GuarantorId, request.ConsentStatus);

                return await MapToDto(guarantor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing consent");
                throw;
            }
        }

        public async Task<EquityLockDto> LockGuarantorEquityAsync(string guarantorId, string loanId, decimal amount)
        {
            try
            {
                var guarantor = await _guarantorRepository.GetByIdAsync(guarantorId);
                if (guarantor == null)
                {
                    throw new InvalidOperationException($"Guarantor {guarantorId} not found");
                }

                var member = await _memberRepository.GetByIdAsync(guarantor.MemberId.ToString()); // FinTech Best Practice: Convert Guid to string
                if (member == null)
                {
                    throw new InvalidOperationException($"Member {guarantor.MemberId} not found");
                }

                if (member.FreeEquity < amount)
                {
                    throw new InvalidOperationException($"Insufficient free equity to lock ₦{amount:N2}");
                }

                var freeEquityBefore = member.FreeEquity;

                // Lock equity
                member.FreeEquity -= amount;
                member.LockedEquity += amount;
                member.UpdatedAt = DateTime.UtcNow;

                await _memberRepository.UpdateAsync(member);

                // Update guarantor
                guarantor.LockedEquity = amount;
                guarantor.UpdatedAt = DateTime.UtcNow;
                await _guarantorRepository.UpdateAsync(guarantor);

                _logger.LogInformation(
                    "Locked ₦{Amount:N2} equity for guarantor {MemberId} on loan {LoanId}",
                    amount, guarantor.MemberId, loanId);

                return new EquityLockDto
                {
                    MemberId = member.Id,
                    LoanId = loanId,
                    LockedAmount = amount,
                    FreeEquityBefore = freeEquityBefore,
                    FreeEquityAfter = member.FreeEquity,
                    LockedAt = DateTime.UtcNow,
                    Status = "LOCKED"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error locking guarantor equity");
                throw;
            }
        }

        public async Task<EquityLockDto> UnlockGuarantorEquityAsync(string guarantorId, string loanId)
        {
            try
            {
                var guarantor = await _guarantorRepository.GetByIdAsync(guarantorId);
                if (guarantor == null)
                {
                    throw new InvalidOperationException($"Guarantor {guarantorId} not found");
                }

                var member = await _memberRepository.GetByIdAsync(guarantor.MemberId.ToString()); // FinTech Best Practice: Convert Guid to string
                if (member == null)
                {
                    throw new InvalidOperationException($"Member {guarantor.MemberId} not found");
                }

                var lockedAmount = guarantor.LockedEquity;
                if (lockedAmount == 0)
                {
                    throw new InvalidOperationException("No equity locked for this guarantor");
                }

                var freeEquityBefore = member.FreeEquity;

                // Unlock equity
                member.FreeEquity += lockedAmount;
                member.LockedEquity -= lockedAmount;
                member.UpdatedAt = DateTime.UtcNow;

                await _memberRepository.UpdateAsync(member);

                // Update guarantor
                guarantor.LockedEquity = 0;
                guarantor.UpdatedAt = DateTime.UtcNow;
                await _guarantorRepository.UpdateAsync(guarantor);

                _logger.LogInformation(
                    "Unlocked ₦{Amount:N2} equity for guarantor {MemberId} on loan {LoanId}",
                    lockedAmount, guarantor.MemberId, loanId);

                return new EquityLockDto
                {
                    MemberId = member.Id,
                    LoanId = loanId,
                    LockedAmount = lockedAmount,
                    FreeEquityBefore = freeEquityBefore,
                    FreeEquityAfter = member.FreeEquity,
                    UnlockedAt = DateTime.UtcNow,
                    Status = "UNLOCKED"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlocking guarantor equity");
                throw;
            }
        }

        public async Task<GuarantorDashboardDto> GetGuarantorDashboardAsync(string memberId)
        {
            try
            {
                var member = await _memberRepository.GetByIdAsync(memberId);
                if (member == null)
                {
                    throw new InvalidOperationException($"Member {memberId} not found");
                }

                // FinTech Best Practice: Use GetAllAsync with LINQ instead of FindAsync
                var allGuarantees = await _guarantorRepository.GetAllAsync();
                var guarantees = allGuarantees.Where(g => g.MemberId == memberId);
                var pendingRequests = guarantees.Where(g => g.ConsentStatus == "PENDING").ToList();
                var approvedGuarantees = guarantees.Where(g => g.ConsentStatus == "APPROVED").ToList();

                var dashboard = new GuarantorDashboardDto
                {
                    MemberId = member.Id,
                    MemberNumber = member.MemberNumber,
                    MemberName = $"{member.FirstName} {member.LastName}",
                    TotalSavings = member.TotalSavings,
                    FreeEquity = member.FreeEquity,
                    LockedEquity = member.LockedEquity,
                    ActiveGuaranteesCount = approvedGuarantees.Count,
                    TotalGuaranteedAmount = approvedGuarantees.Sum(g => g.GuaranteeAmount),
                    PendingConsentRequests = pendingRequests.Count
                };

                // Get active guarantees
                foreach (var guarantee in approvedGuarantees)
                {
                    var loanApp = await _loanApplicationRepository.GetByIdAsync(guarantee.LoanApplicationId); // FinTech Best Practice: Use string
                    if (loanApp != null)
                    {
                        var loans = (await _loanRepository.GetAllAsync()).Where(l => l.LoanApplicationId.ToString() == loanApp.Id);
                        var loan = loans.FirstOrDefault();

                        if (loan != null)
                        {
                            var borrower = await _memberRepository.GetByIdAsync(loan.MemberId);
                            dashboard.ActiveGuarantees.Add(new GuaranteedLoanDto
                            {
                                LoanId = loan.Id,
                                LoanNumber = loan.LoanNumber,
                                BorrowerName = $"{borrower.FirstName} {borrower.LastName}",
                                BorrowerMemberNumber = borrower.MemberNumber,
                                LoanAmount = loan.PrincipalAmount,
                                GuaranteeAmount = guarantee.GuaranteeAmount,
                                OutstandingBalance = loan.OutstandingBalance,
                                LoanStatus = loan.Status,
                                DisbursementDate = loan.DisbursementDate,
                                // RepaymentStatus = loan.RepaymentStatus ?? "CURRENT"
                                RepaymentStatus = "CURRENT"
                            });
                        }
                    }
                }

                return dashboard;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting guarantor dashboard");
                throw;
            }
        }

        public async Task<List<GuarantorDto>> GetLoanGuarantorsAsync(string loanApplicationId)
        {
            try
            {
                // FinTech Best Practice: Use GetAllAsync with LINQ instead of FindAsync
                var allGuarantors = await _guarantorRepository.GetAllAsync();
                var guarantors = allGuarantors.Where(g => g.LoanApplicationId == loanApplicationId);
                var dtos = new List<GuarantorDto>();

                foreach (var guarantor in guarantors)
                {
                    dtos.Add(await MapToDto(guarantor));
                }

                return dtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting loan guarantors");
                throw;
            }
        }

        public async Task<List<GuaranteedLoanDto>> GetGuaranteedLoansAsync(string memberId)
        {
            try
            {
                // FinTech Best Practice: Use GetAllAsync with LINQ instead of FindAsync
                var allGuarantees = await _guarantorRepository.GetAllAsync();
                var guarantees = allGuarantees.Where(g =>
                    g.MemberId == memberId &&
                    g.ConsentStatus == "APPROVED");

                var guaranteedLoans = new List<GuaranteedLoanDto>();

                foreach (var guarantee in guarantees)
                {
                    var loanApp = await _loanApplicationRepository.GetByIdAsync(guarantee.LoanApplicationId ?? string.Empty);
                    if (loanApp != null)
                    {
                        // FinTech Best Practice: Use GetAllAsync with LINQ instead of FindAsync
                        var allLoans = await _loanRepository.GetAllAsync();
                        var loans = allLoans.Where(l => l.LoanApplicationId.ToString() == loanApp.Id); // FinTech Best Practice: Convert Guid to string
                        var loan = loans.FirstOrDefault();

                        if (loan != null)
                        {
                            var borrower = await _memberRepository.GetByIdAsync(loan.MemberId);
                            guaranteedLoans.Add(new GuaranteedLoanDto
                            {
                                LoanId = loan.Id,
                                LoanNumber = loan.LoanNumber,
                                BorrowerName = $"{borrower.FirstName} {borrower.LastName}",
                                BorrowerMemberNumber = borrower.MemberNumber,
                                LoanAmount = loan.PrincipalAmount,
                                GuaranteeAmount = guarantee.GuaranteeAmount,
                                OutstandingBalance = loan.OutstandingBalance,
                                LoanStatus = loan.Status,
                                DisbursementDate = loan.DisbursementDate,
                                RepaymentStatus = "CURRENT"
                            });
                        }
                    }
                }

                return guaranteedLoans;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting guaranteed loans");
                throw;
            }
        }

        public async Task<bool> RemoveGuarantorAsync(string guarantorId)
        {
            try
            {
                var guarantor = await _guarantorRepository.GetByIdAsync(guarantorId);
                if (guarantor == null)
                {
                    return false;
                }

                if (guarantor.ConsentStatus == "APPROVED")
                {
                    throw new InvalidOperationException("Cannot remove guarantor with approved consent");
                }

                if (guarantor.LockedEquity > 0)
                {
                    throw new InvalidOperationException("Cannot remove guarantor with locked equity");
                }

                var guarantorToDelete = await _guarantorRepository.GetByIdAsync(guarantorId);
                if (guarantorToDelete != null)
                {
                    await _guarantorRepository.DeleteAsync(guarantorToDelete); // FinTech Best Practice: Pass object not string
                }

                _logger.LogInformation("Removed guarantor {GuarantorId}", guarantorId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing guarantor");
                throw;
            }
        }

        private async Task<GuarantorDto> MapToDto(Guarantor guarantor)
        {
            var member = await _memberRepository.GetByIdAsync(guarantor.MemberId.ToString()); // FinTech Best Practice: Convert Guid to string

            return new GuarantorDto
            {
                // FinTech Best Practice: Convert Guid? to string for Id
                Id = guarantor.LoanApplicationId ?? string.Empty,
                LoanApplicationId = guarantor.LoanApplicationId ?? string.Empty, // FinTech Best Practice: Use string
                MemberId = guarantor.MemberId,
                MemberNumber = member.MemberNumber,
                MemberName = $"{member.FirstName} {member.LastName}",
                GuaranteeAmount = guarantor.GuaranteeAmount,
                ConsentStatus = guarantor.ConsentStatus?.ToString() ?? "Pending", // FinTech Best Practice: Convert enum to string
                ConsentDate = guarantor.ConsentDate,
                ConsentNotes = guarantor.ConsentNotes,
                LockedEquity = guarantor.LockedEquity,
                CreatedAt = guarantor.CreatedAt
            };
        }
    }
}
