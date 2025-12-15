using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Identity;

public class SecurityAlert : BaseEntity
{
    public Guid UserId { get; set; }
    public SecurityAlertType AlertType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public SecurityAlertSeverity Severity { get; set; }

    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? IpAddress { get; set; }
    public string? Details { get; set; } // Added
    public string? DeviceId { get; set; } // Added
    public string? DeviceInfo { get; set; }
    public string? Location { get; set; }
    
    // Navigation property
    public virtual ApplicationUser? User { get; set; }
}

public enum SecurityAlertType
{
    UnusualLogin = 1,
    NewDevice = 2,
    PasswordChanged = 3,
    MfaDisabled = 4,
    AccountLocked = 5,
    FailedLoginAttempts = 6,
    SuspiciousActivity = 7
}

public enum SecurityAlertSeverity
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}
