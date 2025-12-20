using System.ComponentModel.DataAnnotations;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Enums;

namespace FinTech.Core.Domain.Entities.Customers;

public class Customer : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string CustomerNumber { get; set; } = string.Empty;
    
    [Required]
    public CustomerType CustomerType { get; set; }
    
    // Individual Customer Fields
    [StringLength(100)]
    public string? FirstName { get; set; }
    
    [StringLength(100)]
    public string? LastName { get; set; }
    
    [StringLength(100)]
    public string? MiddleName { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
    
    public Gender? Gender { get; set; }
    
    [StringLength(20)]
    public string? MaritalStatus { get; set; }
    
    // Corporate Customer Fields
    [StringLength(200)]
    public string? CompanyName { get; set; }
    
    [StringLength(20)]
    public string? RCNumber { get; set; }
    
    [StringLength(20)]
    public string? TINNumber { get; set; }
    
    public DateTime? IncorporationDate { get; set; }
    
    // Common Contact Information
    [Required]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string? AlternatePhoneNumber { get; set; }
    
    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Address { get; set; }
    
    [StringLength(100)]
    public string? City { get; set; }
    
    [StringLength(50)]
    public string? State { get; set; }
    
    [StringLength(20)]
    public string? LGA { get; set; }
    
    [StringLength(10)]
    public string? PostalCode { get; set; }
    
    // KYC Information
    [StringLength(20)]
    public string? BVN { get; set; }
    
    [StringLength(20)]
    public string? NIN { get; set; }
    
    [StringLength(50)]
    public string? IdentificationType { get; set; }
    
    [StringLength(50)]
    public string? IdentificationNumber { get; set; }
    
    public DateTime? IdentificationExpiryDate { get; set; }
    
    [Required]
    public CustomerStatus Status { get; set; } = CustomerStatus.Active;
    
    [Required]
    public RiskRating RiskRating { get; set; } = RiskRating.Low;
    
    public DateTime? LastKYCUpdateDate { get; set; }
    
    [StringLength(200)]
    public string? Occupation { get; set; }
    
    [StringLength(200)]
    public string? EmployerName { get; set; }
    
    [StringLength(500)]
    public string? EmployerAddress { get; set; }
    
    [StringLength(200)]
    public string? NextOfKinName { get; set; }
    
    [StringLength(20)]
    public string? NextOfKinPhone { get; set; }
    
    [StringLength(500)]
    public string? NextOfKinAddress { get; set; }
    
    [StringLength(100)]
    public string? NextOfKinRelationship { get; set; }
    
    
    [Required]
    public string TenantId { get; set; } = string.Empty;

    public virtual ICollection<CustomerDocument> Documents { get; set; } = new List<CustomerDocument>();
    
    public string GetFullName()
    {
        if (CustomerType == CustomerType.Individual)
        {
            var middleName = !string.IsNullOrEmpty(MiddleName) ? $" {MiddleName}" : "";
            return $"{FirstName}{middleName} {LastName}";
        }
        return CompanyName ?? "";
    }
}
