using System.ComponentModel.DataAnnotations;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Identity;

public class UserRole : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }
    public virtual ApplicationUser? User { get; set; }
    
    [Required]
    public string RoleId { get; set; } = string.Empty;
    public virtual Role? Role { get; set; }
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    public DateTime? AssignedDate { get; set; }
    
    public string? AssignedBy { get; set; }
    
    [Required]
    public string TenantId { get; set; } = string.Empty;
}
