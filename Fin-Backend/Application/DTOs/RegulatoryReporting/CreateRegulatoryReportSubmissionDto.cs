using System;

namespace FinTech.Core.Application.DTOs.RegulatoryReporting
{
    public class CreateRegulatoryReportSubmissionDto
    {
        public Guid ReportTemplateId { get; set; }
        public DateTime ReportingPeriodStart { get; set; }
        public DateTime ReportingPeriodEnd { get; set; }
        public Guid SubmittedById { get; set; }
        public string Comments { get; set; }
    }
}
