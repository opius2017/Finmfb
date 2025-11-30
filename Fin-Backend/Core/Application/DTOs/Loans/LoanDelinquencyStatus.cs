using System;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class LoanDelinquencyStatus
    {
        public string LoanId { get; set; }
        public string LoanNumber { get; set; }
        public DateTime CheckDate { get; set; }
        public bool IsOverdue { get; set; }
        public int DaysOverdue { get; set; }
        public decimal OverdueAmount { get; set; }
        public decimal PenaltyAmount { get; set; }
        public string Classification { get; set; }
        public bool IsNewDelinquency { get; set; }
        public bool PenaltyApplied { get; set; }
        public bool ClassificationChanged { get; set; }
    }
}
