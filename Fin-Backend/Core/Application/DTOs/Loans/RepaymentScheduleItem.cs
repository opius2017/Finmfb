using System;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class RepaymentScheduleItem
    {
        public int InstallmentNumber { get; set; }
        public DateTime DueDate { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime? PaidDate { get; set; }
        public decimal? AmountPaid { get; set; }
    }
}
