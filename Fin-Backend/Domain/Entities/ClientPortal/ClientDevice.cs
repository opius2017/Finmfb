using FinTech.Domain.Common;
using System;

namespace FinTech.Domain.Entities.ClientPortal
{
    public class ClientDevice : BaseEntity
    {
        public Guid ClientPortalProfileId { get; set; }
        public ClientPortalProfile? ClientPortalProfile { get; set; }
        public string? DeviceId { get; set; }
        public string? DeviceType { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastLoginDate { get; set; }
        public string? PushNotificationToken { get; set; }
    }
}
