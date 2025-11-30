using System;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Accounting
{
    public class Vendor : AuditableEntity
    {
        public string VendorCode { get; set; }
        public string VendorName { get; set; }
        public string ContactPerson { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string TaxId { get; set; }
        public string VendorCategoryId { get; set; }
        public string PaymentTerms { get; set; }
        public bool IsActive { get; set; }
        
        // Navigation properties
        public virtual VendorCategory VendorCategory { get; set; }
        
        public Vendor()
        {
            IsActive = true;
        }
    }
}
