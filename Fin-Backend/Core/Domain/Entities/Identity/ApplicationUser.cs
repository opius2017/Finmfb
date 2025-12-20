using Microsoft.AspNetCore.Identity;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public bool IsMfaEnabled { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    
    public string? CustomerId { get; set; }
    public string? LastLoginIp { get; set; }

    public bool IsSecurityLocked { get; set; }
    public DateTime? SecurityLockoutEnd { get; set; }
    public string? SecurityLockoutReason { get; set; }

    // Navigation properties
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public virtual UserMfaSettings? MfaSettings { get; set; }
    public virtual ICollection<TrustedDevice> TrustedDevices { get; set; } = new List<TrustedDevice>();
    public virtual ICollection<LoginAttempt> LoginAttempts { get; set; } = new List<LoginAttempt>();
    public virtual ICollection<SocialLoginProfile> SocialLoginProfiles { get; set; } = new List<SocialLoginProfile>();
    public virtual ICollection<SecurityAlert> SecurityAlerts { get; set; } = new List<SecurityAlert>();
    public virtual UserSecurityPreferences? SecurityPreferences { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public string? TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }
}
