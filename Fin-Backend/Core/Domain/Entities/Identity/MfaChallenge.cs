using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Identity;

public class MfaChallenge : BaseEntity
{
    public Guid UserId { get; set; }
    public string ChallengeCode { get; set; } = string.Empty;
    public MfaMethod Method { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsValid => !IsUsed && !IsExpired;
    
    // Navigation property
    public virtual ApplicationUser User { get; set; } = null!;
}
