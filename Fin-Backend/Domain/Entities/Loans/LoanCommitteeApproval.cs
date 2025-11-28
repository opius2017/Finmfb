using System;
using System.Collections.Generic;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Loans
{
    /// <summary>
    /// Represents the Loan Committee approval workflow
    /// All loans above certain threshold or high-risk loans must go through committee approval
    /// Implements governance and compliance requirement
    /// </summary>
    public class LoanCommitteeApproval : AuditableEntity
    {
        /// <summary>
        /// Reference to the loan application being reviewed
        /// </summary>
        public int LoanApplicationId { get; set; }
        
        /// <summary>
        /// Loan application reference
        /// </summary>
        public virtual LoanApplication LoanApplication { get; set; }
        
        /// <summary>
        /// Unique committee approval reference number
        /// </summary>
        public string ApprovalRefNumber { get; set; }
        
        /// <summary>
        /// Date when application was submitted to committee
        /// </summary>
        public DateTime SubmittedToCommitteeDate { get; set; }
        
        /// <summary>
        /// Reason for committee referral
        /// E.g., "HighAmount", "HighRisk", "LowCreditScore", "MultipleDefaultHistory"
        /// </summary>
        public string ReferralReason { get; set; }
        
        /// <summary>
        /// Current status: "Pending", "InReview", "Approved", "Rejected", "OnHold"
        /// </summary>
        public string Status { get; set; } = "Pending";
        
        /// <summary>
        /// Summary of application for committee review
        /// </summary>
        public string CommitteeSummary { get; set; }
        
        /// <summary>
        /// Risk rating assessed by credit officer: Low, Medium, High, Critical
        /// </summary>
        public string RiskRating { get; set; }
        
        /// <summary>
        /// Recommendation from credit officer: Approve, Reject, ReviewMore
        /// </summary>
        public string CreditOfficerRecommendation { get; set; }
        
        /// <summary>
        /// Member's loan repayment history score (0-100)
        /// Calculated based on previous loans' timely payments
        /// </summary>
        public decimal? LoanRepaymentHistoryScore { get; set; }
        
        /// <summary>
        /// Number of previous loans successfully repaid
        /// </summary>
        public int PreviousSuccessfulLoans { get; set; }
        
        /// <summary>
        /// Number of previous loan defaults
        /// </summary>
        public int PreviousDefaultCount { get; set; }
        
        /// <summary>
        /// Average days late for previous repayments
        /// </summary>
        public decimal? AverageDaysLate { get; set; }
        
        /// <summary>
        /// Whether member has defaulted on any previous loan
        /// </summary>
        public bool HasDefaultHistory { get; set; }
        
        /// <summary>
        /// Guarantor 1 details and equity check
        /// </summary>
        public int? Guarantor1Id { get; set; }
        public string Guarantor1Name { get; set; }
        public decimal? Guarantor1MemberSavings { get; set; }
        public bool? Guarantor1ApprovedByCommittee { get; set; }
        
        /// <summary>
        /// Guarantor 2 details and equity check
        /// </summary>
        public int? Guarantor2Id { get; set; }
        public string Guarantor2Name { get; set; }
        public decimal? Guarantor2MemberSavings { get; set; }
        public bool? Guarantor2ApprovedByCommittee { get; set; }
        
        /// <summary>
        /// Whether all guarantors are verified and approved
        /// </summary>
        public bool AllGuarantorsApproved { get; set; }
        
        /// <summary>
        /// Committee meeting date when application was reviewed
        /// </summary>
        public DateTime? CommitteeMeetingDate { get; set; }
        
        /// <summary>
        /// Committee members who reviewed the application (comma-separated IDs)
        /// </summary>
        public string CommitteeReviewers { get; set; }
        
        /// <summary>
        /// Date and time of final committee decision
        /// </summary>
        public DateTime? CommitteeDecisionDate { get; set; }
        
        /// <summary>
        /// Final committee decision: "Approved", "Rejected", "ApprovedWithConditions"
        /// </summary>
        public string CommitteeDecision { get; set; }
        
        /// <summary>
        /// Committee approval notes and conditions
        /// </summary>
        public string CommitteeNotes { get; set; }
        
        /// <summary>
        /// Any conditions attached to approval
        /// </summary>
        public string ApprovalConditions { get; set; }
        
        /// <summary>
        /// Officer responsible for approving/rejecting
        /// </summary>
        public string ApprovedByOfficerId { get; set; }
        
        /// <summary>
        /// Approval rejection reason (if rejected)
        /// </summary>
        public string RejectionReason { get; set; }
        
        /// <summary>
        /// Whether applicant can appeal committee decision
        /// </summary>
        public bool AllowsAppeal { get; set; } = true;
        
        /// <summary>
        /// Appeal details if applicant appeals the decision
        /// </summary>
        public string AppealDetails { get; set; }
        
        /// <summary>
        /// Date of appeal submission
        /// </summary>
        public DateTime? AppealSubmittedDate { get; set; }
        
        /// <summary>
        /// Appeal decision: "Approved", "Rejected", "Reconsidered"
        /// </summary>
        public string AppealDecision { get; set; }
    }
}
