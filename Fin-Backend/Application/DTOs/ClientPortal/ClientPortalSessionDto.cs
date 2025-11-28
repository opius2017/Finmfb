using System;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    public class ClientPortalSessionDto
    {
        public Guid Id { get; set; }
        public string SessionId { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public string IpAddress { get; set; }
        public string DeviceType { get; set; }
        public string Browser { get; set; }
        public string OperatingSystem { get; set; }
        public string Location { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuccessful { get; set; }
        public string FailureReason { get; set; }
    }
}
