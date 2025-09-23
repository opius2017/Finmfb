using System;

namespace FinTech.Domain.Entities.Common
{
    public class DomainEventRecord
    {
        public Guid Id { get; set; }
        public string EventType { get; set; }
        public string EntityName { get; set; }
        public string EntityId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Data { get; set; }
    }
}