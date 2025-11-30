using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Identity;

public class MfaSettings : BaseEntity
{
    public Guid UserId { get; set; }
    public bool IsEnabled { get; set; }
    public MfaMethod PreferredMethod { get; set; } = MfaMethod.None;
    public string? TotpSecret { get; set; }
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberVerified { get; set; }
    public string? Email { get; set; }
    public bool EmailVerified { get; set; }
    public DateTime? EnabledAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    
    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual ICollection<BackupCode> BackupCodes { get; set; } = new List<BackupCode>();
    public virtual ICollection<MfaChallenge> Challenges { get; set; } = new List<MfaChallenge>();
}

public enum MfaMethod
{
    None = 0,
    Totp = 1,
    Sms = 2,
    Email = 3,
    Authenticator = 4
}
