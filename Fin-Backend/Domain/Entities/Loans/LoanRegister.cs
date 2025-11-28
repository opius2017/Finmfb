using System;
using System.Collections.Generic;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Loans
{
    /// <summary>
    /// Central loan register - maintains complete audit trail of all loans
    /// Provides transparency and helps with reconciliation
    /// Required for microfinance compliance and reporting
    /// </summary>
    public class LoanRegister : AuditableEntity
    {
        /// <summary>
        /// Reference to the actual loan
        /// </summary>
        public int LoanId { get; set; }
        public virtual Loan Loan { get; set; }
        
        /// <summary>
        /// Unique register entry number for audit trail
        /// </summary>
        public string RegisterEntryNumber { get; set; }
        
        /// <summary>
        /// Member/Customer ID
        /// </summary>
        public int CustomerId { get; set; }
        
        /// <summary>
        /// Member name
        /// </summary>
        public string MemberName { get; set; }
        
        /// <summary>
        /// Loan origination date
        /// </summary>
        public DateTime LoanOriginationDate { get; set; }
        
        /// <summary>
        /// Loan type: "Normal", "Commodity", "Car"
        /// </summary>
        public string LoanType { get; set; }
        
        /// <summary>
        /// Principal amount disbursed (₦)
        /// </summary>
        public decimal PrincipalAmount { get; set; }
        
        /// <summary>
        /// Interest rate (%)
        /// </summary>
        public decimal InterestRate { get; set; }
        
        /// <summary>
        /// Total interest payable (₦)
        /// </summary>
        public decimal TotalInterestPayable { get; set; }
        
        /// <summary>
        /// Loan tenor in months
        /// </summary>
        public int LoanTermMonths { get; set; }
        
        /// <summary>
        /// Expected maturity date
        /// </summary>
        public DateTime ExpectedMaturityDate { get; set; }
        
        /// <summary>
        /// Monthly repayment amount (₦)
        /// </summary>
        public decimal MonthlyRepaymentAmount { get; set; }
        
        /// <summary>
        /// Monthly deduction from salary
        /// </summary>
        public decimal MonthlyDeductionAmount { get; set; }
        
        /// <summary>
        /// Number of payments made so far
        /// </summary>
        public int PaymentsMadeCount { get; set; }
        
        /// <summary>
        /// Total amount repaid to date (₦)
        /// </summary>
        public decimal TotalAmountRepaid { get; set; }
        
        /// <summary>
        /// Outstanding balance (₦)
        /// </summary>
        public decimal OutstandingBalance { get; set; }
        
        /// <summary>
        /// Guarantor 1 name
        /// </summary>
        public string Guarantor1Name { get; set; }
        
        /// <summary>
        /// Guarantor 1 ID
        /// </summary>
        public int? Guarantor1Id { get; set; }
        
        /// <summary>
        /// Guarantor 2 name
        /// </summary>
        public string Guarantor2Name { get; set; }
        
        /// <summary>
        /// Guarantor 2 ID
        /// </summary>
        public int? Guarantor2Id { get; set; }
        
        /// <summary>
        /// Current loan status: "Active", "Closed", "Defaulted", "OnHold", "Restructured"
        /// </summary>
        public string LoanStatus { get; set; }
        
        /// <summary>
        /// Days past due (if delinquent)
        /// </summary>
        public int? DaysPastDue { get; set; }
        
        /// <summary>
        /// Amount overdue (₦)
        /// </summary>
        public decimal? AmountOverdue { get; set; }
        
        /// <summary>
        /// Last payment date
        /// </summary>
        public DateTime? LastPaymentDate { get; set; }
        
        /// <summary>
        /// Loan closure date (if loan is closed)
        /// </summary>
        public DateTime? LoanClosureDate { get; set; }
        
        /// <summary>
        /// Reason for closure: "FullRepayment", "Default", "Restructured", "Cancelled"
        /// </summary>
        public string ClosureReason { get; set; }
        
        /// <summary>
        /// Loan Committee approval reference (if applicable)
        /// </summary>
        public string CommitteeApprovalRef { get; set; }
        
        /// <summary>
        /// Loan Committee approval date
        /// </summary>
        public DateTime? CommitteeApprovalDate { get; set; }
        
        /// <summary>
        /// Approving officer's ID
        /// </summary>
        public string ApprovingOfficerId { get; set; }
        
        /// <summary>
        /// Processing fee charged (₦)
        /// </summary>
        public decimal? ProcessingFeeAmount { get; set; }
        
        /// <summary>
        /// Insurance premium (if applicable, ₦)
        /// </summary>
        public decimal? InsurancePremium { get; set; }
        
        /// <summary>
        /// Collateral description (if applicable)
        /// </summary>
        public string CollateralDescription { get; set; }
        
        /// <summary>
        /// Collateral value (₦)
        /// </summary>
        public decimal? CollateralValue { get; set; }
        
        /// <summary>
        /// Notes and remarks
        /// </summary>
        public string Remarks { get; set; }
        
        /// <summary>
        /// Register last reviewed date (for audit purposes)
        /// </summary>
        public DateTime? LastReviewDate { get; set; }
        
        /// <summary>
        /// Reviewed by officer ID
        /// </summary>
        public string ReviewedByOfficerId { get; set; }
        
        /// <summary>
        /// Month and year register entry was created (for monthly schedules)
        /// Format: "YYYY-MM"
        /// </summary>
        public string RegisterMonth { get; set; }
    }
}
