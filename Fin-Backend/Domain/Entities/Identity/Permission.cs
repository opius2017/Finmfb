using System.ComponentModel.DataAnnotations;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Enums;

namespace FinTech.Core.Domain.Entities.Identity;

public class Permission : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string PermissionName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    public string DisplayName { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Module { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Resource { get; set; } = string.Empty;
    
    [Required]
    public PermissionAction Action { get; set; }
    
    [Required]
    public bool IsSystemPermission { get; set; } = false;
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = [];
}
