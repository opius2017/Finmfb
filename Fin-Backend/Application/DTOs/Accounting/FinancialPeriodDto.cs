using System;

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
}
