using System;

namespace FinTech.Infrastructure.Messaging
{
    /// <summary>
    /// Represents an integration event that needs to be published to external systems.
    /// Used for implementing the outbox pattern for reliable message delivery.
    /// </summary>
    public class IntegrationEventOutboxItem
    {
        /// <summary>
        /// Unique identifier for the outbox item
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// The type name of the integration event
        /// </summary>
        public string EventType { get; set; }
        
        /// <summary>
        /// JSON-serialized content of the event
        /// </summary>
        public string Content { get; set; }
        
        /// <summary>
        /// When the outbox item was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// When the outbox item was processed, null if not yet processed
        /// </summary>
        public DateTime? ProcessedAt { get; set; }
        
        /// <summary>
        /// Number of processing attempts
        /// </summary>
        public int ProcessingAttempts { get; set; }
        
        /// <summary>
        /// When the last processing attempt was made
        /// </summary>
        public DateTime? LastProcessingAttempt { get; set; }
        
        /// <summary>
        /// Error information if processing failed
        /// </summary>
        public string Error { get; set; }
    }
}