using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Auth
{
    /// <summary>
    /// DTO for social login provider information
    /// </summary>
    public class SocialLoginProviderDto
    {
        /// <summary>
        /// Gets or sets the provider name
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the provider display name
        /// </summary>
        public string DisplayName { get; set; }
        
        /// <summary>
        /// Gets or sets the provider icon URL
        /// </summary>
        public string IconUrl { get; set; }
        
        /// <summary>
        /// Gets or sets whether the provider is enabled
        /// </summary>
        public bool IsEnabled { get; set; }
    }

    /// <summary>
    /// DTO for linked social account information
    /// </summary>
    public class LinkedSocialAccountDto
    {
        /// <summary>
        /// Gets or sets the provider name
        /// </summary>
        public string Provider { get; set; }
        
        /// <summary>
        /// Gets or sets the provider display name
        /// </summary>
        public string ProviderDisplayName { get; set; }
        
        /// <summary>
        /// Gets or sets the external user ID
        /// </summary>
        public string ExternalUserId { get; set; }
        
        /// <summary>
        /// Gets or sets the external user name or email
        /// </summary>
        public string ExternalUserName { get; set; }
        
        /// <summary>
        /// Gets or sets the date when the account was linked
        /// </summary>
        public DateTime LinkedDate { get; set; }
    }

    /// <summary>
    /// DTO for device information
    /// </summary>
    public class DeviceInfo
    {
        /// <summary>
        /// Gets or sets the client IP address
        /// </summary>
        public string ClientIp { get; set; }
        
        /// <summary>
        /// Gets or sets the device ID
        /// </summary>
        public string DeviceId { get; set; }
        
        /// <summary>
        /// Gets or sets the browser information
        /// </summary>
        public string Browser { get; set; }
        
        /// <summary>
        /// Gets or sets the browser version
        /// </summary>
        public string BrowserVersion { get; set; }
        
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
    }

    /// <summary>
    /// DTO for social login initiation result
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
    /// DTO for authentication result
    /// </summary>
    public class AuthResult
    {
        /// <summary>
        /// Gets or sets whether the authentication was successful
        /// </summary>
        public bool Succeeded { get; set; }
        
        /// <summary>
        /// Gets or sets the error message if authentication failed
        /// </summary>
        public string ErrorMessage { get; set; }
        
        /// <summary>
        /// Gets or sets the JWT token
        /// </summary>
        public string Token { get; set; }
        
        /// <summary>
        /// Gets or sets the refresh token
        /// </summary>
        public string RefreshToken { get; set; }
        
        /// <summary>
        /// Gets or sets the token expiration date
        /// </summary>
        public DateTime TokenExpiration { get; set; }
        
        /// <summary>
        /// Gets or sets the user ID
        /// </summary>
        public string UserId { get; set; }
        
        /// <summary>
        /// Gets or sets the user name
        /// </summary>
        public string UserName { get; set; }
        
        /// <summary>
        /// Gets or sets the user email
        /// </summary>
        public string Email { get; set; }
        
        /// <summary>
        /// Gets or sets the user roles
        /// </summary>
        public List<string> Roles { get; set; }
        
        /// <summary>
        /// Gets or sets whether MFA is required
        /// </summary>
        public bool RequiresMfa { get; set; }
        
        /// <summary>
        /// Gets or sets the MFA challenge ID if MFA is required
        /// </summary>
        public string MfaChallengeId { get; set; }
    }

    /// <summary>
    /// DTO for authentication request
    /// </summary>
    public class AuthRequest
    {
        /// <summary>
        /// Gets or sets the username or email
        /// </summary>
        public string Username { get; set; }
        
        /// <summary>
        /// Gets or sets the password
        /// </summary>
        public string Password { get; set; }
        
        /// <summary>
        /// Gets or sets whether to remember the device
        /// </summary>
        public bool RememberDevice { get; set; }
        
        /// <summary>
        /// Gets or sets the device information
        /// </summary>
        public DeviceInfo DeviceInfo { get; set; }
    }

    /// <summary>
    /// DTO for refresh token request
    /// </summary>
    public class RefreshTokenRequest
    {
        /// <summary>
        /// Gets or sets the refresh token
        /// </summary>
        public string RefreshToken { get; set; }
        
        /// <summary>
        /// Gets or sets the device information
        /// </summary>
        public DeviceInfo DeviceInfo { get; set; }
    }

    /// <summary>
    /// DTO for token revocation request
    /// </summary>
    public class RevokeTokenRequest
    {
        /// <summary>
        /// Gets or sets the user ID
        /// </summary>
        public string UserId { get; set; }
        
        /// <summary>
        /// Gets or sets the refresh token
        /// </summary>
        public string RefreshToken { get; set; }
    }

    /// <summary>
    /// DTO for registration request
    /// </summary>
    public class RegistrationRequest
    {
        /// <summary>
        /// Gets or sets the email
        /// </summary>
        public string Email { get; set; }
        
        /// <summary>
        /// Gets or sets the username
        /// </summary>
        public string Username { get; set; }
        
        /// <summary>
        /// Gets or sets the password
        /// </summary>
        public string Password { get; set; }
        
        /// <summary>
        /// Gets or sets the password confirmation
        /// </summary>
        public string ConfirmPassword { get; set; }
        
        /// <summary>
        /// Gets or sets the first name
        /// </summary>
        public string FirstName { get; set; }
        
        /// <summary>
        /// Gets or sets the last name
        /// </summary>
        public string LastName { get; set; }
        
        /// <summary>
        /// Gets or sets the phone number
        /// </summary>
        public string PhoneNumber { get; set; }
    }

    /// <summary>
    /// DTO for registration result
    /// </summary>
    public class RegistrationResult
    {
        /// <summary>
        /// Gets or sets whether the registration was successful
        /// </summary>
        public bool Succeeded { get; set; }
        
        /// <summary>
        /// Gets or sets the user ID
        /// </summary>
        public string UserId { get; set; }
        
        /// <summary>
        /// Gets or sets the user name
        /// </summary>
        public string UserName { get; set; }
        
        /// <summary>
        /// Gets or sets the user email
        /// </summary>
        public string Email { get; set; }
        
        /// <summary>
        /// Gets or sets the error messages if registration failed
        /// </summary>
        public List<string> Errors { get; set; }
    }

    /// <summary>
    /// DTO for password reset verification request
    /// </summary>
    public class PasswordResetVerificationRequest
    {
        /// <summary>
        /// Gets or sets the email
        /// </summary>
        public string Email { get; set; }
        
        /// <summary>
        /// Gets or sets the token
        /// </summary>
        public string Token { get; set; }
        
        /// <summary>
        /// Gets or sets the new password
        /// </summary>
        public string NewPassword { get; set; }
        
        /// <summary>
        /// Gets or sets the new password confirmation
        /// </summary>
        public string ConfirmPassword { get; set; }
    }

    /// <summary>
    /// DTO for authentication history item
    /// </summary>
    public class AuthHistoryItem
    {
        /// <summary>
        /// Gets or sets the login date
        /// </summary>
        public DateTime LoginDate { get; set; }
        
        /// <summary>
        /// Gets or sets the IP address
        /// </summary>
        public string IpAddress { get; set; }
        
        /// <summary>
        /// Gets or sets the browser
        /// </summary>
        public string Browser { get; set; }
        
        /// <summary>
        /// Gets or sets the operating system
        /// </summary>
        public string OperatingSystem { get; set; }
        
        /// <summary>
        /// Gets or sets the device name
        /// </summary>
        public string Device { get; set; }
        
        /// <summary>
        /// Gets or sets the approximate location
        /// </summary>
        public string Location { get; set; }
        
        /// <summary>
        /// Gets or sets whether the login was successful
        /// </summary>
        public bool Successful { get; set; }
        
        /// <summary>
        /// Gets or sets whether MFA was used
        /// </summary>
        public bool UsedMfa { get; set; }
    }
}
