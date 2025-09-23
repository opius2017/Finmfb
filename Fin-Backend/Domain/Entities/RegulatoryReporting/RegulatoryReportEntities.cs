using FinTech.Domain.Entities.Common;
using System;
using System.Collections.Generic;

namespace FinTech.Domain.Entities.RegulatoryReporting
{
    /// <summary>
    /// Represents a regulatory report template for various regulatory bodies (CBN, NDIC, FIRS)
    /// </summary>
    public class RegulatoryReportTemplate : BaseEntity
    {
        public string TemplateName { get; set; }
        public string TemplateCode { get; set; }
        public string Description { get; set; }
        public RegulatoryBody RegulatoryBody { get; set; }
        public ReportingFrequency Frequency { get; set; }
        public string FileFormat { get; set; } // XML, Excel, PDF, etc.
        public bool IsActive { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string SchemaVersion { get; set; }
        public string TemplateStructure { get; set; } // JSON structure of the template
        
        // Navigation properties
        public virtual ICollection<RegulatoryReportSection> Sections { get; set; }
        public virtual ICollection<RegulatoryReportSubmission> Submissions { get; set; }
    }

    /// <summary>
    /// Represents a section within a regulatory report template
    /// </summary>
    public class RegulatoryReportSection : BaseEntity
    {
        public Guid ReportTemplateId { get; set; }
        public string SectionName { get; set; }
        public string SectionCode { get; set; }
        public string Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsRequired { get; set; }
        public string ValidationRules { get; set; } // JSON validation rules
        
        // Navigation properties
        public virtual RegulatoryReportTemplate ReportTemplate { get; set; }
        public virtual ICollection<RegulatoryReportField> Fields { get; set; }
    }

    /// <summary>
    /// Represents a field within a regulatory report section
    /// </summary>
    public class RegulatoryReportField : BaseEntity
    {
        public Guid SectionId { get; set; }
        public string FieldName { get; set; }
        public string FieldCode { get; set; }
        public string Description { get; set; }
        public string DataType { get; set; } // String, Numeric, Date, Boolean, etc.
        public bool IsRequired { get; set; }
        public string ValidationRules { get; set; } // JSON validation rules
        public int DisplayOrder { get; set; }
        public string DefaultValue { get; set; }
        public string Formula { get; set; } // For calculated fields
        public string MappingQuery { get; set; } // SQL or LINQ query to map from system data
        
        // Navigation properties
        public virtual RegulatoryReportSection Section { get; set; }
    }

    /// <summary>
    /// Represents a submission of a regulatory report
    /// </summary>
    public class RegulatoryReportSubmission : BaseEntity
    {
        public Guid ReportTemplateId { get; set; }
        public DateTime ReportingPeriodStart { get; set; }
        public DateTime ReportingPeriodEnd { get; set; }
        public DateTime SubmissionDate { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public Guid SubmittedById { get; set; }
        public Guid? ApprovedById { get; set; }
        public SubmissionStatus Status { get; set; }
        public string Comments { get; set; }
        public string SubmissionReference { get; set; } // Reference number from regulatory body
        public string FilePath { get; set; } // Path to the generated report file
        
        // Navigation properties
        public virtual RegulatoryReportTemplate ReportTemplate { get; set; }
        public virtual ICollection<RegulatoryReportData> ReportData { get; set; }
    }

    /// <summary>
    /// Represents data values for a specific submission
    /// </summary>
    public class RegulatoryReportData : BaseEntity
    {
        public Guid SubmissionId { get; set; }
        public Guid FieldId { get; set; }
        public string Value { get; set; }
        public bool IsCalculated { get; set; }
        public string Comments { get; set; }
        public bool HasException { get; set; }
        public string ExceptionReason { get; set; }
        
        // Navigation properties
        public virtual RegulatoryReportSubmission Submission { get; set; }
        public virtual RegulatoryReportField Field { get; set; }
    }

    /// <summary>
    /// Represents a validation error for a report submission
    /// </summary>
    public class RegulatoryReportValidation : BaseEntity
    {
        public Guid SubmissionId { get; set; }
        public Guid? FieldId { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorCode { get; set; }
        public ValidationSeverity Severity { get; set; }
        public bool IsResolved { get; set; }
        public string ResolutionComments { get; set; }
        
        // Navigation properties
        public virtual RegulatoryReportSubmission Submission { get; set; }
        public virtual RegulatoryReportField Field { get; set; }
    }

    /// <summary>
    /// Represents a schedule for generating and submitting reports
    /// </summary>
    public class RegulatoryReportSchedule : BaseEntity
    {
        public Guid ReportTemplateId { get; set; }
        public DateTime NextGenerationDate { get; set; }
        public DateTime NextSubmissionDeadline { get; set; }
        public bool IsAutoGenerate { get; set; }
        public bool IsAutoSubmit { get; set; }
        public string NotificationEmails { get; set; }
        public int ReminderDays { get; set; } // Days before deadline to send reminder
        
        // Navigation properties
        public virtual RegulatoryReportTemplate ReportTemplate { get; set; }
    }

    public enum RegulatoryBody
    {
        CBN, // Central Bank of Nigeria
        NDIC, // Nigeria Deposit Insurance Corporation
        FIRS, // Federal Inland Revenue Service
        SEC, // Securities and Exchange Commission
        CAC, // Corporate Affairs Commission
        Other
    }

    public enum ReportingFrequency
    {
        Daily,
        Weekly,
        Monthly,
        Quarterly,
        BiAnnually,
        Annually,
        OnDemand
    }

    public enum SubmissionStatus
    {
        Draft,
        PendingApproval,
        Approved,
        Submitted,
        Accepted,
        Rejected,
        ReSubmissionRequired
    }

    public enum ValidationSeverity
    {
        Error,
        Warning,
        Info
    }
}