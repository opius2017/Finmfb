using System;
using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.RegulatoryReporting
{
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// <summary>
    /// Represents a validation result for a regulatory report
    /// </summary>
    public class RegulatoryReportValidation : AuditableEntity
    {
        /// <summary>
        /// Reference to the report submission
        /// </summary>
        public string RegulatoryReportSubmissionId { get; set; } = string.Empty;
        [NotMapped]
        public string SubmissionId { get => RegulatoryReportSubmissionId; set => RegulatoryReportSubmissionId = value; }
        
        /// <summary>
        /// Navigation property for the report submission
        /// </summary>
        public virtual RegulatoryReportSubmission? Submission { get; set; }
        
        /// <summary>
        /// Reference to the specific data point (if applicable)
        /// </summary>
        public string? RegulatoryReportDataId { get; set; }
        
        [NotMapped]
        public string? FieldId { get; set; } 
        
        public string? ErrorCode { get; set; } 
        
        /// <summary>
        /// Navigation property for the report data
        /// </summary>
        public virtual RegulatoryReportData? ReportData { get; set; }
        
        /// <summary>
        /// Validation rule code
        /// </summary>
        public string RuleCode { get; set; } = string.Empty;
        
        /// <summary>
        /// Validation rule description
        /// </summary>
        public string? RuleDescription { get; set; }
        
        /// <summary>
        /// Section code the validation applies to
        /// </summary>
        public string? SectionCode { get; set; }
        
        /// <summary>
        /// Field code the validation applies to
        /// </summary>
        public string? FieldCode { get; set; }
        
        [ForeignKey("FieldId")]
        public virtual RegulatoryReportField? Field { get; set; }
        
        /// <summary>
        /// Severity of the validation issue
        /// </summary>
        public ValidationSeverity Severity { get; set; }
        
        /// <summary>
        /// Validation message
        /// </summary>
        public string Message { get; set; } = string.Empty;
        
        [NotMapped]
        public string ErrorMessage { get => Message; set => Message = value; }
        
        /// <summary>
        /// Expected value or condition
        /// </summary>
        public string? ExpectedValue { get; set; }
        
        /// <summary>
        /// Actual value that triggered the validation
        /// </summary>
        public string? ActualValue { get; set; }
        
        /// <summary>
        /// Date and time when the validation was performed
        /// </summary>
        public DateTime ValidationTimestamp { get; set; }
        
        /// <summary>
        /// Whether the validation issue has been resolved
        /// </summary>
        public bool IsResolved { get; set; }
        
        /// <summary>
        /// Date and time when the issue was resolved
        /// </summary>
        public DateTime? ResolvedTimestamp { get; set; }
        
        /// <summary>
        /// Comments about the resolution
        /// </summary>
        public string? ResolutionComments { get; set; }
    }
}
