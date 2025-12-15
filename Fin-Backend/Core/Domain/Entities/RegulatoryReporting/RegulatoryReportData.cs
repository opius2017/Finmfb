using System;
using System.Collections.Generic;
using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTech.Core.Domain.Entities.RegulatoryReporting
{
    /// <summary>
    /// Represents data for a regulatory report submission
    /// </summary>
    public class RegulatoryReportData : AuditableEntity
    {
        /// <summary>
        /// Reference to the report submission
        /// </summary>
        public string RegulatoryReportSubmissionId { get; set; } = string.Empty;
        
        [NotMapped]
        public string SubmissionId { get => RegulatoryReportSubmissionId; set => RegulatoryReportSubmissionId = value; }
        
        public string? ExceptionReason { get; set; }
        
        /// <summary>
        /// Navigation property for the report submission
        /// </summary>
        public virtual RegulatoryReportSubmission? Submission { get; set; }
        
        /// <summary>
        /// Section code from the template
        /// </summary>
        public string SectionCode { get; set; } = string.Empty;
        
        /// <summary>
        /// Section name from the template
        /// </summary>
        public string SectionName { get; set; } = string.Empty;
        
        /// <summary>
        /// Field code from the template
        /// </summary>
        public string FieldCode { get; set; } = string.Empty;
        
        /// <summary>
        /// Field name from the template
        /// </summary>
        public string FieldName { get; set; } = string.Empty;
        
        public string FieldId { get; set; } = string.Empty;
        public virtual RegulatoryReportField? Field { get; set; }
        
        /// <summary>
        /// Row index for tabular data
        /// </summary>
        public int? RowIndex { get; set; }
        
        /// <summary>
        /// Column index for tabular data
        /// </summary>
        public int? ColumnIndex { get; set; }
        
        /// <summary>
        /// Raw data value in string format
        /// </summary>
        public string RawValue { get; set; } = string.Empty;
        [NotMapped]
        public string Value { get => RawValue; set => RawValue = value; }
        
        /// <summary>
        /// Data type of the value (e.g., string, number, date, boolean)
        /// </summary>
        public string DataType { get; set; } = string.Empty;
        
        /// <summary>
        /// Numeric value for calculations
        /// </summary>
        public decimal? NumericValue { get; set; }
        
        /// <summary>
        /// Date value for date fields
        /// </summary>
        public DateTime? DateValue { get; set; }
        
        /// <summary>
        /// Whether the field is calculated or manually entered
        /// </summary>
        public bool IsCalculated { get; set; }
        
        /// <summary>
        /// Calculation formula if applicable
        /// </summary>
        public string? CalculationFormula { get; set; }
        
        /// <summary>
        /// JSON metadata about the field
        /// </summary>
        public string? Metadata { get; set; }
        
        /// <summary>
        /// Comments about this specific data point
        /// </summary>
        public string? Comments { get; set; }
        
        /// <summary>
        /// Whether this data point has validation errors
        /// </summary>
        public bool HasValidationErrors { get; set; }
        [NotMapped]
        public bool HasException { get => HasValidationErrors; set => HasValidationErrors = value; }
    }
}
