using System;

namespace FinTech.Core.Application.DTOs.Accounting
{
    public class PeriodClosingSummaryDto
    {
        public string PeriodId { get; set; }
        public string PeriodCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalDebits { get; set; }
        public decimal TotalCredits { get; set; }
        public bool IsBalanced { get; set; }
    }
}
