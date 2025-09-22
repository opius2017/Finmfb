using FinTech.Domain.Common;
using System;
using System.Collections.Generic;

namespace FinTech.Domain.Entities.ClientPortal
{
    public class ClientSession : BaseEntity
    {
        public int ClientId { get; set; }
        public string SessionToken { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
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
