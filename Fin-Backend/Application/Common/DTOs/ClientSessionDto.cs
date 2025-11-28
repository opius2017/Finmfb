using System;

namespace FinTech.Core.Application.Common.DTOs
{
    public class ClientSessionDto
    {
        public string SessionId { get; set; }
        public string DeviceId { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public DateTime LastAccessed { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsCurrent { get; set; }
    }
}
