using System;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Loans
{
    /// <summary>
    /// Represents a configurable loan type in the system
    /// Supports Normal, Commodity, and Car loans with specific rules
    /// </summary>
    public class LoanType : AuditableEntity
    {
        public string Code { get; set; } // e.g., "NORMAL", "COMMODITY", "CAR"
        public string Name { get; set; } // e.g., "Normal Loan", "Commodity Loan", "Car Loan"
        public string Description { get; set; }
        
        /// <summary>
        /// Maximum allowed interest rate for this loan type
        /// </summary>
        public decimal MaxInterestRate { get; set; }
        
        /// <summary>
        /// Minimum allowed interest rate for this loan type
        /// </summary>
        public decimal MinInterestRate { get; set; }
        
        /// <summary>
        /// Default interest rate if not specified
        /// </summary>
        public decimal DefaultInterestRate { get; set; }
        
        /// <summary>
        /// Maximum loan term in months
        /// </summary>
        public int MaxLoanTermMonths { get; set; }
        
        /// <summary>
        /// Minimum loan term in months
        /// </summary>
        public int MinLoanTermMonths { get; set; }
        
        /// <summary>
        /// Maximum amount as multiple of member's savings
        /// E.g., 3 means member can borrow up to 3x their savings
        /// </summary>
        public decimal MaxLoanMultiplier { get; set; }
        
        /// <summary>
        /// Minimum savings required (in Naira)
        /// </summary>
        public decimal MinimumSavingsRequired { get; set; }
        
        /// <summary>
        /// Maximum deduction rate as percentage of monthly income
        /// Compliance with Central Bank guidelines
        /// </summary>
        public decimal MaxDeductionRatePercent { get; set; }
        
        /// <summary>
        /// Processing fee percentage
        /// </summary>
        public decimal ProcessingFeePercent { get; set; }
        
        /// <summary>
        /// Whether guarantors are required for this loan type
        /// </summary>
        public bool RequiresGuarantors { get; set; }
        
        /// <summary>
        /// Minimum number of guarantors required
        /// </summary>
        public int MinGuarantorsRequired { get; set; }
        
        /// <summary>
        /// Whether collateral is required
        /// </summary>
        public bool RequiresCollateral { get; set; }
        
        /// <summary>
        /// Collateral value as percentage of loan amount (e.g., 1.2 = 120%)
        /// </summary>
        public decimal? CollateralValueRatio { get; set; }
        
        /// <summary>
        /// Whether this loan type requires Loan Committee approval
        /// </summary>
        public bool RequiresCommitteeApproval { get; set; }
        
        /// <summary>
        /// Loan committee approval threshold amount
        /// Loans above this amount require committee review
        /// </summary>
        public decimal? CommitteeApprovalThreshold { get; set; }
        
        /// <summary>
        /// Whether loan applications can be auto-approved if eligibility criteria met
        /// </summary>
        public bool AllowAutoApproval { get; set; }
        
        /// <summary>
        /// Repayment frequency: Weekly, Monthly, etc.
        /// </summary>
        public string DefaultRepaymentFrequency { get; set; } = "Monthly";
        
        /// <summary>
        /// Whether this loan type is active
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Whether this loan type allows early repayment
        /// </summary>
        public bool AllowsEarlyRepayment { get; set; } = true;
        
        /// <summary>
        /// Early repayment penalty percentage (e.g., 2.5% of outstanding balance)
        /// </summary>
        public decimal? EarlyRepaymentPenaltyPercent { get; set; }
        
        // Commodity-specific properties
        public bool IsCommodityLoan { get; set; }
        public string CommodityType { get; set; } // e.g., "Rice", "Maize", "Palm Oil"
        
        // Car loan specific properties
        public bool IsCarLoan { get; set; }
        public decimal? MaxCarLoanAmount { get; set; }
        public int? MaxCarLoanTermMonths { get; set; }
    }
}
