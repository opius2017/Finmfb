using System;
using FinTech.Core.Domain.Enums;

namespace FinTech.Core.Application.DTOs.Customers
{
    public class CustomerDto
    {
        public string Id { get; set; }
        public string CustomerNumber { get; set; }
        public string FullName { get; set; } // Helper calculation
        public CustomerType CustomerType { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public CustomerStatus Status { get; set; }
        public RiskRating RiskRating { get; set; }
    }

    public class CreateCustomerDto
    {
        public CustomerType CustomerType { get; set; }
        
        // Individual
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? MiddleName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public Gender? Gender { get; set; }
        public string? MaritalStatus { get; set; }

        // Corporate
        public string? CompanyName { get; set; }
        public string? RCNumber { get; set; }
        public DateTime? IncorporationDate { get; set; }

        // Contact
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string? Address { get; set; }
        
        // KYC
        public string? BVN { get; set; }
        public string? NIN { get; set; }
    }
}
