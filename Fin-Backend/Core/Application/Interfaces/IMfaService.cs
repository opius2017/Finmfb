using FinTech.Core.Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinTech.Core.Application.Interfaces
{
    public interface IMfaService
    {
        /// <summary>
        /// Generates a new MFA setup including secret key, QR code URL, and backup codes
        /// </summary>
        Task<MfaSetupResponseDto> GenerateMfaSetupAsync(string userId);

        /// <summary>
        /// Verifies a code against the user's MFA secret during setup
        /// </summary>
        Task<bool> VerifyMfaSetupAsync(string userId, string code);

        /// <summary>
        /// Enables MFA for a user after successful verification
        /// </summary>
        Task EnableMfaAsync(string userId, string secret);

        /// <summary>
        /// Disables MFA for a user after code verification
        /// </summary>
        Task<bool> DisableMfaAsync(string userId, string code);

        /// <summary>
        /// Validates a MFA code during login
        /// </summary>
        Task<bool> ValidateMfaCodeAsync(string userId, string code, MfaDeviceInfoDto deviceInfo = null);

        /// <summary>
        /// Validates a backup code during account recovery
        /// </summary>
        Task<bool> ValidateBackupCodeAsync(string userId, string backupCode);

        /// <summary>
        /// Generates a new set of backup codes for a user
        /// </summary>
        Task<List<string>> RegenerateBackupCodesAsync(string userId, string currentCode);

        /// <summary>
        /// Creates a new MFA challenge for step-up authentication
        /// </summary>
        Task<MfaChallengeResponseDto> CreateMfaChallengeAsync(string userId, string operation);

        /// <summary>
        /// Verifies a MFA challenge code
        /// </summary>
        Task<bool> VerifyMfaChallengeAsync(string challengeId, string code);
        
        /// <summary>
        /// Adds a trusted device for a user
        /// </summary>
        Task<string> AddTrustedDeviceAsync(string userId, MfaDeviceInfoDto deviceInfo);
        
        /// <summary>
        /// Gets all trusted devices for a user
        /// </summary>
        Task<List<TrustedDeviceDto>> GetTrustedDevicesAsync(string userId);
        
        /// <summary>
        /// Revokes a specific trusted device
        /// </summary>
        Task<bool> RevokeTrustedDeviceAsync(string userId, string deviceId);
        
        /// <summary>
        /// Revokes all trusted devices except the current one
        /// </summary>
        Task<bool> RevokeAllTrustedDevicesExceptCurrentAsync(string userId, string currentDeviceId);
        
        /// <summary>
        /// Logs a security activity event
        /// </summary>
        Task LogSecurityActivityAsync(SecurityActivityDto activityDto);
        
        /// <summary>
        /// Gets security activity history for a user
        /// </summary>
        Task<List<SecurityActivityDto>> GetSecurityActivityAsync(string userId, int limit = 20);
        
        /// <summary>
        /// Updates security preferences for a user
        /// </summary>
        Task<bool> UpdateSecurityPreferencesAsync(string userId, SecurityPreferencesDto preferences);
        
        /// <summary>
        /// Gets security preferences for a user
        /// </summary>
        Task<SecurityPreferencesDto> GetSecurityPreferencesAsync(string userId);
    }
}
