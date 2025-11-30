using System;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class DisbursementSummary
    {
        public string LoanId { get; set; }
        public string SerialNumber { get; set; }
        public decimal Amount { get; set; }
        public DateTime DisbursementDate { get; set; }
        public string Status { get; set; }
    }
}
