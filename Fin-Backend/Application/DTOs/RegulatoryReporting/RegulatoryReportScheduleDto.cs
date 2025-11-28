using System;
using FinTech.Core.Domain.Entities.RegulatoryReporting;

namespace FinTech.Core.Application.DTOs.RegulatoryReporting
{
    public class RegulatoryReportScheduleDto
    {
        public Guid Id { get; set; }
        public Guid ReportTemplateId { get; set; }
        public string TemplateName { get; set; }
        public string TemplateCode { get; set; }
        public RegulatoryBody RegulatoryBody { get; set; }
        public string RegulatoryBodyName => RegulatoryBody.ToString();
        public ReportingFrequency Frequency { get; set; }
        public string FrequencyName => Frequency.ToString();
        public DateTime NextGenerationDate { get; set; }
        public DateTime NextSubmissionDeadline { get; set; }
        public bool IsAutoGenerate { get; set; }
        public bool IsAutoSubmit { get; set; }
        public string NotificationEmails { get; set; }
        public int ReminderDays { get; set; }
        public int DaysUntilDeadline => (int)(NextSubmissionDeadline - DateTime.UtcNow).TotalDays;
    }
}
