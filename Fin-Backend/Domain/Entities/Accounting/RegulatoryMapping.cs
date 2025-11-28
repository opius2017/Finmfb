using System;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Accounting
{
    /// <summary>
    /// Represents a mapping between a Chart of Account and a Regulatory Code
    /// for regulatory reporting purposes
    /// </summary>
    public class RegulatoryMapping : BaseEntity
    {
        /// <summary>
        /// The ID of the chart of account
        /// </summary>
        public string ChartOfAccountId { get; private set; }
        
        /// <summary>
        /// Navigation property for the Chart of Account
        /// </summary>
        public ChartOfAccount ChartOfAccount { get; private set; }
        
        /// <summary>
        /// The ID of the regulatory code
        /// </summary>
        public string RegulatoryCodeId { get; private set; }
        
        /// <summary>
        /// Navigation property for the Regulatory Code
        /// </summary>
        public RegulatoryCode RegulatoryCode { get; private set; }
        
        /// <summary>
        /// The mapping weight, used for proportional distribution when an account maps to multiple regulatory codes
        /// </summary>
        public decimal MappingWeight { get; private set; }
        
        /// <summary>
        /// Notes or additional information about this mapping
        /// </summary>
        public string Notes { get; private set; }
        
        /// <summary>
        /// Whether this mapping is currently active
        /// </summary>
        public bool IsActive { get; private set; }
        
        /// <summary>
        /// The date this mapping became effective
        /// </summary>
        public DateTime EffectiveDate { get; private set; }
        
        /// <summary>
        /// The date this mapping expires (if applicable)
        /// </summary>
        public DateTime? ExpiryDate { get; private set; }
        
        // Private constructor for EF
        private RegulatoryMapping()
        {
        }
        
        /// <summary>
        /// Creates a new regulatory mapping
        /// </summary>
        public RegulatoryMapping(
            string chartOfAccountId,
            string regulatoryCodeId,
            decimal mappingWeight = 1.0m,
            string notes = null,
            DateTime? effectiveDate = null)
        {
            if (string.IsNullOrWhiteSpace(chartOfAccountId))
                throw new ArgumentException("Chart of account ID cannot be empty", nameof(chartOfAccountId));
                
            if (string.IsNullOrWhiteSpace(regulatoryCodeId))
                throw new ArgumentException("Regulatory code ID cannot be empty", nameof(regulatoryCodeId));
                
            if (mappingWeight <= 0 || mappingWeight > 1)
                throw new ArgumentException("Mapping weight must be between 0 and 1", nameof(mappingWeight));
            
            ChartOfAccountId = chartOfAccountId;
            RegulatoryCodeId = regulatoryCodeId;
            MappingWeight = mappingWeight;
            Notes = notes;
            IsActive = true;
            EffectiveDate = effectiveDate ?? DateTime.UtcNow;
        }
        
        /// <summary>
        /// Updates the regulatory mapping information
        /// </summary>
        public void Update(
            decimal mappingWeight,
            string notes = null,
            DateTime? expiryDate = null)
        {
            if (mappingWeight <= 0 || mappingWeight > 1)
                throw new ArgumentException("Mapping weight must be between 0 and 1", nameof(mappingWeight));
                
            MappingWeight = mappingWeight;
            Notes = notes;
            
            if (expiryDate.HasValue && expiryDate.Value <= EffectiveDate)
                throw new ArgumentException("Expiry date must be after effective date", nameof(expiryDate));
                
            ExpiryDate = expiryDate;
        }
        
        /// <summary>
        /// Activates the regulatory mapping
        /// </summary>
        public void Activate()
        {
            IsActive = true;
        }
        
        /// <summary>
        /// Deactivates the regulatory mapping
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
        }
    }
}
