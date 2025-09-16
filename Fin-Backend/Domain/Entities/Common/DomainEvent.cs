using System;

namespace FinTech.Domain.Entities.Common
{
    /// <summary>
    /// Base class for domain events
    /// </summary>
    public abstract class DomainEvent
    {
        protected DomainEvent()
        {
            EventId = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
        }
        
        public Guid EventId { get; protected set; }
        public DateTime OccurredOn { get; protected set; }
    }
}