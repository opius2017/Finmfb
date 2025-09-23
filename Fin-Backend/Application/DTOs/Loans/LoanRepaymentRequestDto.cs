using System;

namespace FinTech.Application.DTOs.Loans
{
    public class LoanRepaymentRequestDto
    {
        public decimal Amount { get; set; }
        public DateTime RepaymentDate { get; set; }
        public string LoanAccountNumber { get; set; } = string.Empty;
    }
}
