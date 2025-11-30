using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Auth;

public class SocialLoginProfile : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public SocialProvider Provider { get; set; }
    public string ProviderUserId { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public DateTime LinkedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUsedAt { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation property
    public virtual ApplicationUser? User { get; set; }
}

public enum SocialProvider
{
    Google = 1,
    Facebook = 2,
    Microsoft = 3,
    Apple = 4
}
