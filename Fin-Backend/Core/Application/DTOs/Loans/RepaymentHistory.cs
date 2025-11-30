using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class RepaymentHistory
    {
        public string LoanId { get; set; }
        public string LoanSerialNumber { get; set; }
        public int TotalRepayments { get; set; }
        public decimal TotalAmountPaid { get; set; }
        public decimal TotalPrincipalPaid { get; set; }
        public decimal TotalInterestPaid { get; set; }
        public decimal RemainingBalance { get; set; }
        public List<RepaymentTransaction> Transactions { get; set; } = new List<RepaymentTransaction>();
    }
}
