using System;

namespace FinTech.Infrastructure.Data.Messaging
{
    /// <summary>
    /// Represents a message in the outbox that needs to be processed
    /// for reliable message delivery.
    /// </summary>
    public class OutboxMessage
    {
        /// <summary>
        /// Unique identifier for the outbox message
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// The fully qualified type name of the event
        /// </summary>
        public string EventType { get; set; }
        
        /// <summary>
        /// JSON-serialized content of the event
        /// </summary>
        public string Content { get; set; }
        
        /// <summary>
        /// When the message was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// When the message was processed, null if not yet processed
        /// </summary>
        public DateTime? ProcessedAt { get; set; }
        
        /// <summary>
        /// Error information if processing failed
        /// </summary>
        public string Error { get; set; }
    }
}