using System;

namespace FinTech.Core.Application.DTOs.RegulatoryReporting
{
    public class CreateRegulatoryReportScheduleDto
    {
        public Guid ReportTemplateId { get; set; }
        public DateTime NextGenerationDate { get; set; }
        public DateTime NextSubmissionDeadline { get; set; }
        public bool IsAutoGenerate { get; set; }
        public bool IsAutoSubmit { get; set; }
        public string NotificationEmails { get; set; }
        public int ReminderDays { get; set; }
    }
}
