using System;
using FinTech.Core.Domain.Entities.RegulatoryReporting;

namespace FinTech.Core.Application.DTOs.RegulatoryReporting
{
    public class GenerateReportRequestDto
    {
        public RegulatoryBody RegulatoryBody { get; set; }
        public string ReportType { get; set; }
        public DateTime ReportingPeriod { get; set; }
    }
}
