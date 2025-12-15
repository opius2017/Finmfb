using System;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Inventory
{
    public class InventoryItem : AuditableEntity
    {
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int QuantityOnHand { get; set; }
        public int ReorderLevel { get; set; }
        public string Unit { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        
        public InventoryItem()
        {
            IsActive = true;
        }
    }
}
