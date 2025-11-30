using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Auth;

public class LoginAttempt : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string? Username { get; set; }
    public bool IsSuccessful { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
    public string? FailureReason { get; set; }
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
    public string? Location { get; set; }
    public string? DeviceInfo { get; set; }
    
    // Navigation property
    public virtual ApplicationUser? User { get; set; }
}
