using System;
using FinTech.Domain.Common;

namespace FinTech.Domain.Entities.Tax
{
    /// <summary>
    /// Represents a tax rate for a specific tax type
    /// </summary>
    public class TaxRate : AuditableEntity
    {
        /// <summary>
        /// The ID of the tax type this rate belongs to
        /// </summary>
        public string TaxTypeId { get; set; }
        
        /// <summary>
        /// Tax rate name
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// The tax rate as a percentage
        /// </summary>
        public decimal Rate { get; set; }
        
        /// <summary>
        /// The minimum amount for this tax rate to apply
        /// </summary>
        public decimal? MinimumAmount { get; set; }
        
        /// <summary>
        /// The maximum amount for this tax rate to apply
        /// </summary>
        public decimal? MaximumAmount { get; set; }
        
        /// <summary>
        /// The date from which this tax rate is effective
        /// </summary>
        public DateTime EffectiveDate { get; set; }
        
        /// <summary>
        /// The date until which this tax rate is effective
        /// </summary>
        public DateTime? EndDate { get; set; }
        
        /// <summary>
        /// Whether the tax rate is active
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Category to which this tax rate applies (e.g., "Goods", "Services")
        /// </summary>
        public string ApplicableCategory { get; set; }
    }
}