using System.ComponentModel.DataAnnotations;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Enums;

namespace FinTech.Core.Domain.Entities.Security;

public class AuditLog : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Action { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string EntityName { get; set; } = string.Empty;
    
    [Required]
    public Guid EntityId { get; set; }
    
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string UserName { get; set; } = string.Empty;
    
    [Required]
    public DateTime Timestamp { get; set; }
    
    [Required]
    [StringLength(50)]
    public string IPAddress { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? UserAgent { get; set; }
    
    public string? OldValues { get; set; }
    
    public string? NewValues { get; set; }
    
    [Required]
    public AuditAction AuditAction { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }

    public string? Changes { get; set; }
    
    [Required]
    public Guid TenantId { get; set; }
    
    [Required]
    [StringLength(64)]
    public string Hash { get; set; } = string.Empty; // For tamper detection
}
