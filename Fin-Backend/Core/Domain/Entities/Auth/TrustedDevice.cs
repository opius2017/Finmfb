using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Auth;

public class TrustedDevice : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string? DeviceType { get; set; }
    public string? Browser { get; set; }
    public string? OperatingSystem { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public DateTime TrustedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUsedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    
    // Navigation property
    public virtual ApplicationUser? User { get; set; }
}
