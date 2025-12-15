using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Identity;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string IpAddress { get => CreatedByIp; set => CreatedByIp = value; }
    public DateTime ExpiresAt { get; set; }

    public string CreatedByIp { get; set; } = string.Empty;
    public DateTime? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? ReasonRevoked { get; set; }
    
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    
    // Changed to have a setter to fix CS0200
    public bool IsRevoked { get => RevokedAt != null; set { if (value && RevokedAt == null) RevokedAt = DateTime.UtcNow; } } 
    public bool IsActive => !IsRevoked && !IsExpired;
    
    // Added to match usage
    public bool IsUsed { get; set; }

    // Navigation property
    public virtual ApplicationUser? User { get; set; }
}
