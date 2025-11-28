namespace FinTech.Core.Application.DTOs.Auth
{
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
}
