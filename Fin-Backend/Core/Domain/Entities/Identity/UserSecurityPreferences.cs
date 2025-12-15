using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Identity;

public class UserSecurityPreferences : BaseEntity
{
    public Guid UserId { get; set; }
    public bool EnableLoginNotifications { get; set; } = true;
    public bool EnableSecurityAlerts { get; set; } = true;
    public bool EnableMfaReminder { get; set; } = true;
    public bool RequireMfaForSensitiveOperations { get; set; } = true;
    public int SessionTimeoutMinutes { get; set; } = 30;
    public bool AllowMultipleSessions { get; set; } = true;
    public bool TrustDeviceEnabled { get; set; } = true;
    public bool EmailNotificationsEnabled { get; set; } = true;
    public bool LoginNotificationsEnabled { get; set; } = true;
    public bool UnusualActivityNotificationsEnabled { get; set; } = true;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Added to match usage
    public bool NotifyOnNewLogin { get; set; } = true;
    public bool NotifyOnSuspiciousActivity { get; set; } = true;
    public bool UseLocationBasedSecurity { get; set; }
    public bool RequireMfaForSuspiciousLogins { get; set; } = true;
    public bool AllowPasswordReset { get; set; } = true;
    public int MaxConcurrentSessions { get; set; } = 5;
    public int MaxFailedLoginAttempts { get; set; } = 5;
    public int LockoutDurationMinutes { get; set; } = 30;
    public bool UsePasswordlessLogin { get; set; }

    public int TrustDeviceDays { get; set; } = 30;
    public DateTime? LastUpdatedAt { get; set; } // Keep original if needed, but LastUpdated is preferred by Service
    
    // Navigation property
    public virtual ApplicationUser? User { get; set; }
}
