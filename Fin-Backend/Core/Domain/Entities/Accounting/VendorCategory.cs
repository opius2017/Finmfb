using System;
using System.Collections.Generic;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Accounting
{
    public class VendorCategory : AuditableEntity
    {
        public string CategoryCode { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        
        // Navigation properties
        public virtual ICollection<Vendor> Vendors { get; set; } = new List<Vendor>();
        
        public VendorCategory()
        {
            IsActive = true;
        }
    }
}
