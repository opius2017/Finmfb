using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Entities.Common;
using System;

namespace FinTech.Core.Domain.Entities.ClientPortal
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
