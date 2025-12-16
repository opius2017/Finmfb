using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Identity;

public class SocialLoginProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public string Provider { get; set; } = string.Empty; // Google, Facebook, Microsoft, etc.
    public string ProviderKey { get; set; } = string.Empty;
    public string? ProviderDisplayName { get; set; }
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public DateTime LinkedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUsedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? TokenExpiresAt { get; set; }
    
    // Navigation property
    public virtual ApplicationUser? User { get; set; }
}
