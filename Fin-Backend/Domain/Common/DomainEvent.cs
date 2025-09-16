using System;

namespace FinTech.Domain.Common
{
    public abstract class DomainEvent
    {
        public Guid Id { get; }
        public DateTime TimeStamp { get; }

        protected DomainEvent()
        {
            Id = Guid.NewGuid();
            TimeStamp = DateTime.UtcNow;
        }
    }
}