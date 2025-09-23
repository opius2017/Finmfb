using System;
using FinTech.Domain.Common;

namespace FinTech.Domain.Entities.RegulatoryReporting
{
    /// <summary>
    /// Represents a scheduled regulatory report
    /// </summary>
    using FinTech.Domain.Entities.Common;
    public class RegulatoryReportSchedule : AuditableEntity
    {
        /// <summary>
        /// Reference to the report template
        /// </summary>
        public int RegulatoryReportTemplateId { get; set; }
        
        /// <summary>
        /// Navigation property for the report template
        /// </summary>
        public virtual RegulatoryReportTemplate Template { get; set; }
        
        /// <summary>
        /// Name of the schedule
        /// </summary>
        public string ScheduleName { get; set; }
        
        /// <summary>
        /// Description of the schedule
        /// </summary>
        public string Description { get; set; }
        
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
        
        /// <summary>
        /// CRON expression for complex schedules
        /// </summary>
        public string CronExpression { get; set; }
        
        /// <summary>
        /// Comma-separated list of email addresses to notify
        /// </summary>
        public string NotificationEmails { get; set; }
    }
}