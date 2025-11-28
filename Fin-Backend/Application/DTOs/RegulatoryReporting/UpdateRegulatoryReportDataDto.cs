using System;

namespace FinTech.Core.Application.DTOs.RegulatoryReporting
{
    public class UpdateRegulatoryReportDataDto
    {
        public Guid Id { get; set; }
        public string Value { get; set; }
        public string Comments { get; set; }
        public bool HasException { get; set; }
        public string ExceptionReason { get; set; }
    }
}
