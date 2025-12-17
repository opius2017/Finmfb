using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Entities.Common;
using System;
using System.Collections.Generic;

namespace FinTech.Core.Domain.Entities.ClientPortal
{
    public class ClientSession : BaseEntity
    {
        public string ClientPortalProfileId { get; set; } = string.Empty;
        public ClientPortalProfile? ClientPortalProfile { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public string? Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? DeviceType { get; set; }
        public string? Browser { get; set; }
        public string? OperatingSystem { get; set; }
        public string? Location { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuccessful { get; set; }
        public string? FailureReason { get; set; }
    }
}
