using System.ComponentModel.DataAnnotations;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Enums;

namespace FinTech.Core.Domain.Entities.Identity;

public class ModuleDashboard : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string ModuleName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    public string DashboardName { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public string DashboardConfig { get; set; } = string.Empty; // JSON configuration
    
    [Required]
    public DashboardType DashboardType { get; set; }
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    [Required]
    public int SortOrder { get; set; } = 0;
    
    [Required]
    public Guid TenantId { get; set; }
    
    public virtual ICollection<UserDashboardPreference> UserPreferences { get; set; } = [];
}
