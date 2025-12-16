using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTech.Core.Domain.Entities.Identity
{
    public class UserMfaSettings
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public Guid UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
        
        public string SecretKey { get; set; }
        
        public bool IsEnabled { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }

        public DateTime? LastVerifiedAt { get; set; }
        
        public string Method { get; set; }
        
        public string RecoveryEmail { get; set; }
        
        public string RecoveryPhone { get; set; }
        
        public virtual ICollection<MfaBackupCode> BackupCodes { get; set; } = new List<MfaBackupCode>();
        public virtual ICollection<MfaChallenge> Challenges { get; set; } = new List<MfaChallenge>();
        public virtual ICollection<TrustedDevice> TrustedDevices { get; set; } = new List<TrustedDevice>();
    }

    public class MfaBackupCode
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string UserMfaSettingsId { get; set; }
        
        [ForeignKey("UserMfaSettingsId")]
        public virtual UserMfaSettings UserMfaSettings { get; set; }
        
        [Required]
        public string Code { get; set; } // Hashed
        
        public bool IsUsed { get; set; }
        
        public DateTime? UsedAt { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class MfaChallenge
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string UserMfaSettingsId { get; set; }
        
        [ForeignKey("UserMfaSettingsId")]
        public virtual UserMfaSettings UserMfaSettings { get; set; }
        
        public string Operation { get; set; }
        
        public bool IsVerified { get; set; }
        
        public DateTime? VerifiedAt { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime ExpiresAt { get; set; }

        // Added properties to match usage
        public Guid UserId { get; set; }
        public string? VerificationCode { get; set; }
        public string Method { get; set; }
        public bool IsUsed { get; set; }
        public string? IpAddress { get; set; }
        public string? DeviceId { get; set; }
        public DateTime? UsedAt { get; set; }
    }

    public class TrustedDevice
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string UserMfaSettingsId { get; set; }
        
        [ForeignKey("UserMfaSettingsId")]
        public virtual UserMfaSettings UserMfaSettings { get; set; }
        
        public string DeviceName { get; set; }
        
        public string DeviceType { get; set; }
        
        public string Browser { get; set; }
        
        public string OperatingSystem { get; set; }
        
        public string IpAddress { get; set; }
        
        public string Location { get; set; }
        
        public bool IsRevoked { get; set; }
        
        public DateTime? RevokedAt { get; set; }

        // Added properties to match usage
        public Guid UserId { get; set; }
        public string? DeviceId { get; set; } // Separate from Id? Or alias? Service asks for DeviceId. 
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Region { get; set; }
        public string? BrowserVersion { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime LastUsedAt { get; set; }
    }

    public class SecurityActivity
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public Guid UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
        
        public string EventType { get; set; }
        
        public string Status { get; set; }
        
        public string IpAddress { get; set; }
        
        public string DeviceInfo { get; set; }
        
        public string Location { get; set; }
        
        public string Details { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
    
    // Enum for MfaMethod needed for AdvancedAuthService errors
    public enum MfaMethod
    {
        App,
        Email,
        Sms,
        None
    }
}
