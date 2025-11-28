using FinTech.Core.Application.DTOs.Auth;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinTech.Core.Application.Interfaces
{
    public interface IMfaService
    {
        Task<MfaSetupResponseDto> GenerateMfaSetupAsync(string userId);
        Task<bool> VerifyMfaSetupAsync(string userId, string code);
        Task EnableMfaAsync(string userId, string secret);
        Task<bool> DisableMfaAsync(string userId, string code);
        Task<bool> ValidateMfaCodeAsync(string userId, string code, MfaDeviceInfoDto deviceInfo = null);
        Task<bool> ValidateBackupCodeAsync(string userId, string backupCode);
        Task<List<string>> RegenerateBackupCodesAsync(string userId, string currentCode);
        Task<MfaChallengeResponseDto> CreateMfaChallengeAsync(string userId, string operation);
        Task<bool> VerifyMfaChallengeAsync(string challengeId, string code);
        Task<string> AddTrustedDeviceAsync(string userId, MfaDeviceInfoDto deviceInfo);
        Task<List<TrustedDeviceDto>> GetTrustedDevicesAsync(string userId);
        Task<bool> RevokeTrustedDeviceAsync(string userId, string deviceId);
        Task<bool> RevokeAllTrustedDevicesExceptCurrentAsync(string userId, string currentDeviceId);
        Task LogSecurityActivityAsync(SecurityActivityDto activityDto);
        Task<List<SecurityActivityDto>> GetSecurityActivityAsync(string userId, int limit = 20);
        Task<bool> UpdateSecurityPreferencesAsync(string userId, SecurityPreferencesDto preferences);
        Task<SecurityPreferencesDto> GetSecurityPreferencesAsync(string userId);
    }
}
