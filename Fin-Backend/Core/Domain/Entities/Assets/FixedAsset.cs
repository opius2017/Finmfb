using System;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Assets
{
    public class FixedAsset : AuditableEntity
    {
        public string AssetCode { get; set; }
        public string AssetName { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal DepreciationRate { get; set; }
        public string DepreciationMethod { get; set; }
        public int UsefulLife { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
        
        public FixedAsset()
        {
            PurchaseDate = DateTime.UtcNow;
            Status = "ACTIVE";
        }
    }
}
