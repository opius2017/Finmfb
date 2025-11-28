using System.Collections.Generic;
using FinTech.Core.Domain.Common;

namespace FinTech.Core.Domain.Entities.Common
{
    /// <summary>
    /// Base class for aggregate roots in the domain, includes domain events collection
    /// </summary>
    public abstract class AggregateRoot : BaseEntity
    {
        private readonly List<DomainEvent> _domainEvents = new List<DomainEvent>();
        
        public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
        
        public void AddDomainEvent(DomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }
        
        public void RemoveDomainEvent(DomainEvent domainEvent)
        {
            _domainEvents.Remove(domainEvent);
        }
        
        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}
