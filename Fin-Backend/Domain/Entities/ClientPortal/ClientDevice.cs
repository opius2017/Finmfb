using System;

namespace FinTech.Domain.Entities.ClientPortal
{
    public class ClientDevice
    {
        public Guid Id { get; set; }
        public Guid ClientPortalProfileId { get; set; }
        public ClientPortalProfile ClientPortalProfile { get; set; }
        public string DeviceId { get; set; }
        public string DeviceType { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastLoginDate { get; set; }
    }
}
