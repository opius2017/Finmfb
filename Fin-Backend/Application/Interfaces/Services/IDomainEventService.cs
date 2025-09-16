using System.Threading;
using System.Threading.Tasks;
using FinTech.Domain.Common;

namespace FinTech.Application.Interfaces.Services
{
    public interface IDomainEventService
    {
        Task PublishAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default);
    }
}