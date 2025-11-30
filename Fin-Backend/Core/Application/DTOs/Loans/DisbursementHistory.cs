using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class DisbursementHistory
    {
        public string MemberId { get; set; }
        public int TotalDisbursements { get; set; }
        public decimal TotalAmountDisbursed { get; set; }
        public List<DisbursementSummary> Disbursements { get; set; } = new List<DisbursementSummary>();
    }
}
