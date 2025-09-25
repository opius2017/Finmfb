using System;
using System.Collections.Generic;

namespace FinTech.Infrastructure.Data.Interceptors
{
    public class AuditEntry
    {
        public string EntityName { get; set; }
        public string EntityId { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid? UserId { get; set; }
        public Guid? TenantId { get; set; }
        public Dictionary<string, object> Changes { get; set; }
    }
}
