using System;
using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.RegulatoryReporting
{
    /// <summary>
    /// Regulatory bodies that require financial reports
    /// </summary>
    public enum RegulatoryBody
    {
        /// <summary>
        /// Central Bank of Nigeria
        /// </summary>
        CBN = 1,
        
        /// <summary>
        /// Nigeria Deposit Insurance Corporation
        /// </summary>
        NDIC = 2,
        
        /// <summary>
        /// Federal Inland Revenue Service
        /// </summary>
        FIRS = 3,
        
        /// <summary>
        /// Corporate Affairs Commission
        /// </summary>
        CAC = 4,
        
        /// <summary>
        /// Securities and Exchange Commission
        /// </summary>
        SEC = 5,
        
        /// <summary>
        /// Financial Reporting Council of Nigeria
        /// </summary>
        FRCN = 6,
        
        /// <summary>
        /// Other regulatory body
        /// </summary>
        Other = 99
    }

    /// <summary>
    /// Reporting frequencies for regulatory reports
    /// </summary>
    public enum ReportingFrequency
    {
        /// <summary>
        /// Daily reporting
        /// </summary>
        Daily = 1,
        
        /// <summary>
        /// Weekly reporting
        /// </summary>
        Weekly = 2,
        
        /// <summary>
        /// Monthly reporting
        /// </summary>
        Monthly = 3,
        
        /// <summary>
        /// Quarterly reporting
        /// </summary>
        Quarterly = 4,
        
        /// <summary>
        /// Semi-annual reporting
        /// </summary>
        SemiAnnual = 5,
        
        /// <summary>
        /// Annual reporting
        /// </summary>
        Annual = 6,
        
        /// <summary>
        /// Ad-hoc reporting (no set schedule)
        /// </summary>
        AdHoc = 7
    }

    /// <summary>
    /// Status of a report submission
    /// </summary>
    public enum SubmissionStatus
    {
        /// <summary>
        /// Report is being drafted
        /// </summary>
        Draft = 1,
        
        /// <summary>
        /// Report is pending review
        /// </summary>
        PendingReview = 2,
        
        /// <summary>
        /// Report has been reviewed and returned for corrections
        /// </summary>
        ReturnedForCorrections = 3,
        
        /// <summary>
        /// Report has been approved for submission
        /// </summary>
        Approved = 4,
        
        /// <summary>
        /// Report has been submitted to the regulatory body
        /// </summary>
        Submitted = 5,
        
        /// <summary>
        /// Report submission was acknowledged by the regulatory body
        /// </summary>
        Acknowledged = 6,
        
        /// <summary>
        /// Report was rejected by the regulatory body
        /// </summary>
        Rejected = 7
    }

    /// <summary>
    /// Severity levels for validation issues
    /// </summary>
    public enum ValidationSeverity
    {
        /// <summary>
        /// Information only, no action required
        /// </summary>
        Info = 1,
        
        /// <summary>
        /// Warning that should be reviewed but doesn't block submission
        /// </summary>
        Warning = 2,
        
        /// <summary>
        /// Error that must be fixed before submission
        /// </summary>
        Error = 3,
        
        /// <summary>
        /// Critical error that must be fixed before submission
        /// </summary>
        Critical = 4
    }

    /// <summary>
    /// Represents a regulatory report template
    /// </summary>
    public class RegulatoryReportTemplate : AuditableEntity
    {
        /// <summary>
        /// Name of the report template
        /// </summary>
        public string TemplateName { get; set; }
        
        /// <summary>
        /// Unique code for the report template
        /// </summary>
        public string TemplateCode { get; set; }
        
        /// <summary>
        /// Description of the report template
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Regulatory body that requires this report
        /// </summary>
        public RegulatoryBody RegulatoryBody { get; set; }
        
        /// <summary>
        /// Frequency of report submission
        /// </summary>
        public ReportingFrequency Frequency { get; set; }
        
        /// <summary>
        /// File format for report submission (e.g., XML, XBRL, CSV, Excel)
        /// </summary>
        public string FileFormat { get; set; }
        
        /// <summary>
        /// Whether the template is active
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Version of the report schema
        /// </summary>
        public string SchemaVersion { get; set; }
        
        /// <summary>
        /// Template structure in JSON format
        /// </summary>
        public string TemplateStructure { get; set; }
    }
}
