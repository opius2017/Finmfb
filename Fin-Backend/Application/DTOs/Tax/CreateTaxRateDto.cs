using System;

namespace FinTech.Core.Application.DTOs.Tax
{
    /// <summary>
    /// DTO for creating a new tax rate
    /// </summary>
    public class CreateTaxRateDto
    {
        /// <summary>
        /// Tax type ID this rate belongs to
        /// </summary>
        public string TaxTypeId { get; set; }
        
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
        /// Category or type of items this rate applies to
        /// </summary>
        public string ApplicableCategory { get; set; }
    }
}
