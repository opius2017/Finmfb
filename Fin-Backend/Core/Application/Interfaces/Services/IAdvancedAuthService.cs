using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Auth;
using FinTech.Core.Domain.Entities.Identity;

namespace FinTech.Core.Application.Interfaces.Services
{
    public interface IAdvancedAuthService
    {
        Task<AuthResult> AuthenticateAsync(AuthRequest request);
        Task<AuthResult> RefreshTokenAsync(RefreshTokenRequest request);
        Task<bool> RevokeTokenAsync(RevokeTokenRequest request);
        Task<RegistrationResult> RegisterAsync(RegistrationRequest request);
        Task<MfaSetupResult> InitiateMfaSetupAsync(string userId, MfaMethod method);
        Task<bool> VerifyMfaSetupAsync(string userId, string code);
        Task<bool> DisableMfaAsync(string userId, string code);
        Task<MfaChallengeResult> InitiateMfaChallengeAsync(string userId);
        Task<AuthResult> VerifyMfaChallengeAsync(string userId, string code, string challengeId);
        Task<List<string>> GenerateBackupCodesAsync(string userId);
        Task<bool> ValidateBackupCodeAsync(string userId, string code);
        Task<bool> InitiatePasswordResetAsync(string email);
        Task<bool> VerifyPasswordResetAsync(PasswordResetVerificationRequest request);
        Task<List<AuthHistoryItem>> GetAuthHistoryAsync(string userId, int limit = 10);
        List<SocialLoginProviderDto> GetSocialLoginProviders();
        Task<SocialLoginInitiationResult> InitiateSocialLoginAsync(string provider, string returnUrl);
        Task<AuthResult> ProcessSocialLoginCallbackAsync(string provider, string code, string state, DeviceInfo deviceInfo = null);
        Task<bool> LinkSocialLoginAsync(string userId, string provider, string accessToken, string tokenSecret);
        Task<bool> UnlinkSocialLoginAsync(string userId, string provider);
        Task<List<LinkedSocialAccountDto>> GetLinkedSocialAccountsAsync(string userId);
        Task<string> AddTrustedDeviceAsync(string userId, DeviceInfo deviceInfo);
        Task<bool> IsTrustedDeviceAsync(string userId, string deviceId);
        Task<bool> RemoveTrustedDeviceAsync(string userId, string deviceId);
        Task<List<TrustedDeviceInfo>> GetTrustedDevicesAsync(string userId);
        Task<SecurityDashboardDto> GetSecurityDashboardAsync(string userId);
        Task<List<ClientSessionDto>> GetActiveSessionsAsync(string userId);
        Task<bool> RevokeAllSessionsExceptCurrentAsync(string userId, string currentSessionId);
        Task<bool> RevokeSessionAsync(string userId, string sessionId);
        Task<Microsoft.AspNetCore.Identity.IdentityResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<bool> RequestDataExportAsync(string userId);
        Task<AdvancedSecurityPreferencesDto> GetSecurityPreferencesAsync(string userId);
        Task<bool> UpdateSecurityPreferencesAsync(string userId, AdvancedSecurityPreferencesDto preferences);
    }
}
