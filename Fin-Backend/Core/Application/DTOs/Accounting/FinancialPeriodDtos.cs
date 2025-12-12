using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Accounting
{
    public class FinancialPeriodDto
    {
        public string Id { get; set; }
        public string PeriodCode { get; set; }
        public string PeriodName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsClosed { get; set; }
        public DateTime? ClosedDate { get; set; }
        public string ClosedBy { get; set; }
        public int FiscalYear { get; set; }
        public int FiscalMonth { get; set; }
        public bool IsAdjustmentPeriod { get; set; }
    }

    public class ClosePeriodRequestDto
    {
        public string PeriodId { get; set; }
        public bool ForceClose { get; set; }
    }

    public class PeriodClosingStatusDto
    {
        public string PeriodId { get; set; }
        public string Status { get; set; }
        public List<string> ValidationMessages { get; set; }
        public bool CanBeClosed { get; set; }
    }

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
