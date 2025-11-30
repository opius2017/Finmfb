using System;

namespace FinTech.Core.Application.DTOs.Loans
{
    /// <summary>
    /// Represents a single installment in the amortization schedule
    /// </summary>
    public class AmortizationScheduleItem
    {
        public int InstallmentNumber { get; set; }
        public DateTime DueDate { get; set; }
        public decimal EMI { get; set; }
        public decimal PrincipalPayment { get; set; }
        public decimal InterestPayment { get; set; }
        public decimal RemainingBalance { get; set; }
        public string Status { get; set; } // Pending, Paid, Overdue
        public DateTime? PaidDate { get; set; }
        public decimal? ActualAmountPaid { get; set; }
    }
}
