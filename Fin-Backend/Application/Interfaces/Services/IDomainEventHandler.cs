using System.Threading;
using System.Threading.Tasks;
using FinTech.Domain.Entities.Common;

namespace FinTech.Application.Interfaces.Services
{
    public interface IDomainEventHandler<in TEvent> where TEvent : DomainEvent
    {
        Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
    }
}