using System;
using FinTech.Core.Domain.Common;

namespace FinTech.Core.Domain.Entities.Tax
{
    /// <summary>
    /// Represents a tax exemption for a specific party
    /// </summary>
    using FinTech.Core.Domain.Entities.Common;
    public class TaxExemption : AuditableEntity
    {
        /// <summary>
        /// The ID of the party (customer or vendor) exempted from tax
        /// </summary>
        public string PartyId { get; set; } = string.Empty;
        
        /// <summary>
        /// The ID of the tax type this exemption applies to (null for all tax types)
        /// </summary>
        public string? TaxTypeId { get; set; }
        
        /// <summary>
        /// The reason for the exemption
        /// </summary>
        public string Reason { get; set; } = string.Empty;
        
        /// <summary>
        /// The exemption certificate number (if applicable)
        /// </summary>
        public string? ExemptionCertificateNumber { get; set; }
        
        /// <summary>
        /// The start date of the exemption
        /// </summary>
        public DateTime StartDate { get; set; }
        
        /// <summary>
        /// The end date of the exemption (null for indefinite)
        /// </summary>
        public DateTime? EndDate { get; set; }
        
        /// <summary>
        /// Whether the exemption is active
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// The name of the approving authority
        /// </summary>
        public string? ApprovingAuthority { get; set; }
        
        /// <summary>
        /// Additional notes or comments about the exemption
        /// </summary>
        public string? Notes { get; set; }
    }
}
