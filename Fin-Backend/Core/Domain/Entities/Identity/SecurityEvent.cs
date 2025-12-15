using System;
using System.ComponentModel.DataAnnotations;

namespace FinTech.Core.Domain.Entities.Identity
{
    public class SecurityEvent
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string EventType { get; set; }
        public string Description { get; set; }
        public string Username { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string Severity { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public static class SecurityEventSeverity
    {
        public const string Info = "Info";
        public const string Warning = "Warning";
        public const string Critical = "Critical";
    }
}
