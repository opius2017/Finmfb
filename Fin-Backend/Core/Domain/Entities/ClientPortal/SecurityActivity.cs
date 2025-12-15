using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Entities.Common;
using System;

namespace FinTech.Core.Domain.Entities.ClientPortal
{
    public class SecurityActivity : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string ActivityType { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Location { get; set; }
        public string? Status { get; set; }
    }
}
