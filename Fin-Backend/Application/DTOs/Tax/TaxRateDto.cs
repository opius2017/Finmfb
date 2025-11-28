using System;

namespace FinTech.Core.Application.DTOs.Tax
{
    /// <summary>
    /// DTO for tax rate information
    /// </summary>
    public class TaxRateDto
    {
        /// <summary>
        /// Unique identifier for the tax rate
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Tax type ID this rate belongs to
        /// </summary>
        public string TaxTypeId { get; set; }
        
        /// <summary>
        /// Tax type code (e.g., VAT, WHT)
        /// </summary>
        public string TaxTypeCode { get; set; }
        
        /// <summary>
        /// Rate name or description
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Rate percentage value
        /// </summary>
        public decimal Rate { get; set; }
        
        /// <summary>
        /// Minimum taxable amount for this rate
        /// </summary>
        public decimal? MinimumAmount { get; set; }
        
        /// <summary>
        /// Maximum taxable amount for this rate
        /// </summary>
        public decimal? MaximumAmount { get; set; }
        
        /// <summary>
        /// Effective date when this rate becomes valid
        /// </summary>
        public DateTime EffectiveDate { get; set; }
        
        /// <summary>
        /// End date when this rate expires (null for indefinite)
        /// </summary>
        public DateTime? EndDate { get; set; }
        
        /// <summary>
        /// Whether this rate is currently active
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Category or type of items this rate applies to
        /// </summary>
        public string ApplicableCategory { get; set; }
    }
}
