using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Entities.Common;
using System;
using System.Collections.Generic;

namespace FinTech.Core.Domain.Entities.ClientPortal
{
    public class ClientSession : BaseEntity
    {
        public Guid ClientPortalProfileId { get; set; }
        public ClientPortalProfile? ClientPortalProfile { get; set; }
        public string? Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }

    public class ClientDevice : BaseEntity
    {
        public int ClientId { get; set; }
        public string DeviceId { get; set; }
        public string DeviceType { get; set; }
        public string PushNotificationToken { get; set; }
        public bool IsActive { get; set; }
    }
}
