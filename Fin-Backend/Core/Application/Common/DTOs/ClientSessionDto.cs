using System;

namespace FinTech.Core.Application.Common.DTOs
{
    public class ClientSessionDto
    {
        public string SessionId { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public DateTime LastAccessed { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsCurrent { get; set; }
    }
}
