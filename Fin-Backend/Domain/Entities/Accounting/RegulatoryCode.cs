using System;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Accounting
{
    /// <summary>
    /// Represents a regulatory reporting code for Central Bank of Nigeria (CBN)
    /// </summary>
    public class RegulatoryCode : BaseEntity
    {
        /// <summary>
        /// The unique code assigned by the regulatory authority (e.g., CBN)
        /// </summary>
        public string Code { get; private set; }
        
        /// <summary>
        /// The description of the regulatory code
        /// </summary>
        public string Description { get; private set; }
        
        /// <summary>
        /// The category of the regulatory code (e.g., Asset, Liability, Income, Expense)
        /// </summary>
        public string Category { get; private set; }
        
        /// <summary>
        /// The sub-category of the regulatory code
        /// </summary>
        public string? SubCategory { get; private set; }
        
        /// <summary>
        /// The reporting authority that defined this code (e.g., CBN, NDIC, etc.)
        /// </summary>
        public string Authority { get; private set; }
        
        /// <summary>
        /// The reporting form this code appears on (e.g., MCFPR 300, etc.)
        /// </summary>
        public string ReportingForm { get; private set; }
        
        /// <summary>
        /// Whether this code is currently active
        /// </summary>
        public bool IsActive { get; private set; }
        
        /// <summary>
        /// The version of the regulatory reporting framework this code belongs to
        /// </summary>
        public string? Version { get; private set; }
        
        /// <summary>
        /// The date this regulatory code became effective
        /// </summary>
        public DateTime EffectiveDate { get; private set; }
        
        /// <summary>
        /// Notes or additional information about this regulatory code
        /// </summary>
        public string? Notes { get; private set; }
        
        // Private constructor for EF
        private RegulatoryCode()
        {
            Code = string.Empty;
            Description = string.Empty;
            Category = string.Empty;
            Authority = string.Empty;
            ReportingForm = string.Empty;
        }
        
        /// <summary>
        /// Creates a new regulatory code
        /// </summary>
        public RegulatoryCode(
            string code,
            string description,
            string category,
            string authority,
            string reportingForm,
            DateTime effectiveDate,
            string? subCategory = null,
            string? version = "1.0",
            string? notes = null)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Regulatory code cannot be empty", nameof(code));
                
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description cannot be empty", nameof(description));
                
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Category cannot be empty", nameof(category));
                
            if (string.IsNullOrWhiteSpace(authority))
                throw new ArgumentException("Authority cannot be empty", nameof(authority));
                
            if (string.IsNullOrWhiteSpace(reportingForm))
                throw new ArgumentException("Reporting form cannot be empty", nameof(reportingForm));
            
            Code = code;
            Description = description;
            Category = category;
            SubCategory = subCategory;
            Authority = authority;
            ReportingForm = reportingForm;
            IsActive = true;
            Version = version;
            EffectiveDate = effectiveDate;
            Notes = notes;
        }
        
        /// <summary>
        /// Updates the regulatory code information
        /// </summary>
        public void Update(
            string description,
            string category,
            string authority,
            string reportingForm,
            string? subCategory = null,
            string? version = null,
            string? notes = null)
        {
            if (!string.IsNullOrWhiteSpace(description))
                Description = description;
                
            if (!string.IsNullOrWhiteSpace(category))
                Category = category;
                
            if (!string.IsNullOrWhiteSpace(authority))
                Authority = authority;
                
            if (!string.IsNullOrWhiteSpace(reportingForm))
                ReportingForm = reportingForm;
                
            SubCategory = subCategory;
            
            if (!string.IsNullOrWhiteSpace(version))
                Version = version;
                
            Notes = notes;
        }
        
        /// <summary>
        /// Activates the regulatory code
        /// </summary>
        public void Activate()
        {
            IsActive = true;
        }
        
        /// <summary>
        /// Deactivates the regulatory code
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
        }
    }
}
