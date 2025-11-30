using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class CommitteeReviewDto
    {
        public string Id { get; set; } = string.Empty;
        public string LoanApplicationId { get; set; } = string.Empty;
        public string MemberId { get; set; } = string.Empty;
        public string MemberNumber { get; set; } = string.Empty;
        public string MemberName { get; set; } = string.Empty;
        public decimal RequestedAmount { get; set; }
        public string ReviewStatus { get; set; } = string.Empty; // PENDING, APPROVED, REJECTED
        public string? Decision { get; set; }
        public string? DecisionNotes { get; set; }
        public DateTime? DecisionDate { get; set; }
        public string? ReviewedBy { get; set; }
        public MemberCreditProfileDto CreditProfile { get; set; } = new MemberCreditProfileDto();
        public RepaymentScoreDto RepaymentScore { get; set; } = new RepaymentScoreDto();
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCommitteeReviewRequest
    {
        public string LoanApplicationId { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class SubmitReviewDecisionRequest
    {
        public string ReviewId { get; set; } = string.Empty;
        public string Decision { get; set; } = string.Empty; // APPROVED, REJECTED
        public string? Notes { get; set; }
        public string ReviewedBy { get; set; } = string.Empty;
    }

    public class MemberCreditProfileDto
    {
        public string MemberId { get; set; } = string.Empty;
        public string MemberNumber { get; set; } = string.Empty;
        public string MemberName { get; set; } = string.Empty;
        public int MembershipMonths { get; set; }
        public decimal TotalSavings { get; set; }
        public decimal FreeEquity { get; set; }
        public decimal MonthlySalary { get; set; }
        public int TotalLoansCount { get; set; }
        public int ActiveLoansCount { get; set; }
        public int ClosedLoansCount { get; set; }
        public decimal TotalBorrowed { get; set; }
        public decimal TotalRepaid { get; set; }
        public decimal CurrentOutstanding { get; set; }
        public int OnTimePayments { get; set; }
        public int LatePayments { get; set; }
        public int MissedPayments { get; set; }
        public decimal CurrentMonthlyDeductions { get; set; }
        public decimal DeductionRate { get; set; }
        public string RepaymentScore { get; set; } = string.Empty;
        public string CreditRating { get; set; } = string.Empty;
        public List<LoanHistoryItem> LoanHistory { get; set; } = new List<LoanHistoryItem>();
    }

    public class LoanHistoryItem
    {
        public string LoanNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime DisbursementDate { get; set; }
        public DateTime? ClosureDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string RepaymentStatus { get; set; } = string.Empty;
        public int DaysOverdue { get; set; }
    }

    public class RepaymentScoreDto
    {
        public string MemberId { get; set; } = string.Empty;
        public int Score { get; set; } // 0-100
        public string Grade { get; set; } = string.Empty; // EXCELLENT, GOOD, FAIR, POOR
        public decimal OnTimePaymentRate { get; set; }
        public int ConsecutiveOnTimePayments { get; set; }
        public int TotalPaymentsMade { get; set; }
        public int LatePaymentsLast12Months { get; set; }
        public int MissedPaymentsLast12Months { get; set; }
        public decimal AverageDelayDays { get; set; }
        public bool HasActiveDelinquency { get; set; }
        public List<string> ScoreFactors { get; set; } = new List<string>();
        public DateTime CalculatedAt { get; set; }
    }

    public class CommitteeDashboardDto
    {
        public int PendingReviewsCount { get; set; }
        public int ReviewedTodayCount { get; set; }
        public int ApprovedTodayCount { get; set; }
        public int RejectedTodayCount { get; set; }
        public decimal TotalPendingAmount { get; set; }
        public decimal TotalApprovedAmount { get; set; }
        public List<CommitteeReviewDto> PendingReviews { get; set; } = new List<CommitteeReviewDto>();
        public List<CommitteeReviewDto> RecentDecisions { get; set; } = new List<CommitteeReviewDto>();
        public CommitteeStatistics Statistics { get; set; } = new CommitteeStatistics();
    }

    public class CommitteeStatistics
    {
        public int TotalReviewsThisMonth { get; set; }
        public int ApprovedThisMonth { get; set; }
        public int RejectedThisMonth { get; set; }
        public decimal ApprovalRate { get; set; }
        public decimal AverageApprovedAmount { get; set; }
        public decimal AverageProcessingDays { get; set; }
    }
}
