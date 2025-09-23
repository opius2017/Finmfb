using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FinTech.Domain.Entities.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? MiddleName { get; set; }
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastLoginDate { get; set; }
    
    [StringLength(20)]
    public string? EmployeeId { get; set; }
    
    [StringLength(50)]
    public string? Department { get; set; }
    
    [StringLength(100)]
    public string? Position { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
    
    [StringLength(200)]
    public string? Address { get; set; }
    
    [StringLength(50)]
    public string? City { get; set; }
    
    [StringLength(50)]
    public string? State { get; set; }
    
    [StringLength(10)]
    public string? PostalCode { get; set; }
    
    [Required]
    public Guid TenantId { get; set; }
    public virtual Tenant Tenant { get; set; } = null!;
    
    [StringLength(250)]
    public string? ProfileImageUrl { get; set; }
    
    public virtual ICollection<UserRole> UserRoles { get; set; } = [];
    
    public string FullName => $"{FirstName} {LastName}";
}