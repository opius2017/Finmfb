namespace FinTech.Core.Application.DTOs.Auth
{
    public class MfaDeviceInfoDto
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Browser { get; set; }
        public string OperatingSystem { get; set; }
        public string IpAddress { get; set; }
    }
}
