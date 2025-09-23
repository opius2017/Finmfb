using System;
using FinTech.Domain.Common;
using System.Collections.Generic;
using FinTech.Domain.Entities.Common;

namespace FinTech.Domain.Entities.RegulatoryReporting
{
    /// <summary>
    /// Represents a section in a regulatory report template
    /// </summary>
    public class RegulatoryReportSection : AuditableEntity
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
        /// Section code
        /// </summary>
        public string SectionCode { get; set; }
        
        /// <summary>
        /// Section name
        /// </summary>
        public string SectionName { get; set; }
        
        /// <summary>
        /// Section description
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Display order of the section
        /// </summary>
        public int DisplayOrder { get; set; }
        
        /// <summary>
        /// Whether the section is a table
        /// </summary>
        public bool IsTable { get; set; }
        
        /// <summary>
        /// Number of rows for table sections (null for dynamic tables)
        /// </summary>
        public int? RowCount { get; set; }
        
        /// <summary>
        /// Instructions for completing the section
        /// </summary>
        public string Instructions { get; set; }
        
        /// <summary>
        /// Whether the section is required
        /// </summary>
        public bool IsRequired { get; set; }
        
        /// <summary>
        /// Whether the section is hidden
        /// </summary>
        public bool IsHidden { get; set; }
        
        /// <summary>
        /// JSON metadata about the section
        /// </summary>
        public string Metadata { get; set; }
        
        /// <summary>
        /// Parent section ID for nested sections (null for top-level sections)
        /// </summary>
        public int? ParentSectionId { get; set; }
        
        /// <summary>
        /// Navigation property for the parent section
        /// </summary>
        public virtual RegulatoryReportSection ParentSection { get; set; }
        
        /// <summary>
        /// Collection of child sections
        /// </summary>
        public virtual ICollection<RegulatoryReportSection> ChildSections { get; set; }
        
        /// <summary>
        /// Collection of fields in this section
        /// </summary>
        public virtual ICollection<RegulatoryReportField> Fields { get; set; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public RegulatoryReportSection()
        {
            ChildSections = new HashSet<RegulatoryReportSection>();
            Fields = new HashSet<RegulatoryReportField>();
        }
    }
}