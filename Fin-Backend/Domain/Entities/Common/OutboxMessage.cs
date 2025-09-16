using System;

namespace FinTech.Domain.Entities.Common
{
    public class OutboxMessage
    {
        public Guid Id { get; set; }
        public string EventType { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string Error { get; set; }
    }
}