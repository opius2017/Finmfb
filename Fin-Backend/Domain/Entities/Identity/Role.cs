using System.ComponentModel.DataAnnotations;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Enums;

namespace FinTech.Core.Domain.Entities.Identity;

public class Role : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string RoleName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    public string DisplayName { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public RoleLevel Level { get; set; }
    
    [Required]
    public bool IsSystemRole { get; set; } = false;
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    [StringLength(100)]
    public string? DefaultModule { get; set; }
    
    [StringLength(100)]
    public string? DefaultDashboard { get; set; }
    
    [Required]
    public Guid TenantId { get; set; }
    
    public virtual ICollection<UserRole> UserRoles { get; set; } = [];
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = [];
}
