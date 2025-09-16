using System;

namespace FinTech.Infrastructure.Data.Events
{
    /// <summary>
    /// Represents a record of a domain event that has occurred in the system.
    /// Used for auditing and tracking domain events.
    /// </summary>
    public class DomainEventRecord
    {
        /// <summary>
        /// Unique identifier for the event record
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// The type name of the event
        /// </summary>
        public string EventType { get; set; }
        
        /// <summary>
        /// The name of the entity that raised the event
        /// </summary>
        public string EntityName { get; set; }
        
        /// <summary>
        /// The ID of the entity that raised the event
        /// </summary>
        public string EntityId { get; set; }
        
        /// <summary>
        /// When the event was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// JSON-serialized data of the event
        /// </summary>
        public string Data { get; set; }
    }
}