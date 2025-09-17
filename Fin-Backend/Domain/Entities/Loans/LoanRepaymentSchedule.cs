using System;
using FinTech.Domain.Common;

namespace FinTech.Domain.Entities.Loans
{
    /// <summary>
    /// Represents a repayment schedule item for a loan
    /// </summary>
    public class LoanRepaymentSchedule : BaseEntity
    {
        public string Id { get; set; }
        public string LoanId { get; set; }
        public int InstallmentNumber { get; set; }
        public DateTime DueDate { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal FeesAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidPrincipalAmount { get; set; }
        public decimal PaidInterestAmount { get; set; }
        public decimal PaidFeesAmount { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public decimal OutstandingAmount { get; set; }
        public RepaymentStatus Status { get; set; }
        public DateTime? PaidDate { get; set; }
        public int DaysLate { get; set; }
        public decimal LatePaymentFee { get; set; }
        public bool IsFullyPaid => TotalPaidAmount >= TotalAmount;
        
        // Navigation property
        public virtual Loan Loan { get; set; }
    }
    
    public enum RepaymentStatus
    {
        Pending,
        PartiallyPaid,
        FullyPaid,
        Overdue,
        Waived
    }
}