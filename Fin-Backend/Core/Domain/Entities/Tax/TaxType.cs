using System;
// using FinTech.Core.Application.DTOs.Tax;
using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Tax
{
    /// <summary>
    /// Direction of tax application
    /// </summary>
    public enum TaxDirection
    {
        Input = 1,    // Tax paid on purchases
        Output = 2    // Tax charged on sales
    }

    /// <summary>
    /// Represents a tax type in the system
    /// </summary>
    public class TaxType : AuditableEntity
    {
        /// <summary>
        /// Tax type code (e.g., VAT, WHT)
        /// </summary>
        public string Code { get; set; } = string.Empty;
        
        /// <summary>
        /// Tax type name
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Description of the tax type
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// Whether the tax type is active
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// The direction of the tax (input or output)
        /// </summary>
        public TaxDirection Direction { get; set; }
        
        /// <summary>
        /// Whether the tax is reclaimable
        /// </summary>
        public bool IsReclaimable { get; set; }
        
        /// <summary>
        /// The ID of the account for tax liabilities
        /// </summary>
        public string? LiabilityAccountId { get; set; }
        
        /// <summary>
        /// The ID of the account for tax receivables
        /// </summary>
        public string? ReceivableAccountId { get; set; }
        
        /// <summary>
        /// The regulatory authority managing this tax type
        /// </summary>
        public string? RegulatoryAuthority { get; set; }
    }
}
