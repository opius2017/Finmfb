using System.ComponentModel.DataAnnotations;
using FinTech.Domain.Entities.Common;
using FinTech.Domain.Enums;

namespace FinTech.Domain.Entities.Identity;

public class Tenant : BaseEntity
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Code { get; set; } = string.Empty;
    
    [Required]
    public OrganizationType OrganizationType { get; set; }
    
    [StringLength(20)]
    public string? RCNumber { get; set; }
    
    [StringLength(20)]
    public string? TINNumber { get; set; }
    
    [StringLength(500)]
    public string? Address { get; set; }
    
    [StringLength(100)]
    public string? City { get; set; }
    
    [StringLength(50)]
    public string? State { get; set; }
    
    [StringLength(20)]
    public string? PhoneNumber { get; set; }
    
    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }
    
    [StringLength(200)]
    public string? Website { get; set; }
    
    [StringLength(250)]
    public string? LogoUrl { get; set; }
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    public DateTime? SubscriptionEndDate { get; set; }
    
    [Required]
    [StringLength(3)]
    public string BaseCurrency { get; set; } = "NGN";
    
    [StringLength(50)]
    public string? TimeZone { get; set; } = "West Africa Standard Time";
    
    public virtual ICollection<ApplicationUser> Users { get; set; } = [];
    
    public virtual ICollection<TenantModule> TenantModules { get; set; } = [];
}