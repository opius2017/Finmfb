using System;

namespace FinTech.Core.Domain.Entities.Common
{
    public class DomainEventRecord
    {
        public Guid Id { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Data { get; set; } = string.Empty;
    }
}
