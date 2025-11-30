using System;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Inventory
{
    public class InventoryItem : AuditableEntity
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public decimal UnitPrice { get; set; }
        public int QuantityOnHand { get; set; }
        public int ReorderLevel { get; set; }
        public string Unit { get; set; }
        public bool IsActive { get; set; }
        
        public InventoryItem()
        {
            IsActive = true;
        }
    }
}
