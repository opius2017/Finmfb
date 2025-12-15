using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Identity;

public class LoginAttempt : BaseEntity
{
    public Guid? UserId { get; set; }
    public string? Username { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
    public bool Success { get => IsSuccessful; set => IsSuccessful = value; }
    public bool IsSuccessful { get; set; }
    public DateTime AttemptTime { get => AttemptedAt; set => AttemptedAt = value; }
    public string LoginMethod { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;

    public string? FailureReason { get; set; }
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
    public string? Location { get; set; }
    public string? DeviceInfo { get; set; }
    
    // Navigation property
    public virtual ApplicationUser? User { get; set; }
}
