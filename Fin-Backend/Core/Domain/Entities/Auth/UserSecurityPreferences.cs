using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Auth;

public class UserSecurityPreferences : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public bool EnableEmailNotifications { get; set; } = true;
    public bool EnableSmsNotifications { get; set; } = true;
    public bool EnablePushNotifications { get; set; } = true;
    public bool NotifyOnLogin { get; set; } = true;
    public bool NotifyOnPasswordChange { get; set; } = true;
    public bool NotifyOnMfaChange { get; set; } = true;
    public bool NotifyOnNewDevice { get; set; } = true;
    public bool NotifyOnSuspiciousActivity { get; set; } = true;
    public int SessionTimeoutMinutes { get; set; } = 30;
    public bool RequireMfaForSensitiveOperations { get; set; } = true;
    public DateTime? LastUpdatedAt { get; set; }
    
    // Navigation property
    public virtual ApplicationUser? User { get; set; }
}
