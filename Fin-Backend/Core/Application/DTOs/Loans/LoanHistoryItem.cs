using System;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class LoanHistoryItem
    {
        public string LoanNumber { get; set; }
        public decimal PrincipalAmount { get; set; }
        public DateTime DisbursementDate { get; set; }
        public DateTime MaturityDate { get; set; }
        public string Status { get; set; }
        public decimal OutstandingBalance { get; set; }
        public int DaysInArrears { get; set; }
        public string Classification { get; set; }
    }
}
