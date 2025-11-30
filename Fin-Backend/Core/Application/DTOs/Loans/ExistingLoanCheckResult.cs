using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class ExistingLoanCheckResult
    {
        public bool IsEligible { get; set; }
        public List<string> Reasons { get; set; } = new List<string>();
        
        public int ActiveLoansCount { get; set; }
        public decimal TotalOutstandingBalance { get; set; }
        public int MaxActiveLoans { get; set; }
        public decimal MaxTotalOutstanding { get; set; }
    }
}
