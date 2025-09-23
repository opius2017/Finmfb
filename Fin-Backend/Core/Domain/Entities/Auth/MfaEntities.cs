using System;
using System.Collections.Generic;

namespace FinTech.WebAPI.Domain.Entities.Auth
{
    /// <summary>
    /// Stores MFA settings for a user
    /// </summary>
    public class UserMfaSettings
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string SecretKey { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastVerifiedAt { get; set; }
        
        // Navigation properties
        public virtual List<MfaBackupCode> BackupCodes { get; set; } = new List<MfaBackupCode>();
        public virtual List<MfaChallenge> Challenges { get; set; } = new List<MfaChallenge>();
        public virtual List<TrustedDevice> TrustedDevices { get; set; } = new List<TrustedDevice>();
    }

    /// <summary>
    /// Stores backup codes for MFA recovery
    /// </summary>
    public class MfaBackupCode
    {
        public string Id { get; set; }
        public string UserMfaSettingsId { get; set; }
        public string Code { get; set; }  // Should be stored hashed
        public bool IsUsed { get; set; }
        public DateTime? UsedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation property
        public virtual UserMfaSettings UserMfaSettings { get; set; }
    }

    /// <summary>
    /// Stores active MFA challenges for step-up authentication
    /// </summary>
    public class MfaChallenge
    {
        public string Id { get; set; }
        public string UserMfaSettingsId { get; set; }
        public string Operation { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        
        // Navigation property
        public virtual UserMfaSettings UserMfaSettings { get; set; }
    }

    /// <summary>
    /// Stores devices that have been trusted for MFA
    /// </summary>
    public class TrustedDevice
    {
        public string Id { get; set; }
        public string UserMfaSettingsId { get; set; }
        public string DeviceName { get; set; }
        public string DeviceType { get; set; }
        public string Browser { get; set; }
        public string OperatingSystem { get; set; }
        public string IpAddress { get; set; }
        public string Location { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUsedAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public bool IsRevoked { get; set; }
        
        // Navigation property
        public virtual UserMfaSettings UserMfaSettings { get; set; }
    }

    /// <summary>
    /// Stores security-related activity for audit and monitoring
    /// </summary>
    public class SecurityActivity
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string EventType { get; set; }
        public DateTime Timestamp { get; set; }
        public string IpAddress { get; set; }
        public string Location { get; set; }
        public string DeviceInfo { get; set; }
        public string Status { get; set; }
        public string Details { get; set; }
    }

    /// <summary>
    /// Stores user security preferences
    /// </summary>
    public class SecurityPreferences
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public bool EmailNotificationsEnabled { get; set; } = true;
        public bool LoginNotificationsEnabled { get; set; } = true;
        public bool UnusualActivityNotificationsEnabled { get; set; } = true;
        public DateTime LastUpdated { get; set; }
    }
}