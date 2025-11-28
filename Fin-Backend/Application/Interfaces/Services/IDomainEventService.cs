using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Domain.Common;

namespace FinTech.Core.Application.Interfaces.Services
{
    public interface IDomainEventService
    {
        Task PublishAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default);
    }
}
