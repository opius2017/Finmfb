using System;

namespace FinTech.Infrastructure.Messaging
{
    /// <summary>
    /// Represents an integration event that needs to be published to external systems.
    /// Used for implementing the outbox pattern for reliable message delivery.
    /// </summary>
    public class IntegrationEventOutboxItem
    {
        public Guid EventId { get; set; }
        public string EventTypeFullName { get; set; } = string.Empty;
        public string EventTypeName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreationTime { get; set; }
        public IntegrationEventState State { get; set; }
        public int RetryCount { get; set; }
        public DateTime? LastRetryTime { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public DateTime? PublishTime { get; set; }
    }
}
