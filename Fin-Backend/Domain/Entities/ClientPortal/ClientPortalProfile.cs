using FinTech.Domain.Common;
using FinTech.Domain.Entities.Common;
using FinTech.Domain.Entities.Identity;
using System;
using System.Collections.Generic;

namespace FinTech.Domain.Entities.ClientPortal
{
    public class ClientPortalProfile : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public ApplicationUser? Customer { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public int FailedLoginAttempts { get; set; }
        public string? DeviceInfo { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public string? TwoFactorMethod { get; set; }
        public ICollection<ClientSession> Sessions { get; set; } = new List<ClientSession>();
        public ICollection<ClientDevice> Devices { get; set; } = new List<ClientDevice>();
    }
}
