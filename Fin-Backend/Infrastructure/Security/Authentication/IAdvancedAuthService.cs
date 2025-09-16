using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Fin_Backend.Infrastructure.Security.Authentication
{
    /// <summary>
    /// Advanced authentication service interface
    /// </summary>
    public interface IAdvancedAuthService
    {
        /// <summary>
        /// Authenticates a user with username and password
        /// </summary>
        /// <param name="request">The authentication request</param>
        /// <returns>The authentication result</returns>
        Task<AuthResult> AuthenticateAsync(AuthRequest request);
        
        /// <summary>
        /// Refreshes an authentication token
        /// </summary>
        /// <param name="request">The token refresh request</param>
        /// <returns>The authentication result</returns>
        Task<AuthResult> RefreshTokenAsync(RefreshTokenRequest request);
        
        /// <summary>
        /// Revokes a refresh token
        /// </summary>
        /// <param name="request">The token revocation request</param>
        /// <returns>True if the token was revoked successfully, otherwise false</returns>
        Task<bool> RevokeTokenAsync(RevokeTokenRequest request);
        
        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <param name="request">The registration request</param>
        /// <returns>The registration result</returns>
        Task<RegistrationResult> RegisterAsync(RegistrationRequest request);
        
        /// <summary>
        /// Initiates the MFA setup process
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="method">The MFA method</param>
        /// <returns>The MFA setup result</returns>
        Task<MfaSetupResult> InitiateMfaSetupAsync(string userId, MfaMethod method);
        
        /// <summary>
        /// Verifies the MFA setup
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="code">The verification code</param>
        /// <returns>True if the setup was verified successfully, otherwise false</returns>
        Task<bool> VerifyMfaSetupAsync(string userId, string code);
        
        /// <summary>
        /// Disables MFA for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="code">The verification code</param>
        /// <returns>True if MFA was disabled successfully, otherwise false</returns>
        Task<bool> DisableMfaAsync(string userId, string code);
        
        /// <summary>
        /// Initiates the MFA challenge
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The MFA challenge result</returns>
        Task<MfaChallengeResult> InitiateMfaChallengeAsync(string userId);
        
        /// <summary>
        /// Verifies the MFA challenge
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="code">The verification code</param>
        /// <param name="challengeId">The challenge ID</param>
        /// <returns>The authentication result</returns>
        Task<AuthResult> VerifyMfaChallengeAsync(string userId, string code, string challengeId);
        
        /// <summary>
        /// Generates backup codes for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The list of backup codes</returns>
        Task<List<string>> GenerateBackupCodesAsync(string userId);
        
        /// <summary>
        /// Validates a backup code for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="code">The backup code</param>
        /// <returns>True if the backup code is valid, otherwise false</returns>
        Task<bool> ValidateBackupCodeAsync(string userId, string code);
        
        /// <summary>
        /// Initiates the password reset process
        /// </summary>
        /// <param name="email">The user's email</param>
        /// <returns>True if the password reset was initiated successfully, otherwise false</returns>
        Task<bool> InitiatePasswordResetAsync(string email);
        
        /// <summary>
        /// Verifies the password reset
        /// </summary>
        /// <param name="request">The password reset verification request</param>
        /// <returns>True if the password reset was verified successfully, otherwise false</returns>
        Task<bool> VerifyPasswordResetAsync(PasswordResetVerificationRequest request);
        
        /// <summary>
        /// Gets the authentication history for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="limit">The maximum number of history items to return</param>
        /// <returns>The list of authentication history items</returns>
        Task<List<AuthHistoryItem>> GetAuthHistoryAsync(string userId, int limit = 10);
        
        /// <summary>
        /// Initiates the social login process
        /// </summary>
        /// <param name="provider">The social login provider</param>
        /// <param name="returnUrl">The return URL</param>
        /// <returns>The social login initiation result</returns>
        Task<SocialLoginInitiationResult> InitiateSocialLoginAsync(string provider, string returnUrl);
        
        /// <summary>
        /// Processes the social login callback
        /// </summary>
        /// <param name="provider">The social login provider</param>
        /// <param name="code">The authorization code</param>
        /// <param name="state">The state parameter</param>
        /// <returns>The authentication result</returns>
        Task<AuthResult> ProcessSocialLoginCallbackAsync(string provider, string code, string state);
        
        /// <summary>
        /// Adds a trusted device for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="deviceInfo">The device information</param>
        /// <returns>The trusted device ID</returns>
        Task<string> AddTrustedDeviceAsync(string userId, DeviceInfo deviceInfo);
        
        /// <summary>
        /// Validates if a device is trusted for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="deviceId">The device ID</param>
        /// <returns>True if the device is trusted, otherwise false</returns>
        Task<bool> IsTrustedDeviceAsync(string userId, string deviceId);
        
        /// <summary>
        /// Removes a trusted device for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="deviceId">The device ID</param>
        /// <returns>True if the device was removed successfully, otherwise false</returns>
        Task<bool> RemoveTrustedDeviceAsync(string userId, string deviceId);
        
        /// <summary>
        /// Gets the list of trusted devices for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The list of trusted devices</returns>
        Task<List<TrustedDeviceInfo>> GetTrustedDevicesAsync(string userId);
    }
    
    /// <summary>
    /// Authentication request
    /// </summary>
    public class AuthRequest
    {
        /// <summary>
        /// Gets or sets the username or email
        /// </summary>
        /// <example>john.doe@example.com</example>
        [Required]
        public string Username { get; set; }
        
        /// <summary>
        /// Gets or sets the password
        /// </summary>
        /// <example>P@ssw0rd123!</example>
        [Required]
        public string Password { get; set; }
        
        /// <summary>
        /// Gets or sets the device information
        /// </summary>
        public DeviceInfo DeviceInfo { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether to remember the device
        /// </summary>
        /// <example>true</example>
        public bool RememberDevice { get; set; }
    }
    
    /// <summary>
    /// Authentication result
    /// </summary>
    public class AuthResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the authentication was successful
        /// </summary>
        public bool Succeeded { get; set; }
        
        /// <summary>
        /// Gets or sets the access token
        /// </summary>
        public string AccessToken { get; set; }
        
        /// <summary>
        /// Gets or sets the refresh token
        /// </summary>
        public string RefreshToken { get; set; }
        
        /// <summary>
        /// Gets or sets the expiration time
        /// </summary>
        public DateTime Expiration { get; set; }
        
        /// <summary>
        /// Gets or sets the user ID
        /// </summary>
        public string UserId { get; set; }
        
        /// <summary>
        /// Gets or sets the username
        /// </summary>
        public string Username { get; set; }
        
        /// <summary>
        /// Gets or sets the user's email
        /// </summary>
        public string Email { get; set; }
        
        /// <summary>
        /// Gets or sets the user's roles
        /// </summary>
        public List<string> Roles { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether MFA is required
        /// </summary>
        public bool RequiresMfa { get; set; }
        
        /// <summary>
        /// Gets or sets the MFA challenge ID
        /// </summary>
        public string MfaChallengeId { get; set; }
        
        /// <summary>
        /// Gets or sets the error message
        /// </summary>
        public string ErrorMessage { get; set; }
        
        /// <summary>
        /// Gets or sets the trusted device ID
        /// </summary>
        public string TrustedDeviceId { get; set; }
    }
    
    /// <summary>
    /// Token refresh request
    /// </summary>
    public class RefreshTokenRequest
    {
        /// <summary>
        /// Gets or sets the refresh token
        /// </summary>
        [Required]
        public string RefreshToken { get; set; }
        
        /// <summary>
        /// Gets or sets the device information
        /// </summary>
        public DeviceInfo DeviceInfo { get; set; }
    }
    
    /// <summary>
    /// Token revocation request
    /// </summary>
    public class RevokeTokenRequest
    {
        /// <summary>
        /// Gets or sets the refresh token
        /// </summary>
        [Required]
        public string RefreshToken { get; set; }
        
        /// <summary>
        /// Gets or sets the user ID
        /// </summary>
        public string UserId { get; set; }
    }
    
    /// <summary>
    /// Registration request
    /// </summary>
    public class RegistrationRequest
    {
        /// <summary>
        /// Gets or sets the email
        /// </summary>
        /// <example>john.doe@example.com</example>
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        /// <summary>
        /// Gets or sets the username
        /// </summary>
        /// <example>johndoe</example>
        [Required]
        public string Username { get; set; }
        
        /// <summary>
        /// Gets or sets the password
        /// </summary>
        /// <example>P@ssw0rd123!</example>
        [Required]
        [MinLength(8)]
        public string Password { get; set; }
        
        /// <summary>
        /// Gets or sets the first name
        /// </summary>
        /// <example>John</example>
        [Required]
        public string FirstName { get; set; }
        
        /// <summary>
        /// Gets or sets the last name
        /// </summary>
        /// <example>Doe</example>
        [Required]
        public string LastName { get; set; }
        
        /// <summary>
        /// Gets or sets the phone number
        /// </summary>
        /// <example>+1234567890</example>
        public string PhoneNumber { get; set; }
    }
    
    /// <summary>
    /// Registration result
    /// </summary>
    public class RegistrationResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the registration was successful
        /// </summary>
        public bool Succeeded { get; set; }
        
        /// <summary>
        /// Gets or sets the user ID
        /// </summary>
        public string UserId { get; set; }
        
        /// <summary>
        /// Gets or sets the error messages
        /// </summary>
        public List<string> Errors { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether email confirmation is required
        /// </summary>
        public bool RequiresEmailConfirmation { get; set; }
        
        /// <summary>
        /// Gets or sets the email confirmation token
        /// </summary>
        [JsonIgnore]
        public string EmailConfirmationToken { get; set; }
    }
    
    /// <summary>
    /// MFA method
    /// </summary>
    public enum MfaMethod
    {
        /// <summary>
        /// Authenticator app
        /// </summary>
        App,
        
        /// <summary>
        /// Email
        /// </summary>
        Email,
        
        /// <summary>
        /// SMS
        /// </summary>
        Sms
    }
    
    /// <summary>
    /// MFA setup result
    /// </summary>
    public class MfaSetupResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the setup was initiated successfully
        /// </summary>
        public bool Succeeded { get; set; }
        
        /// <summary>
        /// Gets or sets the shared key for app-based MFA
        /// </summary>
        public string SharedKey { get; set; }
        
        /// <summary>
        /// Gets or sets the QR code URL for app-based MFA
        /// </summary>
        public string QrCodeUrl { get; set; }
        
        /// <summary>
        /// Gets or sets the verification code for email or SMS-based MFA
        /// </summary>
        [JsonIgnore]
        public string VerificationCode { get; set; }
        
        /// <summary>
        /// Gets or sets the error message
        /// </summary>
        public string ErrorMessage { get; set; }
    }
    
    /// <summary>
    /// MFA challenge result
    /// </summary>
    public class MfaChallengeResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the challenge was initiated successfully
        /// </summary>
        public bool Succeeded { get; set; }
        
        /// <summary>
        /// Gets or sets the challenge ID
        /// </summary>
        public string ChallengeId { get; set; }
        
        /// <summary>
        /// Gets or sets the MFA method
        /// </summary>
        public MfaMethod Method { get; set; }
        
        /// <summary>
        /// Gets or sets the masked delivery destination (e.g., m***@example.com)
        /// </summary>
        public string MaskedDeliveryDestination { get; set; }
        
        /// <summary>
        /// Gets or sets the verification code for email or SMS-based MFA
        /// </summary>
        [JsonIgnore]
        public string VerificationCode { get; set; }
        
        /// <summary>
        /// Gets or sets the error message
        /// </summary>
        public string ErrorMessage { get; set; }
    }
    
    /// <summary>
    /// Password reset verification request
    /// </summary>
    public class PasswordResetVerificationRequest
    {
        /// <summary>
        /// Gets or sets the email
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        /// <summary>
        /// Gets or sets the password reset token
        /// </summary>
        [Required]
        public string Token { get; set; }
        
        /// <summary>
        /// Gets or sets the new password
        /// </summary>
        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; }
    }
    
    /// <summary>
    /// Authentication history item
    /// </summary>
    public class AuthHistoryItem
    {
        /// <summary>
        /// Gets or sets the ID
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Gets or sets the user ID
        /// </summary>
        public string UserId { get; set; }
        
        /// <summary>
        /// Gets or sets the login time
        /// </summary>
        public DateTime LoginTime { get; set; }
        
        /// <summary>
        /// Gets or sets the IP address
        /// </summary>
        public string IpAddress { get; set; }
        
        /// <summary>
        /// Gets or sets the user agent
        /// </summary>
        public string UserAgent { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the login was successful
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Gets or sets the login method
        /// </summary>
        public string LoginMethod { get; set; }
        
        /// <summary>
        /// Gets or sets the location information
        /// </summary>
        public LocationInfo Location { get; set; }
        
        /// <summary>
        /// Gets or sets the device information
        /// </summary>
        public DeviceInfo Device { get; set; }
    }
    
    /// <summary>
    /// Location information
    /// </summary>
    public class LocationInfo
    {
        /// <summary>
        /// Gets or sets the country
        /// </summary>
        public string Country { get; set; }
        
        /// <summary>
        /// Gets or sets the city
        /// </summary>
        public string City { get; set; }
        
        /// <summary>
        /// Gets or sets the region
        /// </summary>
        public string Region { get; set; }
        
        /// <summary>
        /// Gets or sets the latitude
        /// </summary>
        public double? Latitude { get; set; }
        
        /// <summary>
        /// Gets or sets the longitude
        /// </summary>
        public double? Longitude { get; set; }
    }
    
    /// <summary>
    /// Device information
    /// </summary>
    public class DeviceInfo
    {
        /// <summary>
        /// Gets or sets the device type
        /// </summary>
        public string DeviceType { get; set; }
        
        /// <summary>
        /// Gets or sets the device name
        /// </summary>
        public string DeviceName { get; set; }
        
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
        /// Gets or sets the client IP address
        /// </summary>
        public string ClientIp { get; set; }
        
        /// <summary>
        /// Gets or sets the unique device identifier
        /// </summary>
        public string DeviceId { get; set; }
    }
    
    /// <summary>
    /// Social login initiation result
    /// </summary>
    public class SocialLoginInitiationResult
    {
        /// <summary>
        /// Gets or sets the authorization URL
        /// </summary>
        public string AuthorizationUrl { get; set; }
        
        /// <summary>
        /// Gets or sets the state parameter
        /// </summary>
        public string State { get; set; }
    }
    
    /// <summary>
    /// Trusted device information
    /// </summary>
    public class TrustedDeviceInfo
    {
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
        /// Gets or sets the last used date
        /// </summary>
        public DateTime LastUsed { get; set; }
        
        /// <summary>
        /// Gets or sets the created date
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Gets or sets the IP address
        /// </summary>
        public string IpAddress { get; set; }
        
        /// <summary>
        /// Gets or sets the location information
        /// </summary>
        public LocationInfo Location { get; set; }
    }
}