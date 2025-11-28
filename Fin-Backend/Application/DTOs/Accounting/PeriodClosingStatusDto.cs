using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Accounting
{
    public class PeriodClosingStatusDto
    {
        public string PeriodId { get; set; }
        public string Status { get; set; }
        public List<string> ValidationMessages { get; set; }
        public bool CanBeClosed { get; set; }
    }
}
