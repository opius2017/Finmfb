using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.GeneralLedger.Period
{
    public class FiscalYearDto
    {
        public string Id { get; set; }
        public int Year { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsClosed { get; set; }
        public DateTime? ClosedDate { get; set; }
        public string ClosedBy { get; set; }
        public bool IsCurrentYear { get; set; }
        public IEnumerable<FinancialPeriodDto> Periods { get; set; }
    }
}