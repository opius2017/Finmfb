using System;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.CustomerManagement
{
    public class Customer : AuditableEntity
    {
        public string CustomerCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string CustomerType { get; set; }
        public bool IsActive { get; set; }
        
        public Customer()
        {
            IsActive = true;
        }
        
        public string GetFullName()
        {
            return $"{FirstName} {LastName}";
        }
    }
}
