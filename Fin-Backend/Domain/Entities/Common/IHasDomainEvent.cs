using System.Collections.Generic;
using FinTech.Core.Domain.Common;

namespace FinTech.Core.Domain.Entities.Common
{
    public interface IHasDomainEvent
    {
        public List<DomainEvent> DomainEvents { get; set; }
    }
}
