using System;
using FinTech.Core.Domain.Common;

namespace FinTech.Core.Domain.Entities.Loans
{
    /// <summary>
    /// Loan eligibility rules engine
    /// Defines criteria for automatic approval or committee review
    /// Supports different rules for different loan types
    /// </summary>
    public class LoanEligibilityRule : AuditableEntity
    {
        /// <summary>
        /// Reference to loan type this rule applies to
        /// Null means rule applies to all loan types
        /// </summary>
        public int? LoanTypeId { get; set; }
        public virtual LoanType LoanType { get; set; }
        
        /// <summary>
        /// Rule name: "MinimumSavingsRatio", "MaxDebtToIncomeRatio", "CreditScoreThreshold", etc.
        /// </summary>
        public string RuleName { get; set; }
        
        /// <summary>
        /// Human-readable description of the rule
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Rule priority: Higher number = evaluated first
        /// </summary>
        public int Priority { get; set; }
        
        /// <summary>
        /// Whether this rule is active
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Minimum member savings required (in Naira)
        /// </summary>
        public decimal? MinimumMemberSavings { get; set; }
        
        /// <summary>
        /// Minimum savings-to-loan ratio
        /// E.g., 0.33 means member must have saved at least 33% of loan amount
        /// </summary>
        public decimal? MinimumSavingsToLoanRatio { get; set; }
        
        /// <summary>
        /// Minimum required credit score (0-100)
        /// </summary>
        public decimal? MinimumCreditScore { get; set; }
        
        /// <summary>
        /// Maximum debt-to-income ratio
        /// E.g., 0.40 means monthly debt payments cannot exceed 40% of income
        /// </summary>
        public decimal? MaxDebtToIncomeRatio { get; set; }
        
        /// <summary>
        /// Maximum number of existing active loans
        /// </summary>
        public int? MaxActiveLoans { get; set; }
        
        /// <summary>
        /// Minimum months of membership required
        /// </summary>
        public int? MinimumMembershipMonths { get; set; }
        
        /// <summary>
        /// Maximum loan amount multiplier on savings
        /// E.g., 3.0 means can borrow up to 3x savings
        /// </summary>
        public decimal? MaxLoanMultiplier { get; set; }
        
        /// <summary>
        /// Automatic approval if all criteria met
        /// </summary>
        public bool AllowsAutoApproval { get; set; }
        
        /// <summary>
        /// Category: "SavingsRatio", "CreditScore", "Income", "DefaultHistory", "Compliance"
        /// </summary>
        public string Category { get; set; }
        
        /// <summary>
        /// Whether rule must be satisfied (AND) or can be one of alternatives (OR)
        /// </summary>
        public bool IsMandatory { get; set; } = true;
        
        /// <summary>
        /// Default behavior if rule check fails: "AutoReject", "ManualReview", "Approve"
        /// </summary>
        public string FailureAction { get; set; } = "ManualReview";
        
        /// <summary>
        /// Effective start date of this rule
        /// </summary>
        public DateTime EffectiveFromDate { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// End date if rule is being phased out
        /// </summary>
        public DateTime? EffectiveToDate { get; set; }
        
        /// <summary>
        /// Officer who created/last modified this rule
        /// </summary>
        public string ModifiedByOfficerId { get; set; }
        
        /// <summary>
        /// Reason for last rule change
        /// </summary>
        public string LastModificationReason { get; set; }
    }
}
