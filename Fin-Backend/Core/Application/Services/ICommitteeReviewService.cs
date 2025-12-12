using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Loans;

namespace FinTech.Core.Application.Services
{
    /// <summary>
    /// Committee review decision types
    /// </summary>
    public enum CommitteeReviewDecision
    {
        Approved,
        Rejected,
        Deferred,
        ConditionalApproval
    }

    /// <summary>
    /// Service for loan committee review operations
    /// </summary>
    public interface ICommitteeReviewService
    {
        /// <summary>
        /// Get member credit profile for committee review
        /// </summary>
        Task<MemberCreditProfile> GetMemberCreditProfileAsync(Guid memberId);
        
        /// <summary>
        /// Calculate member repayment score
        /// </summary>
        Task<RepaymentScoreResult> CalculateRepaymentScoreAsync(Guid memberId);
        
        /// <summary>
        /// Submit committee review
        /// </summary>
        Task<CommitteeReview> SubmitReviewAsync(SubmitReviewCommand command);
        
        /// <summary>
        /// Get all reviews for an application
        /// </summary>
        Task<List<CommitteeReview>> GetApplicationReviewsAsync(Guid applicationId);
        
        /// <summary>
        /// Get applications pending committee review
        /// </summary>
        Task<List<LoanApplication>> GetPendingReviewApplicationsAsync();
        
        /// <summary>
        /// Check if application has sufficient approvals
        /// </summary>
        Task<bool> HasSufficientApprovalsAsync(Guid applicationId, int requiredApprovals = 2);
    }
    
    /// <summary>
    /// Command to submit a committee review
    /// </summary>
    public class SubmitReviewCommand
    {
        public Guid ApplicationId { get; set; }
        public string ReviewerUserId { get; set; }
        public string ReviewerName { get; set; }
        public CommitteeReviewDecision Decision { get; set; }
        public string Comments { get; set; }
        public decimal? CreditScore { get; set; }
        public string RiskRating { get; set; }
        public decimal? RepaymentScore { get; set; }
        public bool? SavingsConsistency { get; set; }
        public bool? PreviousLoanPerformance { get; set; }
        public decimal? RecommendedAmount { get; set; }
        public int? RecommendedTenor { get; set; }
        public decimal? RecommendedInterestRate { get; set; }
        public string RecommendedAction { get; set; }
    }
    
    /// <summary>
    /// Member credit profile for committee review
    /// </summary>
    public class MemberCreditProfile
    {
        public Guid MemberId { get; set; }
        public string MemberNumber { get; set; }
        public string FullName { get; set; }
        public DateTime MembershipDate { get; set; }
        public int MembershipDurationMonths { get; set; }
        
        // Savings Information
        public decimal TotalSavings { get; set; }
        public decimal MonthlyContribution { get; set; }
        public decimal ShareCapital { get; set; }
        public decimal FreeEquity { get; set; }
        public decimal LockedEquity { get; set; }
        public bool SavingsConsistency { get; set; }
        public int ConsecutiveMonthsWithContributions { get; set; }
        
        // Loan History
        public int TotalLoansCount { get; set; }
        public int ActiveLoansCount { get; set; }
        public int ClosedLoansCount { get; set; }
        public int DefaultedLoansCount { get; set; }
        public decimal TotalOutstandingLoans { get; set; }
        public List<LoanHistoryItem> LoanHistory { get; set; } = new List<LoanHistoryItem>();
        
        // Repayment Performance
        public decimal RepaymentScore { get; set; }
        public int OnTimePaymentsCount { get; set; }
        public int LatePaymentsCount { get; set; }
        public int MissedPaymentsCount { get; set; }
        public decimal AverageRepaymentRate { get; set; }
        
        // Guarantor Information
        public int TimesAsGuarantor { get; set; }
        public int ActiveGuaranteeCount { get; set; }
        public decimal TotalGuaranteedAmount { get; set; }
        
        // Credit Assessment
        public decimal CreditScore { get; set; }
        public string RiskRating { get; set; }
        public decimal DebtToIncomeRatio { get; set; }
    }
    
    /// <summary>
    /// Loan history item
    /// </summary>
    public class LoanHistoryItem
    {
        public string LoanNumber { get; set; }
        public decimal PrincipalAmount { get; set; }
        public DateTime DisbursementDate { get; set; }
        public DateTime? ClosureDate { get; set; }
        public string Status { get; set; }
        public decimal OutstandingBalance { get; set; }
        public int DaysOverdue { get; set; }
        public bool WasDefaulted { get; set; }
        public string RepaymentPerformance { get; set; }
    }
    
    /// <summary>
    /// Repayment score calculation result
    /// </summary>
    public class RepaymentScoreResult
    {
        public decimal Score { get; set; }
        public string Rating { get; set; }
        public Dictionary<string, decimal> Components { get; set; } = new Dictionary<string, decimal>();
        public List<string> Factors { get; set; } = new List<string>();
    }
}
