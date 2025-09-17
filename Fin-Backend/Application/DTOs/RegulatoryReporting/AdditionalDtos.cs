using System;

namespace FinTech.Core.Application.DTOs.RegulatoryReporting
{
    public class InitiateReportSubmissionDto
    {
        public Guid TemplateId { get; set; }
        public DateTime ReportingPeriodStart { get; set; }
        public DateTime ReportingPeriodEnd { get; set; }
    }

    public class ApproveReportDto
    {
        public Guid ApproverId { get; set; }
        public string Comments { get; set; }
    }

    public class RejectReportDto
    {
        public string Reason { get; set; }
    }
}