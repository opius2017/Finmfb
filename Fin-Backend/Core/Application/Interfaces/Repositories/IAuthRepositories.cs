using FinTech.Core.Domain.Entities.Identity;
using AuthRefreshToken = FinTech.Core.Domain.Entities.Identity.RefreshToken;
using AuthMfaSettings = FinTech.Core.Domain.Entities.Identity.UserMfaSettings;
using AuthBackupCode = FinTech.Core.Domain.Entities.Identity.BackupCode;
using AuthMfaChallenge = FinTech.Core.Domain.Entities.Identity.MfaChallenge;
using AuthTrustedDevice = FinTech.Core.Domain.Entities.Identity.TrustedDevice;
using AuthLoginAttempt = FinTech.Core.Domain.Entities.Identity.LoginAttempt;
using AuthSocialLoginProfile = FinTech.Core.Domain.Entities.Identity.SocialLoginProfile;
using AuthSecurityAlert = FinTech.Core.Domain.Entities.Identity.SecurityAlert;
using AuthUserSecurityPreferences = FinTech.Core.Domain.Entities.Identity.UserSecurityPreferences;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinTech.Core.Application.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for the authentication entities
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public interface IBaseAuthRepository<T> where T : class
    {
        /// <summary>
        /// Gets an entity by ID
        /// </summary>
        /// <param name="id">The ID</param>
        /// <returns>The entity</returns>
        Task<T> GetByIdAsync(string id);
        
        /// <summary>
        /// Gets all entities
        /// </summary>
        /// <returns>The entities</returns>
        Task<IEnumerable<T>> GetAllAsync();
        
        /// <summary>
        /// Adds an entity
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <returns>The added entity</returns>
        Task<T> AddAsync(T entity);
        
        /// <summary>
        /// Updates an entity
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <returns>The updated entity</returns>
        Task<T> UpdateAsync(T entity);
        
        /// <summary>
        /// Deletes an entity
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <returns>A task</returns>
        Task DeleteAsync(T entity);
        
        /// <summary>
        /// Deletes an entity by ID
        /// </summary>
        /// <param name="id">The ID</param>
        /// <returns>A task</returns>
        Task DeleteByIdAsync(string id);
    }
    
    /// <summary>
    /// Repository interface for the refresh token entity
    /// </summary>
    public interface IRefreshTokenRepository : IBaseAuthRepository<AuthRefreshToken>
    {
        /// <summary>
        /// Gets a refresh token by token
        /// </summary>
        /// <param name="token">The token</param>
        /// <returns>The refresh token</returns>
        Task<AuthRefreshToken> GetByTokenAsync(string token);
        
        /// <summary>
        /// Gets all refresh tokens for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The refresh tokens</returns>
        Task<IEnumerable<AuthRefreshToken>> GetByUserIdAsync(string userId);
        
        /// <summary>
        /// Gets all active refresh tokens for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The refresh tokens</returns>
        Task<IEnumerable<AuthRefreshToken>> GetActiveByUserIdAsync(string userId);
        
        /// <summary>
        /// Revokes all refresh tokens for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>A task</returns>
        Task RevokeAllForUserAsync(string userId);
        
        /// <summary>
        /// Revokes all refresh tokens for a user except the current one
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="currentToken">The current token</param>
        /// <returns>A task</returns>
        Task RevokeAllForUserExceptCurrentAsync(string userId, string currentToken);
    }
    
    /// <summary>
    /// Repository interface for the MFA settings entity
    /// </summary>
    public interface IMfaSettingsRepository : IBaseAuthRepository<AuthMfaSettings>
    {
        /// <summary>
        /// Gets the MFA settings for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The MFA settings</returns>
        Task<AuthMfaSettings> GetByUserIdAsync(string userId);
        
        /// <summary>
        /// Enables MFA for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="method">The MFA method</param>
        /// <param name="sharedKey">The shared key for app-based MFA</param>
        /// <param name="recoveryEmail">The recovery email for email-based MFA</param>
        /// <param name="recoveryPhone">The recovery phone for SMS-based MFA</param>
        /// <returns>The MFA settings</returns>
        Task<AuthMfaSettings> EnableMfaAsync(string userId, string method, string? sharedKey = null, string? recoveryEmail = null, string? recoveryPhone = null);
        
        /// <summary>
        /// Disables MFA for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>A task</returns>
        Task DisableMfaAsync(string userId);
    }
    
    /// <summary>
    /// Repository interface for the backup code entity
    /// </summary>
    public interface IBackupCodeRepository : IBaseAuthRepository<AuthBackupCode>
    {
        /// <summary>
        /// Gets all backup codes for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The backup codes</returns>
        Task<IEnumerable<AuthBackupCode>> GetByUserIdAsync(string userId);
        
        /// <summary>
        /// Gets all unused backup codes for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The backup codes</returns>
        Task<IEnumerable<AuthBackupCode>> GetUnusedByUserIdAsync(string userId);
        
        /// <summary>
        /// Validates a backup code for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="code">The code</param>
        /// <returns>True if the code is valid, false otherwise</returns>
        Task<bool> ValidateCodeAsync(string userId, string code);
        
        /// <summary>
        /// Marks a backup code as used
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="code">The code</param>
        /// <returns>True if the code was found and marked as used, false otherwise</returns>
        Task<bool> MarkAsUsedAsync(string userId, string code);
        
        /// <summary>
        /// Generates new backup codes for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="count">The number of codes to generate</param>
        /// <returns>The backup codes</returns>
        Task<IEnumerable<AuthBackupCode>> GenerateCodesAsync(string userId, int count = 10);
        
        /// <summary>
        /// Deletes all backup codes for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>A task</returns>
        Task DeleteAllForUserAsync(string userId);
    }
    
    /// <summary>
    /// Repository interface for the MFA challenge entity
    /// </summary>
    public interface IMfaChallengeRepository : IBaseAuthRepository<AuthMfaChallenge>
    {
        /// <summary>
        /// Gets an MFA challenge by verification code
        /// </summary>
        /// <param name="code">The verification code</param>
        /// <returns>The MFA challenge</returns>
        Task<AuthMfaChallenge> GetByVerificationCodeAsync(string code);
        
        /// <summary>
        /// Gets all MFA challenges for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The MFA challenges</returns>
        Task<IEnumerable<AuthMfaChallenge>> GetByUserIdAsync(string userId);
        
        /// <summary>
        /// Gets all active MFA challenges for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The MFA challenges</returns>
        Task<IEnumerable<AuthMfaChallenge>> GetActiveByUserIdAsync(string userId);
        
        /// <summary>
        /// Creates a new MFA challenge
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="method">The MFA method</param>
        /// <param name="ipAddress">The IP address</param>
        /// <param name="deviceId">The device ID</param>
        /// <param name="expiresInMinutes">The expiration time in minutes</param>
        /// <returns>The MFA challenge</returns>
        Task<AuthMfaChallenge> CreateChallengeAsync(string userId, string method, string ipAddress, string deviceId, int expiresInMinutes = 15);
        
        /// <summary>
        /// Validates an MFA challenge
        /// </summary>
        /// <param name="code">The verification code</param>
        /// <param name="userId">The user ID</param>
        /// <param name="ipAddress">The IP address</param>
        /// <returns>True if the challenge is valid, false otherwise</returns>
        Task<bool> ValidateChallengeAsync(string code, string userId, string ipAddress);
        
        /// <summary>
        /// Marks an MFA challenge as used
        /// </summary>
        /// <param name="code">The verification code</param>
        /// <param name="userId">The user ID</param>
        /// <returns>True if the challenge was found and marked as used, false otherwise</returns>
        Task<bool> MarkAsUsedAsync(string code, string userId);
        
        /// <summary>
        /// Deletes all expired MFA challenges
        /// </summary>
        /// <returns>A task</returns>
        Task DeleteExpiredAsync();
    }
    
    /// <summary>
    /// Repository interface for the trusted device entity
    /// </summary>
    public interface ITrustedDeviceRepository : IBaseAuthRepository<AuthTrustedDevice>
    {
        /// <summary>
        /// Gets a trusted device by device ID
        /// </summary>
        /// <param name="deviceId">The device ID</param>
        /// <returns>The trusted device</returns>
        Task<AuthTrustedDevice> GetByDeviceIdAsync(string deviceId);
        
        /// <summary>
        /// Gets all trusted devices for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The trusted devices</returns>
        Task<IEnumerable<AuthTrustedDevice>> GetByUserIdAsync(string userId);
        
        /// <summary>
        /// Checks if a device is trusted for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="deviceId">The device ID</param>
        /// <returns>True if the device is trusted, false otherwise</returns>
        Task<bool> IsDeviceTrustedAsync(string userId, string deviceId);
        
        /// <summary>
        /// Adds a trusted device
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="deviceId">The device ID</param>
        /// <param name="deviceName">The device name</param>
        /// <param name="deviceType">The device type</param>
        /// <param name="operatingSystem">The operating system</param>
        /// <param name="browser">The browser</param>
        /// <param name="browserVersion">The browser version</param>
        /// <param name="ipAddress">The IP address</param>
        /// <param name="country">The country</param>
        /// <param name="city">The city</param>
        /// <param name="region">The region</param>
        /// <returns>The trusted device</returns>
        Task<AuthTrustedDevice> AddTrustedDeviceAsync(string userId, string deviceId, string deviceName, 
            string deviceType, string operatingSystem, string browser, string browserVersion, 
            string ipAddress, string? country = null, string? city = null, string? region = null);
        
        /// <summary>
        /// Removes a trusted device
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="deviceId">The device ID</param>
        /// <returns>A task</returns>
        Task RemoveTrustedDeviceAsync(string userId, string deviceId);
        
        /// <summary>
        /// Updates the last used date for a trusted device
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="deviceId">The device ID</param>
        /// <returns>A task</returns>
        Task UpdateLastUsedAsync(string userId, string deviceId);
    }
    
    /// <summary>
    /// Repository interface for the login attempt entity
    /// </summary>
    public interface ILoginAttemptRepository : IBaseAuthRepository<AuthLoginAttempt>
    {
        /// <summary>
        /// Gets all login attempts for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The login attempts</returns>
        Task<IEnumerable<AuthLoginAttempt>> GetByUserIdAsync(string userId);
        
        /// <summary>
        /// Gets login attempts for a user in a time range
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="startTime">The start time</param>
        /// <param name="endTime">The end time</param>
        /// <returns>The login attempts</returns>
        Task<IEnumerable<AuthLoginAttempt>> GetByUserIdAndTimeRangeAsync(string userId, DateTime startTime, DateTime endTime);
        
        /// <summary>
        /// Gets login attempts by username
        /// </summary>
        /// <param name="username">The username</param>
        /// <returns>The login attempts</returns>
        Task<IEnumerable<AuthLoginAttempt>> GetByUsernameAsync(string username);
        
        /// <summary>
        /// Gets login attempts by username in a time range
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="startTime">The start time</param>
        /// <param name="endTime">The end time</param>
        /// <returns>The login attempts</returns>
        Task<IEnumerable<AuthLoginAttempt>> GetByUsernameAndTimeRangeAsync(string username, DateTime startTime, DateTime endTime);
        
        /// <summary>
        /// Gets login attempts by IP address
        /// </summary>
        /// <param name="ipAddress">The IP address</param>
        /// <returns>The login attempts</returns>
        Task<IEnumerable<AuthLoginAttempt>> GetByIpAddressAsync(string ipAddress);
        
        /// <summary>
        /// Gets login attempts by IP address in a time range
        /// </summary>
        /// <param name="ipAddress">The IP address</param>
        /// <param name="startTime">The start time</param>
        /// <param name="endTime">The end time</param>
        /// <returns>The login attempts</returns>
        Task<IEnumerable<AuthLoginAttempt>> GetByIpAddressAndTimeRangeAsync(string ipAddress, DateTime startTime, DateTime endTime);
        
        /// <summary>
        /// Gets the count of failed login attempts for a username in a time range
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="timeRange">The time range in minutes</param>
        /// <returns>The count</returns>
        Task<int> GetFailedLoginAttemptsCountAsync(string username, int timeRange = 30);
        
        /// <summary>
        /// Records a login attempt
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="userId">The user ID</param>
        /// <param name="success">Whether the login was successful</param>
        /// <param name="failureReason">The failure reason</param>
        /// <param name="ipAddress">The IP address</param>
        /// <param name="userAgent">The user agent</param>
        /// <param name="loginMethod">The login method</param>
        /// <param name="country">The country</param>
        /// <param name="city">The city</param>
        /// <returns>The login attempt</returns>
        Task<AuthLoginAttempt> RecordLoginAttemptAsync(string username, string userId, bool success, string failureReason, 
            string ipAddress, string userAgent, string loginMethod, string? country = null, string? city = null);
    }
    
    /// <summary>
    /// Repository interface for the social login profile entity
    /// </summary>
    public interface ISocialLoginProfileRepository : IBaseAuthRepository<AuthSocialLoginProfile>
    {
        /// <summary>
        /// Gets all social login profiles for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The social login profiles</returns>
        Task<IEnumerable<AuthSocialLoginProfile>> GetByUserIdAsync(string userId);
        
        /// <summary>
        /// Gets a social login profile by provider and provider key
        /// </summary>
        /// <param name="provider">The provider</param>
        /// <param name="providerKey">The provider key</param>
        /// <returns>The social login profile</returns>
        Task<AuthSocialLoginProfile> GetByProviderAndKeyAsync(string provider, string providerKey);
        
        /// <summary>
        /// Adds a social login profile
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="provider">The provider</param>
        /// <param name="providerKey">The provider key</param>
        /// <param name="providerDisplayName">The provider display name</param>
        /// <returns>The social login profile</returns>
        Task<AuthSocialLoginProfile> AddSocialLoginAsync(string userId, string provider, string providerKey, string providerDisplayName);
        
        /// <summary>
        /// Removes a social login profile
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="provider">The provider</param>
        /// <returns>A task</returns>
        Task RemoveSocialLoginAsync(string userId, string provider);
        
        /// <summary>
        /// Updates the last used date for a social login profile
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="provider">The provider</param>
        /// <returns>A task</returns>
        Task UpdateLastUsedAsync(string userId, string provider);
    }
    
    /// <summary>
    /// Repository interface for the security alert entity
    /// </summary>
    public interface ISecurityAlertRepository : IBaseAuthRepository<AuthSecurityAlert>
    {
        /// <summary>
        /// Gets all security alerts for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The security alerts</returns>
        Task<IEnumerable<AuthSecurityAlert>> GetByUserIdAsync(string userId);
        
        /// <summary>
        /// Gets all unread security alerts for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The security alerts</returns>
        Task<IEnumerable<AuthSecurityAlert>> GetUnreadByUserIdAsync(string userId);
        
        /// <summary>
        /// Gets security alerts for a user by alert type
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="alertType">The alert type</param>
        /// <returns>The security alerts</returns>
        Task<IEnumerable<AuthSecurityAlert>> GetByUserIdAndTypeAsync(string userId, string alertType);
        
        /// <summary>
        /// Gets security alerts for a user by severity
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="severity">The severity</param>
        /// <returns>The security alerts</returns>
        Task<IEnumerable<AuthSecurityAlert>> GetByUserIdAndSeverityAsync(string userId, string severity);
        
        /// <summary>
        /// Creates a security alert
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="alertType">The alert type</param>
        /// <param name="message">The message</param>
        /// <param name="details">The details</param>
        /// <param name="severity">The severity</param>
        /// <param name="ipAddress">The IP address</param>
        /// <param name="deviceId">The device ID</param>
        /// <returns>The security alert</returns>
        Task<AuthSecurityAlert> CreateAlertAsync(string userId, string alertType, string message, string details, 
            string severity, string? ipAddress = null, string? deviceId = null);
        
        /// <summary>
        /// Marks a security alert as read
        /// </summary>
        /// <param name="alertId">The alert ID</param>
        /// <param name="userId">The user ID</param>
        /// <returns>True if the alert was found and marked as read, false otherwise</returns>
        Task<bool> MarkAsReadAsync(string alertId, string userId);
        
        /// <summary>
        /// Marks all security alerts as read for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>A task</returns>
        Task MarkAllAsReadAsync(string userId);
    }
    
    /// <summary>
    /// Repository interface for the user security preferences entity
    /// </summary>
    public interface IUserSecurityPreferencesRepository : IBaseAuthRepository<AuthUserSecurityPreferences>
    {
        /// <summary>
        /// Gets the security preferences for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The security preferences</returns>
        Task<AuthUserSecurityPreferences> GetByUserIdAsync(string userId);
        
        /// <summary>
        /// Updates the security preferences for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="preferences">The security preferences</param>
        /// <returns>The security preferences</returns>
        Task<AuthUserSecurityPreferences> UpdatePreferencesAsync(string userId, AuthUserSecurityPreferences preferences);
    }
}
