using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTech.Domain.Entities.Authentication
{
    /// <summary>
    /// Refresh token entity
    /// </summary>
    public class RefreshToken
    {
        /// <summary>
        /// Gets or sets the ID
        /// </summary>
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// Gets or sets the user ID
        /// </summary>
        public string UserId { get; set; }
        
        /// <summary>
        /// Gets or sets the token
        /// </summary>
        public string Token { get; set; }
        
        /// <summary>
        /// Gets or sets the JWT ID
        /// </summary>
        public string JwtId { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the token is used
        /// </summary>
        public bool IsUsed { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the token is revoked
        /// </summary>
        public bool IsRevoked { get; set; }
        
        /// <summary>
        /// Gets or sets the creation date
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Gets or sets the expiration date
        /// </summary>
        public DateTime ExpiresAt { get; set; }
        
        /// <summary>
        /// Gets or sets the device ID
        /// </summary>
        public string DeviceId { get; set; }
        
        /// <summary>
        /// Gets or sets the IP address
        /// </summary>
        public string IpAddress { get; set; }
    }
    
    /// <summary>
    /// MFA settings entity
    /// </summary>
    public class MfaSettings
    {
        /// <summary>
        /// Gets or sets the ID
        /// </summary>
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// Gets or sets the user ID
        /// </summary>
        public string UserId { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether MFA is enabled
        /// </summary>
        public bool IsEnabled { get; set; }
        
        /// <summary>
        /// Gets or sets the MFA method
        /// </summary>
        public string Method { get; set; }
        
        /// <summary>
        /// Gets or sets the shared key for app-based MFA
        /// </summary>
        public string SharedKey { get; set; }
        
        /// <summary>
        /// Gets or sets the recovery email for email-based MFA
        /// </summary>
        public string RecoveryEmail { get; set; }
        
        /// <summary>
        /// Gets or sets the recovery phone for SMS-based MFA
        /// </summary>
        public string RecoveryPhone { get; set; }
        
        /// <summary>
        /// Gets or sets the created date
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Gets or sets the last modified date
        /// </summary>
        public DateTime UpdatedAt { get; set; }
        
        /// <summary>
        /// Gets or sets the backup codes
        /// </summary>
        [NotMapped]
        public List<BackupCode> BackupCodes { get; set; } = new List<BackupCode>();
    }
    
    /// <summary>
    /// Backup code entity
    /// </summary>
    public class BackupCode
    {
        /// <summary>
        /// Gets or sets the ID
        /// </summary>
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// Gets or sets the user ID
        /// </summary>
        public string UserId { get; set; }
        
        /// <summary>
        /// Gets or sets the code
        /// </summary>
        public string Code { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the code is used
        /// </summary>
        public bool IsUsed { get; set; }
        
        /// <summary>
        /// Gets or sets the created date
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Gets or sets the used date
        /// </summary>
        public DateTime? UsedAt { get; set; }
    }
    
    /// <summary>
    /// MFA challenge entity
    /// </summary>
    public class MfaChallenge
    {
        /// <summary>
        /// Gets or sets the ID
        /// </summary>
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// Gets or sets the user ID
        /// </summary>
        public string UserId { get; set; }
        
        /// <summary>
        /// Gets or sets the verification code
        /// </summary>
        public string VerificationCode { get; set; }
        
        /// <summary>
        /// Gets or sets the method
        /// </summary>
        public string Method { get; set; }
        
        /// <summary>
        /// Gets or sets the created date
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Gets or sets the expiration date
        /// </summary>
        public DateTime ExpiresAt { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the challenge is used
        /// </summary>
        public bool IsUsed { get; set; }
        
        /// <summary>
        /// Gets or sets the used date
        /// </summary>
        public DateTime? UsedAt { get; set; }
        
        /// <summary>
        /// Gets or sets the IP address
        /// </summary>
        public string IpAddress { get; set; }
        
        /// <summary>
        /// Gets or sets the device ID
        /// </summary>
        public string DeviceId { get; set; }
    }
    
    /// <summary>
    /// Trusted device entity
    /// </summary>
    public class TrustedDevice
    {
        /// <summary>
        /// Gets or sets the ID
        /// </summary>
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// Gets or sets the user ID
        /// </summary>
        public string UserId { get; set; }
        
        /// <summary>
        /// Gets or sets the device ID
        /// </summary>
        public string DeviceId { get; set; }
        
        /// <summary>
        /// Gets or sets the device name
        /// </summary>
        public string DeviceName { get; set; }
        
        /// <summary>
        /// Gets or sets the device type
        /// </summary>
        public string DeviceType { get; set; }
        
        /// <summary>
        /// Gets or sets the operating system
        /// </summary>
        public string OperatingSystem { get; set; }
        
        /// <summary>
        /// Gets or sets the browser
        /// </summary>
        public string Browser { get; set; }
        
        /// <summary>
        /// Gets or sets the browser version
        /// </summary>
        public string BrowserVersion { get; set; }
        
        /// <summary>
        /// Gets or sets the IP address
        /// </summary>
        public string IpAddress { get; set; }
        
        /// <summary>
        /// Gets or sets the created date
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Gets or sets the last used date
        /// </summary>
        public DateTime LastUsedAt { get; set; }
        
        /// <summary>
        /// Gets or sets the location country
        /// </summary>
        public string Country { get; set; }
        
        /// <summary>
        /// Gets or sets the location city
        /// </summary>
        public string City { get; set; }
        
        /// <summary>
        /// Gets or sets the location region
        /// </summary>
        public string Region { get; set; }
    }
    
    /// <summary>
    /// Login attempt entity
    /// </summary>
    public class LoginAttempt
    {
        /// <summary>
        /// Gets or sets the ID
        /// </summary>
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// Gets or sets the user ID
        /// </summary>
        public string UserId { get; set; }
        
        /// <summary>
        /// Gets or sets the username
        /// </summary>
        public string Username { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the login was successful
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Gets or sets the failure reason
        /// </summary>
        public string FailureReason { get; set; }
        
        /// <summary>
        /// Gets or sets the IP address
        /// </summary>
        public string IpAddress { get; set; }
        
        /// <summary>
        /// Gets or sets the user agent
        /// </summary>
        public string UserAgent { get; set; }
        
        /// <summary>
        /// Gets or sets the login time
        /// </summary>
        public DateTime AttemptTime { get; set; }
        
        /// <summary>
        /// Gets or sets the location country
        /// </summary>
        public string Country { get; set; }
        
        /// <summary>
        /// Gets or sets the location city
        /// </summary>
        public string City { get; set; }
        
        /// <summary>
        /// Gets or sets the login method
        /// </summary>
        public string LoginMethod { get; set; }
    }
    
    /// <summary>
    /// Social login profile entity
    /// </summary>
    public class SocialLoginProfile
    {
        /// <summary>
        /// Gets or sets the ID
        /// </summary>
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// Gets or sets the user ID
        /// </summary>
        public string UserId { get; set; }
        
        /// <summary>
        /// Gets or sets the provider
        /// </summary>
        public string Provider { get; set; }
        
        /// <summary>
        /// Gets or sets the provider key
        /// </summary>
        public string ProviderKey { get; set; }
        
        /// <summary>
        /// Gets or sets the provider display name
        /// </summary>
        public string ProviderDisplayName { get; set; }
        
        /// <summary>
        /// Gets or sets the created date
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Gets or sets the last used date
        /// </summary>
        public DateTime LastUsedAt { get; set; }
    }
    
    /// <summary>
    /// Security alert entity
    /// </summary>
    public class SecurityAlert
    {
        /// <summary>
        /// Gets or sets the ID
        /// </summary>
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// Gets or sets the user ID
        /// </summary>
        public string UserId { get; set; }
        
        /// <summary>
        /// Gets or sets the alert type
        /// </summary>
        public string AlertType { get; set; }
        
        /// <summary>
        /// Gets or sets the message
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Gets or sets the details
        /// </summary>
        public string Details { get; set; }
        
        /// <summary>
        /// Gets or sets the created date
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the alert is read
        /// </summary>
        public bool IsRead { get; set; }
        
        /// <summary>
        /// Gets or sets the read date
        /// </summary>
        public DateTime? ReadAt { get; set; }
        
        /// <summary>
        /// Gets or sets the severity
        /// </summary>
        public string Severity { get; set; }
        
        /// <summary>
        /// Gets or sets the IP address
        /// </summary>
        public string IpAddress { get; set; }
        
        /// <summary>
        /// Gets or sets the device ID
        /// </summary>
        public string DeviceId { get; set; }
    }
    
    /// <summary>
    /// User security preferences entity
    /// </summary>
    public class UserSecurityPreferences
    {
        /// <summary>
        /// Gets or sets the ID
        /// </summary>
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// Gets or sets the user ID
        /// </summary>
        public string UserId { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether to notify on new logins
        /// </summary>
        public bool NotifyOnNewLogin { get; set; } = true;
        
        /// <summary>
        /// Gets or sets a value indicating whether to notify on suspicious activities
        /// </summary>
        public bool NotifyOnSuspiciousActivity { get; set; } = true;
        
        /// <summary>
        /// Gets or sets a value indicating whether to use location-based security
        /// </summary>
        public bool UseLocationBasedSecurity { get; set; } = true;
        
        /// <summary>
        /// Gets or sets a value indicating whether to require MFA for suspicious logins
        /// </summary>
        public bool RequireMfaForSuspiciousLogins { get; set; } = true;
        
        /// <summary>
        /// Gets or sets a value indicating whether to allow password reset
        /// </summary>
        public bool AllowPasswordReset { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the session timeout in minutes
        /// </summary>
        public int SessionTimeoutMinutes { get; set; } = 30;
        
        /// <summary>
        /// Gets or sets the maximum number of concurrent sessions
        /// </summary>
        public int MaxConcurrentSessions { get; set; } = 5;
        
        /// <summary>
        /// Gets or sets the maximum number of failed login attempts before lockout
        /// </summary>
        public int MaxFailedLoginAttempts { get; set; } = 5;
        
        /// <summary>
        /// Gets or sets the lockout duration in minutes
        /// </summary>
        public int LockoutDurationMinutes { get; set; } = 15;
        
        /// <summary>
        /// Gets or sets a value indicating whether to use passwordless login
        /// </summary>
        public bool UsePasswordlessLogin { get; set; } = false;
    }
    
    /// <summary>
    /// Extended the ASP.NET Identity User class to add additional fields
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Gets or sets the first name
        /// </summary>
        [StringLength(100)]
        public string FirstName { get; set; }
        
        /// <summary>
        /// Gets or sets the last name
        /// </summary>
        [StringLength(100)]
        public string LastName { get; set; }
        
        /// <summary>
        /// Gets or sets the created date
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Gets or sets the last login date
        /// </summary>
        public DateTime? LastLoginAt { get; set; }
        
        /// <summary>
        /// Gets or sets the last login IP
        /// </summary>
        public string LastLoginIp { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether MFA is enabled
        /// </summary>
        public bool IsMfaEnabled { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the account is locked for security reasons
        /// </summary>
        public bool IsSecurityLocked { get; set; }
        
        /// <summary>
        /// Gets or sets the security lockout reason
        /// </summary>
        public string SecurityLockoutReason { get; set; }
        
        /// <summary>
        /// Gets or sets the security lockout end date
        /// </summary>
        public DateTime? SecurityLockoutEnd { get; set; }
        
        /// <summary>
        /// Gets or sets the MFA settings
        /// </summary>
        [NotMapped]
        public MfaSettings MfaSettings { get; set; }
        
        /// <summary>
        /// Gets or sets the security preferences
        /// </summary>
        [NotMapped]
        public UserSecurityPreferences SecurityPreferences { get; set; }
        
        /// <summary>
        /// Gets or sets the trusted devices
        /// </summary>
        [NotMapped]
        public List<TrustedDevice> TrustedDevices { get; set; } = new List<TrustedDevice>();
        
        /// <summary>
        /// Gets or sets the social login profiles
        /// </summary>
        [NotMapped]
        public List<SocialLoginProfile> SocialLoginProfiles { get; set; } = new List<SocialLoginProfile>();
        
        /// <summary>
        /// Gets or sets the refresh tokens
        /// </summary>
        [NotMapped]
        public List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        
        /// <summary>
        /// Gets or sets the security alerts
        /// </summary>
        [NotMapped]
        public List<SecurityAlert> SecurityAlerts { get; set; } = new List<SecurityAlert>();
        
        /// <summary>
        /// Gets or sets the login attempts
        /// </summary>
        [NotMapped]
        public List<LoginAttempt> LoginAttempts { get; set; } = new List<LoginAttempt>();
    }
}