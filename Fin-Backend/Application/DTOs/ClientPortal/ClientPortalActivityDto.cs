using System;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    public class ClientPortalActivityDto
    {
        public Guid Id { get; set; }
        public string ActivityType { get; set; }
        public string Page { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
