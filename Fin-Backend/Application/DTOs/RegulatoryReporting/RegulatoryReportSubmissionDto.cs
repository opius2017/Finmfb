using System;
using FinTech.Core.Domain.Entities.RegulatoryReporting;

namespace FinTech.Core.Application.DTOs.RegulatoryReporting
{
    public class RegulatoryReportSubmissionDto
    {
        public Guid Id { get; set; }
        public Guid ReportTemplateId { get; set; }
        public string TemplateName { get; set; }
        public string TemplateCode { get; set; }
        public RegulatoryBody RegulatoryBody { get; set; }
        public string RegulatoryBodyName => RegulatoryBody.ToString();
        public DateTime ReportingPeriodStart { get; set; }
        public DateTime ReportingPeriodEnd { get; set; }
        public DateTime SubmissionDate { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public Guid SubmittedById { get; set; }
        public string SubmittedByName { get; set; }
        public Guid? ApprovedById { get; set; }
        public string ApprovedByName { get; set; }
        public SubmissionStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public string Comments { get; set; }
        public string SubmissionReference { get; set; }
        public string FilePath { get; set; }
        public int ValidationErrorCount { get; set; }
        public int ValidationWarningCount { get; set; }
    }
}
