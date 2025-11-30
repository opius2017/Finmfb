using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Auth;

public class MfaSettings : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string? SecretKey { get; set; }
    public MfaMethod PreferredMethod { get; set; } = MfaMethod.None;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public DateTime? EnabledAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    
    // Navigation properties
    public virtual ApplicationUser? User { get; set; }
    public virtual ICollection<MfaChallenge> Challenges { get; set; } = new List<MfaChallenge>();
    public virtual ICollection<BackupCode> BackupCodes { get; set; } = new List<BackupCode>();
}

public enum MfaMethod
{
    None = 0,
    Authenticator = 1,
    Sms = 2,
    Email = 3
}
