using System;
using FinTech.Core.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTech.Core.Domain.Entities.RegulatoryReporting
{
    /// <summary>
    /// Represents a scheduled regulatory report
    /// </summary>
    using FinTech.Core.Domain.Entities.Common;
    public class RegulatoryReportSchedule : AuditableEntity
    {
        /// <summary>
        /// Reference to the report template
        /// </summary>
        public string RegulatoryReportTemplateId { get; set; } = string.Empty;
        [NotMapped]
        public string ReportTemplateId { get => RegulatoryReportTemplateId; set => RegulatoryReportTemplateId = value; }
        
        /// <summary>
        /// Navigation property for the report template
        /// </summary>
        public virtual RegulatoryReportTemplate? Template { get; set; }
        [NotMapped]
        public virtual RegulatoryReportTemplate? ReportTemplate { get => Template; set => Template = value; }
        
        /// <summary>
        /// Name of the schedule
        /// </summary>
        public string ScheduleName { get; set; } = string.Empty;
        
        /// <summary>
        /// Description of the schedule
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// Frequency of the schedule
        /// </summary>
        public ReportingFrequency Frequency { get; set; }
        
        /// <summary>
        /// Start date of the schedule
        /// </summary>
        public DateTime StartDate { get; set; }
        
        /// <summary>
        /// End date of the schedule (null for indefinite)
        /// </summary>
        public DateTime? EndDate { get; set; }
        
        /// <summary>
        /// Days before due date to start preparation
        /// </summary>
        public int PreparationLeadTimeDays { get; set; }
        
        /// <summary>
        /// Days before due date to send first reminder
        /// </summary>
        public int FirstReminderDays { get; set; }
        
        /// <summary>
        /// Days before due date to send second reminder
        /// </summary>
        public int SecondReminderDays { get; set; }
        
        /// <summary>
        /// Days before due date to send escalation
        /// </summary>
        public int EscalationDays { get; set; }
        
        /// <summary>
        /// Whether the schedule is active
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Last execution date
        /// </summary>
        public DateTime? LastExecutionDate { get; set; }
        
        /// <summary>
        /// Next execution date
        /// </summary>
        public DateTime? NextExecutionDate { get; set; }
        [NotMapped]
        public DateTime? NextGenerationDate { get => NextExecutionDate; set => NextExecutionDate = value; }
        
        public DateTime? NextSubmissionDeadline { get; set; }
        
        /// <summary>
        /// CRON expression for complex schedules
        /// </summary>
        public string? CronExpression { get; set; }
        
        /// <summary>
        /// Comma-separated list of email addresses to notify
        /// </summary>
        public string? NotificationEmails { get; set; }
    }
}
