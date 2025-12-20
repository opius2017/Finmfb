using System.ComponentModel.DataAnnotations;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Enums;

namespace FinTech.Core.Domain.Entities.Identity;

public class TenantModule : BaseEntity
{
    [Required]
    public string TenantId { get; set; } = string.Empty;
    public virtual Tenant? Tenant { get; set; }
    
    [Required]
    public SystemModule Module { get; set; }
    
    [Required]
    public bool IsEnabled { get; set; } = true;
    
    public DateTime? EnabledDate { get; set; }
    
    public DateTime? DisabledDate { get; set; }
    
    [StringLength(500)]
    public string? Configuration { get; set; }
}
