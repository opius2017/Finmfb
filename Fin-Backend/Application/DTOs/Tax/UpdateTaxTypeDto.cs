namespace FinTech.Core.Application.DTOs.Tax
{
    /// <summary>
    /// DTO for updating an existing tax type
    /// </summary>
    public class UpdateTaxTypeDto
    {
        /// <summary>
        /// Unique identifier for the tax type
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Tax type name (e.g., Value Added Tax)
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Description of the tax type
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Whether the tax type is active
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// The liability account where collected tax is recorded
        /// </summary>
        public string LiabilityAccountId { get; set; }
        
        /// <summary>
        /// The receivable account where claimable tax is recorded
        /// </summary>
        public string ReceivableAccountId { get; set; }
        
        /// <summary>
        /// Whether the tax is collected from customers or paid to vendors
        /// </summary>
        public TaxDirection Direction { get; set; }
        
        /// <summary>
        /// Whether the tax can be reclaimed/offset
        /// </summary>
        public bool IsReclaimable { get; set; }
        
        /// <summary>
        /// The regulatory authority for this tax type
        /// </summary>
        public string RegulatoryAuthority { get; set; }
    }
}
