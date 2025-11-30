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
    public int TrustDeviceDays { get; set; } = 30;
    public DateTime? LastUpdatedAt { get; set; }
    
    // Navigation property
    public virtual ApplicationUser User { get; set; } = null!;
}
