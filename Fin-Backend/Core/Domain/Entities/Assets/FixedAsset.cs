using System;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Assets
{
    public class FixedAsset : AuditableEntity
    {
        public string AssetCode { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal DepreciationRate { get; set; }
        public string DepreciationMethod { get; set; } = string.Empty;
        public int UsefulLife { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        
        public FixedAsset()
        {
            PurchaseDate = DateTime.UtcNow;
            Status = "ACTIVE";
        }
    }
}
