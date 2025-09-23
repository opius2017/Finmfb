using System.ComponentModel.DataAnnotations;
using FinTech.Domain.Entities.Common;

namespace FinTech.Domain.Entities.Identity;

public class UserRole : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }
    public virtual ApplicationUser User { get; set; } = null!;
    
    [Required]
    public Guid RoleId { get; set; }
    public virtual Role Role { get; set; } = null!;
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    public DateTime? AssignedDate { get; set; }
    
    public string? AssignedBy { get; set; }
    
    [Required]
    public Guid TenantId { get; set; }
}