using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Entities.Identity;
using System;
using System.Collections.Generic;

namespace FinTech.Core.Domain.Entities.ClientPortal
{
    public class ClientPortalProfile : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public ApplicationUser? Customer { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public int FailedLoginAttempts { get; set; }
        public int LoginCount { get; set; }
        public string? DeviceInfo { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public string? TwoFactorMethod { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public bool IsProfileComplete { get; set; }

        public Guid UserId { get; set; } // Link to Identity User
        public ICollection<ClientSession> Sessions { get; set; } = new List<ClientSession>();
        public ICollection<ClientDevice> Devices { get; set; } = new List<ClientDevice>();
        public NotificationPreferences? NotificationPreferences { get; set; }
        public DashboardPreferences? DashboardPreferences { get; set; }
    }
}
