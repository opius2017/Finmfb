using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Auth
{
    public class MfaSetupRequestDto
    {
        public string UserId { get; set; } = string.Empty;
    }

    public class MfaSetupResponseDto
    {
        public string Secret { get; set; } = string.Empty;
        public string QrCodeUrl { get; set; } = string.Empty;
        public List<string> BackupCodes { get; set; } = new List<string>();
    }

    public class MfaVerifyRequestDto
    {
        public string Code { get; set; } = string.Empty;
        public string Secret { get; set; } = string.Empty;
    }

    public class MfaVerifyResponseDto
    {
        public bool Verified { get; set; }
        public string Token { get; set; }
    }

    public class MfaValidateRequestDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public string Browser { get; set; } = string.Empty;
        public string OperatingSystem { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
    }

    public class MfaValidateBackupRequestDto
    {
        public string UserId { get; set; } = string.Empty;
        public string BackupCode { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public string Browser { get; set; } = string.Empty;
        public string OperatingSystem { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
    }

    public class MfaVerifyChallengeRequestDto
    {
        public string ChallengeId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class MfaBackupCodeRequestDto
    {
        public string Code { get; set; } = string.Empty;
        public string MfaToken { get; set; } = string.Empty;
    }

    public class MfaBackupCodeResponseDto
    {
        public bool Success { get; set; }
        public string Token { get; set; }
    }

    public class MfaChallengeRequestDto
    {
        public string Operation { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    public class MfaChallengeResponseDto
    {
        public bool Success { get; set; }
        public string ChallengeId { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class MfaDeviceInfoDto
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Browser { get; set; }
        public string OperatingSystem { get; set; }
        public string IpAddress { get; set; }
    }

    public class TrustedDeviceDto
    {
        public string Id { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public string Browser { get; set; } = string.Empty;
        public string OperatingSystem { get; set; } = string.Empty;
        public DateTime LastUsed { get; set; }
        public string Location { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public bool IsCurrent { get; set; }
    }

    public class SecurityActivityDto
    {
        public string Id { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string DeviceInfo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    public class SecurityPreferencesDto
    {
        public bool EmailNotificationsEnabled { get; set; }
        public bool LoginNotificationsEnabled { get; set; }
        public bool UnusualActivityNotificationsEnabled { get; set; }
    }

    public class RevokeDevicesRequestDto
    {
        public string CurrentDeviceId { get; set; } = string.Empty;
    }
    public class MfaLoginRequestDto
    {
        public string UserId { get; set; } = string.Empty;
        public string ChallengeId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public bool RememberDevice { get; set; }
    }

    public class MfaBackupLoginRequestDto
    {
        public string UserId { get; set; } = string.Empty;
        public string BackupCode { get; set; } = string.Empty;
        public bool RememberDevice { get; set; }
    }

    public class MfaStatusResponseDto
    {
        public bool IsEnabled { get; set; }
        public DateTime? LastVerified { get; set; }
    }
}
