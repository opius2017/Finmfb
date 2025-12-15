using System;
using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.RegulatoryReporting
{
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// <summary>
    /// Field data types
    /// </summary>
    public enum FieldDataType
    {
        /// <summary>
        /// Text field
        /// </summary>
        Text = 1,
        
        /// <summary>
        /// Numeric field
        /// </summary>
        Number = 2,
        
        /// <summary>
        /// Date field
        /// </summary>
        Date = 3,
        
        /// <summary>
        /// Boolean field
        /// </summary>
        Boolean = 4,
        
        /// <summary>
        /// Selection from predefined options
        /// </summary>
        Select = 5,
        
        /// <summary>
        /// Multiple selection from predefined options
        /// </summary>
        MultiSelect = 6,
        
        /// <summary>
        /// Currency amount
        /// </summary>
        Currency = 7,
        
        /// <summary>
        /// Percentage value
        /// </summary>
        Percentage = 8,
        
        /// <summary>
        /// Formula/calculated field
        /// </summary>
        Formula = 9
    }

    /// <summary>
    /// Represents a field in a regulatory report template section
    /// </summary>
    public class RegulatoryReportField : AuditableEntity
    {
        /// <summary>
        /// Reference to the section
        /// </summary>
        public string RegulatoryReportSectionId { get; set; } = string.Empty;
        [NotMapped]
        public string SectionId { get => RegulatoryReportSectionId; set => RegulatoryReportSectionId = value; }
        
        /// <summary>
        /// Navigation property for the section
        /// </summary>
        public virtual RegulatoryReportSection? Section { get; set; }
        
        /// <summary>
        /// Field code
        /// </summary>
        public string FieldCode { get; set; } = string.Empty;
        
        /// <summary>
        /// Field name
        /// </summary>
        public string FieldName { get; set; } = string.Empty;
        
        /// <summary>
        /// Field description
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// Data type of the field
        /// </summary>
        public FieldDataType DataType { get; set; }
        
        /// <summary>
        /// Display order of the field
        /// </summary>
        public int DisplayOrder { get; set; }
        
        /// <summary>
        /// Default value for the field
        /// </summary>
        public string? DefaultValue { get; set; }
        
        /// <summary>
        /// Placeholder text for the field
        /// </summary>
        public string? Placeholder { get; set; }
        
        /// <summary>
        /// Help text for the field
        /// </summary>
        public string? HelpText { get; set; }
        
        /// <summary>
        /// Whether the field is required
        /// </summary>
        public bool IsRequired { get; set; }
        
        /// <summary>
        /// Whether the field is read-only
        /// </summary>
        public bool IsReadOnly { get; set; }
        
        /// <summary>
        /// Whether the field is hidden
        /// </summary>
        public bool IsHidden { get; set; }
        
        /// <summary>
        /// Minimum length/value for validation
        /// </summary>
        public string? MinValue { get; set; }
        
        /// <summary>
        /// Maximum length/value for validation
        /// </summary>
        public string? MaxValue { get; set; }
        
        /// <summary>
        /// Validation pattern (regex)
        /// </summary>
        public string? ValidationPattern { get; set; }
        
        /// <summary>
        /// Validation error message
        /// </summary>
        public string? ValidationMessage { get; set; }
        
        public string? ValidationRules { get; set; } 
        
        /// <summary>
        /// Options for select/multi-select fields in JSON format
        /// </summary>
        public string? Options { get; set; }
        
        /// <summary>
        /// Formula expression for calculated fields
        /// </summary>
        public string? Formula { get; set; }
        
        public string? MappingQuery { get; set; } 
        
        /// <summary>
        /// Field format (e.g., date format, number format)
        /// </summary>
        public string? Format { get; set; }
        
        /// <summary>
        /// Unit of measurement (e.g., NGN, USD, %)
        /// </summary>
        public string? Unit { get; set; }
        
        /// <summary>
        /// Number of decimal places for numeric fields
        /// </summary>
        public int? DecimalPlaces { get; set; }
        
        /// <summary>
        /// Whether to show the field's unit
        /// </summary>
        public bool ShowUnit { get; set; }
        
        /// <summary>
        /// JSON metadata about the field
        /// </summary>
        public string? Metadata { get; set; }
        
        /// <summary>
        /// Row index for table fields (null for non-table fields)
        /// </summary>
        public int? RowIndex { get; set; }
        
        /// <summary>
        /// Column index for table fields (null for non-table fields)
        /// </summary>
        public int? ColumnIndex { get; set; }
        
        /// <summary>
        /// Column span for table fields (default 1)
        /// </summary>
        public int ColumnSpan { get; set; } = 1;
        
        /// <summary>
        /// Row span for table fields (default 1)
        /// </summary>
        public int RowSpan { get; set; } = 1;
    }
}
