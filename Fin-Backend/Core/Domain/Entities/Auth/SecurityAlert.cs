using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Auth;

public class SecurityAlert : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public SecurityAlertType AlertType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public SecurityAlertSeverity Severity { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? IpAddress { get; set; }
    public string? DeviceInfo { get; set; }
    public string? AdditionalData { get; set; }
    
    // Navigation property
    public virtual ApplicationUser? User { get; set; }
}

public enum SecurityAlertType
{
    UnusualLogin = 1,
    PasswordChanged = 2,
    MfaEnabled = 3,
    MfaDisabled = 4,
    NewDeviceAdded = 5,
    FailedLoginAttempts = 6,
    AccountLocked = 7,
    SuspiciousActivity = 8
}

public enum SecurityAlertSeverity
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}
