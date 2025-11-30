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
    /// Implementation of committee review service
    /// </summary>
    public class CommitteeReviewService : ICommitteeReviewService
    {
        private readonly IRepository<Member> _memberRepository;
        private readonly IRepository<LoanApplication> _applicationRepository;
        private readonly IRepository<Loan> _loanRepository;
        private readonly IRepository<CommitteeReview> _reviewRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CommitteeReviewService> _logger;
        
        public CommitteeReviewService(
            IRepository<Member> memberRepository,
            IRepository<LoanApplication> applicationRepository,
            IRepository<Loan> loanRepository,
            IRepository<CommitteeReview> reviewRepository,
            IUnitOfWork unitOfWork,
            ILogger<CommitteeReviewService> logger)
        {
            _memberRepository = memberRepository ?? throw new ArgumentNullException(nameof(memberRepository));
            _applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
            _reviewRepository = reviewRepository ?? throw new ArgumentNullException(nameof(reviewRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Get comprehensive member credit profile
        /// </summary>
        public async Task<MemberCreditProfile> GetMemberCreditProfileAsync(Guid memberId)
        {
            var member = await _memberRepository.GetByIdAsync(memberId.ToString());
            if (member == null)
                throw new ArgumentException($"Member not found: {memberId}", nameof(memberId));
            
            var memberLoans = member.Loans.ToList();
            var closedLoans = memberLoans.Where(l => l.Status == "CLOSED").ToList();
            var activeLoans = memberLoans.Where(l => l.Status == "ACTIVE").ToList();
            var defaultedLoans = memberLoans.Where(l => l.Status == "DEFAULTED" || l.Status == "WRITTEN_OFF").ToList();
            
            // Calculate repayment score
            var repaymentScore = await CalculateRepaymentScoreAsync(memberId);
            
            // Calculate savings consistency
            bool savingsConsistency = CalculateSavingsConsistency(member);
            
            var profile = new MemberCreditProfile
            {
                MemberId = memberId,
                MemberNumber = member.MemberNumber,
                FullName = member.GetFullName(),
                MembershipDate = member.MembershipDate,
                MembershipDurationMonths = (int)((DateTime.UtcNow - member.MembershipDate).TotalDays / 30.44),
                
                // Savings
                TotalSavings = member.TotalSavings,
                MonthlyContribution = member.MonthlyContribution,
                ShareCapital = member.ShareCapital,
                FreeEquity = member.FreeEquity,
                LockedEquity = member.LockedEquity,
                SavingsConsistency = savingsConsistency,
                ConsecutiveMonthsWithContributions = CalculateConsecutiveMonths(member),
                
                // Loan History
                TotalLoansCount = memberLoans.Count,
                ActiveLoansCount = activeLoans.Count,
                ClosedLoansCount = closedLoans.Count,
                DefaultedLoansCount = defaultedLoans.Count,
                TotalOutstandingLoans = member.TotalOutstandingLoans,
                
                // Repayment Performance
                RepaymentScore = repaymentScore.Score,
                
                // Guarantor Information
                TimesAsGuarantor = member.GuarantorObligations.Count,
                ActiveGuaranteeCount = member.GuarantorObligations.Count(g => g.Status == GuarantorStatus.Verified),
                TotalGuaranteedAmount = member.LockedEquity,
                
                // Credit Assessment
                CreditScore = member.CreditScore ?? 0,
                RiskRating = member.RiskRating ?? "UNRATED",
                DebtToIncomeRatio = CalculateDebtToIncomeRatio(member)
            };
            
            // Add loan history
            foreach (var loan in memberLoans.OrderByDescending(l => l.DisbursementDate).Take(5))
            {
                profile.LoanHistory.Add(new LoanHistoryItem
                {
                    LoanNumber = loan.LoanNumber,
                    PrincipalAmount = loan.PrincipalAmount,
                    DisbursementDate = loan.DisbursementDate ?? DateTime.MinValue,
                    Status = loan.Status,
                    OutstandingBalance = loan.OutstandingPrincipal,
                    WasDefaulted = loan.Status == "DEFAULTED" || loan.Status == "WRITTEN_OFF",
                    RepaymentPerformance = DetermineRepaymentPerformance(loan)
                });
            }
            
            _logger.LogInformation(
                "Generated credit profile: Member={MemberId}, Score={Score}, Rating={Rating}",
                memberId, profile.CreditScore, profile.RiskRating);
            
            return profile;
        }
        
        /// <summary>
        /// Calculate member repayment score (0-100)
        /// </summary>
        public async Task<RepaymentScoreResult> CalculateRepaymentScoreAsync(Guid memberId)
        {
            var member = await _memberRepository.GetByIdAsync(memberId.ToString());
            if (member == null)
                throw new ArgumentException($"Member not found: {memberId}", nameof(memberId));
            
            var result = new RepaymentScoreResult
            {
                Components = new Dictionary<string, decimal>()
            };
            
            decimal totalScore = 0;
            
            // 1. Savings Consistency (30 points)
            decimal savingsScore = CalculateSavingsScore(member);
            result.Components["Savings Consistency"] = savingsScore;
            totalScore += savingsScore;
            
            // 2. Loan Repayment History (40 points)
            decimal repaymentHistoryScore = CalculateRepaymentHistoryScore(member);
            result.Components["Repayment History"] = repaymentHistoryScore;
            totalScore += repaymentHistoryScore;
            
            // 3. Membership Duration (15 points)
            decimal membershipScore = CalculateMembershipScore(member);
            result.Components["Membership Duration"] = membershipScore;
            totalScore += membershipScore;
            
            // 4. Guarantor Performance (15 points)
            decimal guarantorScore = CalculateGuarantorScore(member);
            result.Components["Guarantor Performance"] = guarantorScore;
            totalScore += guarantorScore;
            
            result.Score = Math.Round(totalScore, 2);
            result.Rating = DetermineRating(result.Score);
            
            // Add factors
            if (savingsScore >= 25)
                result.Factors.Add("Excellent savings consistency");
            if (repaymentHistoryScore >= 35)
                result.Factors.Add("Strong repayment history");
            if (membershipScore >= 12)
                result.Factors.Add("Long-term member");
            if (guarantorScore >= 12)
                result.Factors.Add("Reliable guarantor");
            
            return result;
        }
        
        /// <summary>
        /// Submit committee review
        /// </summary>
        public async Task<CommitteeReview> SubmitReviewAsync(SubmitReviewCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            
            var application = await _applicationRepository.GetByIdAsync(command.ApplicationId.ToString());
            if (application == null)
                throw new ArgumentException($"Application not found: {command.ApplicationId}", nameof(command));
            
            var review = new CommitteeReview(
                command.ApplicationId,
                command.ReviewerUserId,
                command.ReviewerName,
                command.Decision,
                command.Comments);
            
            // Set credit assessment if provided
            if (command.CreditScore.HasValue)
            {
                review.SetCreditAssessment(
                    command.CreditScore.Value,
                    command.RiskRating,
                    command.RepaymentScore ?? 0,
                    command.SavingsConsistency ?? false,
                    command.PreviousLoanPerformance ?? false);
            }
            
            // Set recommended terms if provided
            if (command.RecommendedAmount.HasValue || command.RecommendedTenor.HasValue)
            {
                review.SetRecommendedTerms(
                    command.RecommendedAmount,
                    command.RecommendedTenor,
                    command.RecommendedInterestRate);
            }
            
            if (!string.IsNullOrWhiteSpace(command.RecommendedAction))
            {
                review.SetRecommendedAction(command.RecommendedAction);
            }
            
            await _reviewRepository.AddAsync(review);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation(
                "Committee review submitted: Application={ApplicationId}, Reviewer={Reviewer}, Decision={Decision}",
                command.ApplicationId, command.ReviewerName, command.Decision);
            
            return review;
        }
        
        /// <summary>
        /// Get all reviews for an application
        /// </summary>
        public async Task<List<CommitteeReview>> GetApplicationReviewsAsync(Guid applicationId)
        {
            var reviews = (await _reviewRepository.GetAllAsync())
                .Where(r => r.ApplicationId == applicationId)
                .OrderByDescending(r => r.ReviewDate)
                .ToList();
            
            return reviews;
        }
        
        /// <summary>
        /// Get applications pending committee review
        /// </summary>
        public async Task<List<LoanApplication>> GetPendingReviewApplicationsAsync()
        {
            var applications = (await _applicationRepository.GetAllAsync())
                .Where(a => a.Status == LoanApplicationStatus.InReview)
                .OrderBy(a => a.SubmittedAt)
                .ToList();
            
            return applications;
        }
        
        /// <summary>
        /// Check if application has sufficient approvals
        /// </summary>
        public async Task<bool> HasSufficientApprovalsAsync(Guid applicationId, int requiredApprovals = 2)
        {
            var reviews = await GetApplicationReviewsAsync(applicationId);
            var approvalCount = reviews.Count(r => 
                r.Decision == CommitteeReviewDecision.Approved || 
                r.Decision == CommitteeReviewDecision.ApprovedWithConditions);
            
            return approvalCount >= requiredApprovals;
        }
        
        // Helper methods
        
        private bool CalculateSavingsConsistency(Member member)
        {
            // Check if member has been making regular contributions
            // This is a simplified version - in production, you'd check actual contribution history
            return member.MonthlyContribution > 0 && member.TotalSavings > 0;
        }
        
        private int CalculateConsecutiveMonths(Member member)
        {
            // Simplified - in production, check actual contribution records
            var monthsSinceMembership = (int)((DateTime.UtcNow - member.MembershipDate).TotalDays / 30.44);
            return member.MonthlyContribution > 0 ? Math.Min(monthsSinceMembership, 12) : 0;
        }
        
        private decimal CalculateDebtToIncomeRatio(Member member)
        {
            if (!member.NetSalary.HasValue || member.NetSalary.Value == 0)
                return 0;
            
            return member.TotalOutstandingLoans / (member.NetSalary.Value * 12);
        }
        
        private string DetermineRepaymentPerformance(Loan loan)
        {
            if (loan.Status == "CLOSED")
                return "Excellent";
            if (loan.Status == "ACTIVE" && loan.OutstandingPrincipal < loan.PrincipalAmount * 0.5m)
                return "Good";
            if (loan.Status == "ACTIVE")
                return "Fair";
            return "Poor";
        }
        
        private decimal CalculateSavingsScore(Member member)
        {
            decimal score = 0;
            
            // Regular contributions (15 points)
            if (member.MonthlyContribution > 0)
                score += 15;
            
            // Savings balance (15 points)
            if (member.TotalSavings >= 100000) score += 15;
            else if (member.TotalSavings >= 50000) score += 10;
            else if (member.TotalSavings >= 20000) score += 5;
            
            return Math.Min(score, 30);
        }
        
        private decimal CalculateRepaymentHistoryScore(Member member)
        {
            var loans = member.Loans.ToList();
            if (!loans.Any())
                return 20; // No history, give benefit of doubt
            
            decimal score = 0;
            var closedLoans = loans.Count(l => l.Status == "CLOSED");
            var defaultedLoans = loans.Count(l => l.Status == "DEFAULTED" || l.Status == "WRITTEN_OFF");
            
            // Closed loans (20 points)
            if (closedLoans > 0)
                score += Math.Min(closedLoans * 5, 20);
            
            // No defaults (20 points)
            if (defaultedLoans == 0)
                score += 20;
            else
                score -= defaultedLoans * 10;
            
            return Math.Max(0, Math.Min(score, 40));
        }
        
        private decimal CalculateMembershipScore(Member member)
        {
            var months = (int)((DateTime.UtcNow - member.MembershipDate).TotalDays / 30.44);
            
            if (months >= 36) return 15;
            if (months >= 24) return 12;
            if (months >= 12) return 9;
            if (months >= 6) return 6;
            return 3;
        }
        
        private decimal CalculateGuarantorScore(Member member)
        {
            var guaranteeCount = member.GuarantorObligations.Count;
            
            if (guaranteeCount == 0)
                return 10; // No history, neutral
            
            // Being a guarantor shows trust from community
            return Math.Min(guaranteeCount * 3, 15);
        }
        
        private string DetermineRating(decimal score)
        {
            if (score >= 90) return "EXCELLENT";
            if (score >= 75) return "VERY GOOD";
            if (score >= 60) return "GOOD";
            if (score >= 45) return "FAIR";
            return "POOR";
        }
    }
}
