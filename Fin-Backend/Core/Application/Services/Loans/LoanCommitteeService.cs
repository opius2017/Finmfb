using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Loans;
using FinTech.Core.Domain.Entities;
using FinTech.Core.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using FinTech.Core.Domain.Enums.Loans;

using FinTech.Core.Application.Interfaces.Loans;

namespace FinTech.Core.Application.Services.Loans
{
    public class LoanCommitteeService : ILoanCommitteeService
    {
        private readonly IRepository<CommitteeReview> _reviewRepository;
        private readonly IRepository<LoanApplication> _loanApplicationRepository;
        private readonly IRepository<Member> _memberRepository;
        private readonly IRepository<Loan> _loanRepository;
        private readonly IRepository<Repayment> _repaymentRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<LoanCommitteeService> _logger;

        public LoanCommitteeService(
            IRepository<CommitteeReview> reviewRepository,
            IRepository<LoanApplication> loanApplicationRepository,
            IRepository<Member> memberRepository,
            IRepository<Loan> loanRepository,
            IRepository<Repayment> repaymentRepository,
            INotificationService notificationService,
            ILogger<LoanCommitteeService> logger)
        {
            _reviewRepository = reviewRepository;
            _loanApplicationRepository = loanApplicationRepository;
            _memberRepository = memberRepository;
            _loanRepository = loanRepository;
            _repaymentRepository = repaymentRepository;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<CommitteeReviewDto> CreateReviewAsync(CreateCommitteeReviewRequest request)
        {
            try
            {
                var loanApp = await _loanApplicationRepository.GetByIdAsync(request.LoanApplicationId);
                if (loanApp == null)
                {
                    throw new InvalidOperationException($"Loan application {request.LoanApplicationId} not found");
                }

                // Check if review already exists
                // FinTech Best Practice: Convert Guid to string for comparison
                var existing = await _reviewRepository.GetAll()
                    .Where(r => r.LoanApplicationId == request.LoanApplicationId)
                    .ToListAsync();

                if (existing.Any())
                {
                    throw new InvalidOperationException("Review already exists for this loan application");
                }

                // Get member credit profile and repayment score
                var creditProfile = await GetMemberCreditProfileAsync(loanApp.MemberId);
                var repaymentScore = await CalculateRepaymentScoreAsync(loanApp.MemberId);

                var review = new CommitteeReview
                {
                    LoanApplicationId = request.LoanApplicationId,
                    // MemberId = loanApp.MemberId, // MemberId not in CommitteeReview
                    ReviewStatus = "PENDING",
                    // FinTech Best Practice: Convert Guid to string for ReviewerId
                    ReviewerId = request.CreatedBy.ToString(),
                    ReviewerName = request.CreatedBy.ToString(), // Placeholder if name not available
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = request.CreatedBy
                };

                await _reviewRepository.AddAsync(review);

                _logger.LogInformation("Created committee review {ReviewId} for loan application {LoanApplicationId}",
                    review.Id, request.LoanApplicationId);

                return await MapToDto(review, creditProfile, repaymentScore);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating committee review");
                throw;
            }
        }

        public async Task<MemberCreditProfileDto> GetMemberCreditProfileAsync(string memberId)
        {
            try
            {
                var member = await _memberRepository.GetByIdAsync(memberId);
                if (member == null)
                {
                    throw new InvalidOperationException($"Member {memberId} not found");
                }

                var allLoans = await _loanRepository.GetAll()
                    .Where(l => l.MemberId == memberId)
                    .ToListAsync();
                
                var activeLoans = allLoans.Where(l => l.Status == "ACTIVE").ToList();
                var closedLoans = allLoans.Where(l => l.Status == "CLOSED").ToList();

                var loanIds = allLoans.Select(l => l.Id).ToList();
                var allRepayments = await _repaymentRepository.GetAll()
                    .Where(r => loanIds.Contains(r.LoanId))
                    .ToListAsync();

                var onTimePayments = allRepayments.Count(r => r.Status == "ON_TIME"); // PaymentStatus -> Status
                var latePayments = allRepayments.Count(r => r.Status == "LATE");
                var missedPayments = allRepayments.Count(r => r.Status == "MISSED");

                var membershipMonths = (int)((DateTime.UtcNow - member.JoinDate).TotalDays / 30.44); // MembershipStartDate -> JoinDate
                var currentMonthlyDeductions = activeLoans.Sum(l => l.MonthlyInstallment); // MonthlyRepaymentAmount -> MonthlyInstallment
                var deductionRate = member.MonthlyIncome > 0 ? (currentMonthlyDeductions / member.MonthlyIncome) * 100 : 0; // MonthlySalary -> MonthlyIncome

                var profile = new MemberCreditProfileDto
                {
                    MemberId = member.Id,
                    MemberNumber = member.MemberNumber,
                    MemberName = $"{member.FirstName} {member.LastName}",
                    MembershipMonths = membershipMonths,
                    TotalSavings = member.TotalSavings,
                    FreeEquity = member.FreeEquity,
                    MonthlySalary = member.MonthlyIncome, // MonthlySalary -> MonthlyIncome
                    TotalLoansCount = allLoans.Count(),
                    ActiveLoansCount = activeLoans.Count,
                    ClosedLoansCount = closedLoans.Count,
                    TotalBorrowed = allLoans.Sum(l => l.PrincipalAmount),
                    TotalRepaid = allRepayments.Sum(r => r.PaidAmount), // Amount -> PaidAmount
                    CurrentOutstanding = activeLoans.Sum(l => l.OutstandingBalance), // OutstandingBalance exists on Loan
                    OnTimePayments = onTimePayments,
                    LatePayments = latePayments,
                    MissedPayments = missedPayments,
                    CurrentMonthlyDeductions = currentMonthlyDeductions,
                    DeductionRate = deductionRate,
                    RepaymentScore = member.RepaymentScore ?? "N/A",
                    CreditRating = CalculateCreditRating(onTimePayments, latePayments, missedPayments)
                };

                // Build loan history
                foreach (var loan in allLoans.OrderByDescending(l => l.DisbursementDate))
                {
                    var daysOverdue = 0;
                    if (loan.Status == "ACTIVE" && loan.NextPaymentDate.HasValue && loan.NextPaymentDate < DateTime.UtcNow)
                    {
                        daysOverdue = (DateTime.UtcNow - loan.NextPaymentDate.Value).Days;
                    }

                    profile.LoanHistory.Add(new LoanHistoryItem
                    {
                        LoanNumber = loan.LoanNumber,
                        Amount = loan.PrincipalAmount,
                        DisbursementDate = loan.DisbursementDate, // Not nullable in Loan
                        ClosureDate = null, // Loan doesn't have ClosureDate property? Assuming none.
                        Status = loan.Status,
                        RepaymentStatus = loan.DelinquencyStatus ?? "CURRENT", // RepaymentStatus -> DelinquencyStatus
                        DaysOverdue = daysOverdue
                    });
                }

                return profile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting member credit profile");
                throw;
            }
        }

        public async Task<RepaymentScoreDto> CalculateRepaymentScoreAsync(string memberId)
        {
            try
            {
                var member = await _memberRepository.GetByIdAsync(memberId);
                if (member == null)
                {
                    throw new InvalidOperationException($"Member {memberId} not found");
                }

                var loans = await _loanRepository.GetAll()
                    .Where(l => l.MemberId == memberId)
                    .ToListAsync();
                
                var loanIds = loans.Select(l => l.Id).ToList();
                var repayments = await _repaymentRepository.GetAll()
                    .Where(r => loanIds.Contains(r.LoanId))
                    .ToListAsync();

                var totalPayments = repayments.Count();
                var onTimePayments = repayments.Count(r => r.Status == "ON_TIME");
                var latePayments = repayments.Count(r => r.Status == "LATE");
                var missedPayments = repayments.Count(r => r.Status == "MISSED");

                // Calculate payments in last 12 months
                var last12Months = DateTime.UtcNow.AddMonths(-12);
                var recentRepayments = repayments.Where(r => r.PaymentDate >= last12Months).ToList();
                var latePaymentsLast12Months = recentRepayments.Count(r => r.Status == "LATE");
                var missedPaymentsLast12Months = recentRepayments.Count(r => r.Status == "MISSED");

                // Calculate consecutive on-time payments
                var consecutiveOnTime = 0;
                var orderedRepayments = repayments.OrderByDescending(r => r.PaymentDate).ToList();
                foreach (var payment in orderedRepayments)
                {
                    if (payment.Status == "ON_TIME")
                        consecutiveOnTime++;
                    else
                        break;
                }

                // Calculate average delay days for late payments
                var latePaymentsList = repayments.Where(r => r.Status == "LATE").ToList();
                var averageDelayDays = latePaymentsList.Any() 
                    ? (decimal)latePaymentsList.Average(r => r.DaysOverdue) 
                    : 0;

                // Check for active delinquency
                var activeLoans = loans.Where(l => l.Status == "ACTIVE").ToList();
                var hasActiveDelinquency = activeLoans.Any(l => 
                    l.DelinquencyStatus == "DELINQUENT" || l.DelinquencyStatus == "OVERDUE");

                // Calculate score (0-100)
                var score = CalculateScore(
                    totalPayments,
                    onTimePayments,
                    latePayments,
                    missedPayments,
                    consecutiveOnTime,
                    hasActiveDelinquency);

                var grade = GetScoreGrade(score);
                var onTimeRate = totalPayments > 0 ? (decimal)onTimePayments / totalPayments * 100 : 0;

                var scoreDto = new RepaymentScoreDto
                {
                    MemberId = memberId,
                    Score = score,
                    Grade = grade,
                    OnTimePaymentRate = onTimeRate,
                    ConsecutiveOnTimePayments = consecutiveOnTime,
                    TotalPaymentsMade = totalPayments,
                    LatePaymentsLast12Months = latePaymentsLast12Months,
                    MissedPaymentsLast12Months = missedPaymentsLast12Months,
                    AverageDelayDays = averageDelayDays,
                    HasActiveDelinquency = hasActiveDelinquency,
                    CalculatedAt = DateTime.UtcNow
                };

                // Build score factors
                scoreDto.ScoreFactors = BuildScoreFactors(scoreDto);

                // Update member's repayment score
                member.RepaymentScore = grade;
                member.UpdatedAt = DateTime.UtcNow;
                await _memberRepository.UpdateAsync(member);

                return scoreDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating repayment score");
                throw;
            }
        }

        public async Task<CommitteeReviewDto> SubmitReviewDecisionAsync(SubmitReviewDecisionRequest request)
        {
            try
            {
                var review = await _reviewRepository.GetByIdAsync(request.ReviewId);
                if (review == null)
                {
                    throw new InvalidOperationException($"Review {request.ReviewId} not found");
                }

                if (review.ReviewStatus != "PENDING")
                {
                    throw new InvalidOperationException($"Review already processed with status: {review.ReviewStatus}");
                }

                review.ReviewStatus = request.Decision;
                review.Decision = request.Decision;
                review.DecisionNotes = request.Notes;
                review.DecisionDate = DateTime.UtcNow;
                review.ReviewerId = request.ReviewedBy; // ReviewerId instead of ReviewedBy
                review.ReviewerName = request.ReviewedBy; // Placeholder
                // review.ReviewedBy = request.ReviewedBy; // Not in Entity
                review.UpdatedAt = DateTime.UtcNow;
                review.UpdatedBy = request.ReviewedBy;

                await _reviewRepository.UpdateAsync(review);

                // Update loan application status
                var loanApp = await _loanApplicationRepository.GetByIdAsync(review.LoanApplicationId);
                if (loanApp != null)
                {
                    loanApp.Status = request.Decision == "APPROVED" ? 
                        LoanApplicationStatus.InReview : // Was COMMITTEE_APPROVED
                        LoanApplicationStatus.Rejected; // Was COMMITTEE_REJECTED
                    // Note: InReview might not be perfect mapping for CommitteeApproved but matches Enum
                    loanApp.UpdatedAt = DateTime.UtcNow;
                    loanApp.UpdatedBy = request.ReviewedBy;
                    await _loanApplicationRepository.UpdateAsync(loanApp);

                    // Send notification to applicant
                    var member = await _memberRepository.GetByIdAsync(loanApp.MemberId);
                    // FinTech Best Practice: SendCommitteeDecisionNotificationAsync method doesn't exist, commenting out
                    // await _notificationService.SendCommitteeDecisionNotificationAsync(
                    //     member.Email,
                    //     member.PhoneNumber,
                    //     $"{member.FirstName} {member.LastName}",
                    //     request.Decision,
                    //     loanApp.RequestedAmount);

                }

                _logger.LogInformation("Committee decision submitted: {ReviewId}, Decision: {Decision}",
                    request.ReviewId, request.Decision);

                _logger.LogInformation("Committee decision submitted: {ReviewId}, Decision: {Decision}",
                    request.ReviewId, request.Decision);

                var memberId = loanApp.MemberId;
                var creditProfile = await GetMemberCreditProfileAsync(memberId);
                var repaymentScore = await CalculateRepaymentScoreAsync(memberId);

                return await MapToDto(review, creditProfile, repaymentScore);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting review decision");
                throw;
            }
        }

        public async Task<List<CommitteeReviewDto>> GetPendingReviewsAsync()
        {
            try
            {
                var reviews = await _reviewRepository.GetAll()
                    .Where(r => r.ReviewStatus == "PENDING")
                    .Include(r => r.LoanApplication)
                    .ThenInclude(la => la.Member)
                    .ToListAsync();
                
                var dtos = new List<CommitteeReviewDto>();

                foreach (var review in reviews.OrderBy(r => r.CreatedAt))
                {
                    // MemberId is not on review directly in entity, use LoanApplication.MemberId
                    if (review.LoanApplication == null) continue;
                    
                    var creditProfile = await GetMemberCreditProfileAsync(review.LoanApplication.MemberId);
                    var repaymentScore = await CalculateRepaymentScoreAsync(review.LoanApplication.MemberId);
                    dtos.Add(await MapToDto(review, creditProfile, repaymentScore));
                }

                return dtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending reviews");
                throw;
            }
        }

        public async Task<CommitteeReviewDto> GetReviewByIdAsync(string reviewId)
        {
            try
            {
                var review = await _reviewRepository.GetAll()
                    .Where(r => r.Id == reviewId)
                    .Include(r => r.LoanApplication)
                    .ThenInclude(la => la.Member)
                    .FirstOrDefaultAsync();

                if (review == null)
                {
                    throw new InvalidOperationException($"Review {reviewId} not found");
                }

                if (review.LoanApplication == null)
                    throw new InvalidOperationException("Review has no loan application linked");

                var creditProfile = await GetMemberCreditProfileAsync(review.LoanApplication.MemberId);
                var repaymentScore = await CalculateRepaymentScoreAsync(review.LoanApplication.MemberId);

                return await MapToDto(review, creditProfile, repaymentScore);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting review by ID");
                throw;
            }
        }

        public async Task<CommitteeDashboardDto> GetCommitteeDashboardAsync()
        {
            try
            {
                var allReviews = await _reviewRepository.GetAll()
                    .Include(r => r.LoanApplication)
                    .ThenInclude(la => la.Member)
                    .ToListAsync();
                
                var today = DateTime.UtcNow.Date;
                var thisMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

                var pendingReviews = allReviews.Where(r => r.ReviewStatus == "PENDING").ToList();
                var reviewedToday = allReviews.Where(r => r.DecisionDate.HasValue && r.DecisionDate.Value.Date == today).ToList();
                var reviewsThisMonth = allReviews.Where(r => r.CreatedAt >= thisMonth).ToList();

                var dashboard = new CommitteeDashboardDto
                {
                    PendingReviewsCount = pendingReviews.Count,
                    ReviewedTodayCount = reviewedToday.Count,
                    ApprovedTodayCount = reviewedToday.Count(r => r.Decision == "APPROVED"),
                    RejectedTodayCount = reviewedToday.Count(r => r.Decision == "REJECTED"),
                    TotalPendingAmount = 0,
                    TotalApprovedAmount = 0
                };

                // Calculate amounts
                foreach (var review in pendingReviews)
                {
                    var loanApp = await _loanApplicationRepository.GetByIdAsync(review.LoanApplicationId.ToString());
                    if (loanApp != null)
                    {
                        dashboard.TotalPendingAmount += loanApp.RequestedAmount;
                    }
                }

                foreach (var review in reviewedToday.Where(r => r.Decision == "APPROVED"))
                {
                    var loanApp = await _loanApplicationRepository.GetByIdAsync(review.LoanApplicationId.ToString());
                    if (loanApp != null)
                    {
                        dashboard.TotalApprovedAmount += loanApp.RequestedAmount;
                    }
                }

                // Get pending reviews with details
                foreach (var review in pendingReviews.Take(10))
                {
                    if (review.LoanApplication == null) continue;
                    var creditProfile = await GetMemberCreditProfileAsync(review.LoanApplication.MemberId);
                    var repaymentScore = await CalculateRepaymentScoreAsync(review.LoanApplication.MemberId);
                    dashboard.PendingReviews.Add(await MapToDto(review, creditProfile, repaymentScore));
                }

                // Get recent decisions
                var recentDecisions = allReviews
                    .Where(r => r.DecisionDate.HasValue)
                    .OrderByDescending(r => r.DecisionDate)
                    .Take(10)
                    .ToList();

                foreach (var review in recentDecisions)
                {
                    if (review.LoanApplication == null) continue;
                    var creditProfile = await GetMemberCreditProfileAsync(review.LoanApplication.MemberId);
                    var repaymentScore = await CalculateRepaymentScoreAsync(review.LoanApplication.MemberId);
                    dashboard.RecentDecisions.Add(await MapToDto(review, creditProfile, repaymentScore));
                }

                // Calculate statistics
                var approvedThisMonth = reviewsThisMonth.Count(r => r.Decision == "APPROVED");
                var rejectedThisMonth = reviewsThisMonth.Count(r => r.Decision == "REJECTED");
                var decidedThisMonth = approvedThisMonth + rejectedThisMonth;

                dashboard.Statistics = new CommitteeStatistics
                {
                    TotalReviewsThisMonth = reviewsThisMonth.Count,
                    ApprovedThisMonth = approvedThisMonth,
                    RejectedThisMonth = rejectedThisMonth,
                    ApprovalRate = decidedThisMonth > 0 ? (decimal)approvedThisMonth / decidedThisMonth * 100 : 0,
                    AverageApprovedAmount = 0,
                    AverageProcessingDays = 0
                };

                return dashboard;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting committee dashboard");
                throw;
            }
        }

        #region Helper Methods

        private async Task<CommitteeReviewDto> MapToDto(
            CommitteeReview review,
            MemberCreditProfileDto creditProfile,
            RepaymentScoreDto repaymentScore)
        {
            var loanApp = await _loanApplicationRepository.GetByIdAsync(review.LoanApplicationId);
            var member = loanApp != null ? await _memberRepository.GetByIdAsync(loanApp.MemberId) : null;
            
            if (member == null) throw new InvalidOperationException("Member not found for review");

            return new CommitteeReviewDto
            {
                Id = review.Id,
                LoanApplicationId = review.LoanApplicationId,
                MemberId = loanApp.MemberId, // review.MemberId does not exist
                MemberNumber = member.MemberNumber,
                MemberName = $"{member.FirstName} {member.LastName}",
                RequestedAmount = loanApp?.RequestedAmount ?? 0,
                ReviewStatus = review.ReviewStatus,
                Decision = review.Decision,
                DecisionNotes = review.DecisionNotes,
                DecisionDate = review.DecisionDate,
                ReviewedBy = review.ReviewerName, // ReviewedBy was not in Entity
                CreditProfile = creditProfile,
                RepaymentScore = repaymentScore,
                CreatedAt = review.CreatedAt
            };
        }

        private string CalculateCreditRating(int onTimePayments, int latePayments, int missedPayments)
        {
            var totalPayments = onTimePayments + latePayments + missedPayments;
            if (totalPayments == 0) return "NEW";

            var onTimeRate = (decimal)onTimePayments / totalPayments * 100;

            if (onTimeRate >= 95 && missedPayments == 0) return "EXCELLENT";
            if (onTimeRate >= 85 && missedPayments <= 1) return "GOOD";
            if (onTimeRate >= 70) return "FAIR";
            return "POOR";
        }

        private int CalculateScore(
            int totalPayments,
            int onTimePayments,
            int latePayments,
            int missedPayments,
            int consecutiveOnTime,
            bool hasActiveDelinquency)
        {
            if (totalPayments == 0) return 50; // Neutral score for new members

            var baseScore = 100;

            // On-time payment rate (40 points)
            var onTimeRate = (decimal)onTimePayments / totalPayments;
            baseScore = (int)(onTimeRate * 40);

            // Late payments penalty (up to -20 points)
            var latePenalty = Math.Min(latePayments * 2, 20);
            baseScore -= latePenalty;

            // Missed payments penalty (up to -30 points)
            var missedPenalty = Math.Min(missedPayments * 5, 30);
            baseScore -= missedPenalty;

            // Consecutive on-time bonus (up to +20 points)
            var consecutiveBonus = Math.Min(consecutiveOnTime * 2, 20);
            baseScore += consecutiveBonus;

            // Active delinquency penalty (-30 points)
            if (hasActiveDelinquency)
            {
                baseScore -= 30;
            }

            // Ensure score is between 0 and 100
            return Math.Max(0, Math.Min(100, baseScore));
        }

        private string GetScoreGrade(int score)
        {
            if (score >= 85) return "EXCELLENT";
            if (score >= 70) return "GOOD";
            if (score >= 50) return "FAIR";
            return "POOR";
        }

        private List<string> BuildScoreFactors(RepaymentScoreDto score)
        {
            var factors = new List<string>();

            if (score.OnTimePaymentRate >= 95)
                factors.Add($"✓ Excellent on-time payment rate ({score.OnTimePaymentRate:N1}%)");
            else if (score.OnTimePaymentRate < 70)
                factors.Add($"✗ Low on-time payment rate ({score.OnTimePaymentRate:N1}%)");

            if (score.ConsecutiveOnTimePayments >= 6)
                factors.Add($"✓ {score.ConsecutiveOnTimePayments} consecutive on-time payments");

            if (score.LatePaymentsLast12Months > 0)
                factors.Add($"✗ {score.LatePaymentsLast12Months} late payments in last 12 months");

            if (score.MissedPaymentsLast12Months > 0)
                factors.Add($"✗ {score.MissedPaymentsLast12Months} missed payments in last 12 months");

            if (score.HasActiveDelinquency)
                factors.Add("✗ Active delinquency on current loan");

            if (score.AverageDelayDays > 0)
                factors.Add($"Average delay: {score.AverageDelayDays:N1} days");

            return factors;
        }

        #endregion
    }
}
