using System;
using System.Collections.Generic;

namespace FinTech.WebAPI.Application.DTOs.Auth
{
    public class MfaSetupRequestDto
    {
        public string UserId { get; set; }
    }

    public class MfaSetupResponseDto
    {
        public string Secret { get; set; }
        public string QrCodeUrl { get; set; }
        public List<string> BackupCodes { get; set; }
    }

    public class MfaVerifyRequestDto
    {
        public string Code { get; set; }
        public string Secret { get; set; }
    }

    public class MfaVerifyResponseDto
    {
        public bool Verified { get; set; }
        public string Token { get; set; }
    }

    public class MfaValidateRequestDto
    {
        public string UserId { get; set; }
        public string Code { get; set; }
        public string DeviceName { get; set; }
        public string DeviceType { get; set; }
        public string Browser { get; set; }
        public string OperatingSystem { get; set; }
        public string IpAddress { get; set; }
    }

    public class MfaValidateBackupRequestDto
    {
        public string UserId { get; set; }
        public string BackupCode { get; set; }
        public string DeviceName { get; set; }
        public string DeviceType { get; set; }
        public string Browser { get; set; }
        public string OperatingSystem { get; set; }
        public string IpAddress { get; set; }
    }

    public class MfaVerifyChallengeRequestDto
    {
        public string ChallengeId { get; set; }
        public string Code { get; set; }
    }

    public class MfaBackupCodeRequestDto
    {
        public string Code { get; set; }
        public string MfaToken { get; set; }
    }

    public class MfaBackupCodeResponseDto
    {
        public bool Success { get; set; }
        public string Token { get; set; }
    }

    public class MfaChallengeRequestDto
    {
        public string Operation { get; set; }
        public string UserId { get; set; }
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
        public string Id { get; set; }
        public string DeviceName { get; set; }
        public string DeviceType { get; set; }
        public string Browser { get; set; }
        public string OperatingSystem { get; set; }
        public DateTime LastUsed { get; set; }
        public string Location { get; set; }
        public string IpAddress { get; set; }
        public bool IsCurrent { get; set; }
    }

    public class SecurityActivityDto
    {
        public string Id { get; set; }
        public string EventType { get; set; }
        public DateTime Timestamp { get; set; }
        public string IpAddress { get; set; }
        public string Location { get; set; }
        public string DeviceInfo { get; set; }
        public string Status { get; set; }
        public string UserId { get; set; }
    }

    public class SecurityPreferencesDto
    {
        public bool EmailNotificationsEnabled { get; set; }
        public bool LoginNotificationsEnabled { get; set; }
        public bool UnusualActivityNotificationsEnabled { get; set; }
    }

    public class RevokeDevicesRequestDto
    {
        public string CurrentDeviceId { get; set; }
    }
}