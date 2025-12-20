using System.ComponentModel.DataAnnotations;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Identity;

public class RolePermission : BaseEntity
{
    [Required]
    public string RoleId { get; set; } = string.Empty;
    public virtual Role? Role { get; set; }
    
    [Required]
    public string PermissionId { get; set; } = string.Empty;
    public virtual Permission? Permission { get; set; }
    
    [Required]
    public bool IsGranted { get; set; } = true;
    
    [StringLength(500)]
    public string? Conditions { get; set; }
    
    [Required]
    public string TenantId { get; set; } = string.Empty;
}
