using System.ComponentModel.DataAnnotations;
using FinTech.Domain.Entities.Common;
using FinTech.Domain.Enums;

namespace FinTech.Domain.Entities.Identity;

public class TenantModule : BaseEntity
{
    [Required]
    public Guid TenantId { get; set; }
    public virtual Tenant Tenant { get; set; } = null!;
    
    [Required]
    public SystemModule Module { get; set; }
    
    [Required]
    public bool IsEnabled { get; set; } = true;
    
    public DateTime? EnabledDate { get; set; }
    
    public DateTime? DisabledDate { get; set; }
    
    [StringLength(500)]
    public string? Configuration { get; set; }
}