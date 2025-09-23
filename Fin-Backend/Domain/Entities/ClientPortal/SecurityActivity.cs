using FinTech.Domain.Common;
using FinTech.Domain.Entities.Common;
using System;

namespace FinTech.Domain.Entities.ClientPortal
{
    public class SecurityActivity : BaseEntity
    {
        public string UserId { get; set; }
        public string ActivityType { get; set; }
        public DateTime Timestamp { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
    }
}
