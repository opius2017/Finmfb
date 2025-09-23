using System.Collections.Generic;
using FinTech.Domain.Entities.Common;

namespace FinTech.Domain.Entities.Common
{
    public interface IHasDomainEvent
    {
        public List<DomainEvent> DomainEvents { get; set; }
    }
}
