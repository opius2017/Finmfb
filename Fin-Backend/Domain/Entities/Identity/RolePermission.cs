using System.ComponentModel.DataAnnotations;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Identity;

public class RolePermission : BaseEntity
{
    [Required]
    public Guid RoleId { get; set; }
    public virtual Role Role { get; set; } = null!;
    
    [Required]
    public Guid PermissionId { get; set; }
    public virtual Permission Permission { get; set; } = null!;
    
    [Required]
    public bool IsGranted { get; set; } = true;
    
    [StringLength(500)]
    public string? Conditions { get; set; }
    
    [Required]
    public Guid TenantId { get; set; }
}
