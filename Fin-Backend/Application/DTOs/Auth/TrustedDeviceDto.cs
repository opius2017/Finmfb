using System;

namespace FinTech.Core.Application.DTOs.Auth
{
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
}
