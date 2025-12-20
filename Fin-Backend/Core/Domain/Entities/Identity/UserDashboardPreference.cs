using System.ComponentModel.DataAnnotations;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Identity;

public class UserDashboardPreference : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }
    public virtual ApplicationUser? User { get; set; }
    
    [Required]
    public string ModuleDashboardId { get; set; } = string.Empty;
    public virtual ModuleDashboard? ModuleDashboard { get; set; }
    
    [Required]
    public string PreferenceConfig { get; set; } = string.Empty; // JSON configuration
    
    [Required]
    public bool IsDefault { get; set; } = false;
    
    [Required]
    public bool IsVisible { get; set; } = true;
    
    [Required]
    public int SortOrder { get; set; } = 0;
    
    [Required]
    public string TenantId { get; set; } = string.Empty;
}
