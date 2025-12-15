using FinTech.Core.Domain.Common;

namespace FinTech.Core.Application.Interfaces.Events;

/// <summary>
/// Generic interface for domain event handlers
/// </summary>
/// <typeparam name="TEvent">The type of domain event to handle</typeparam>
public interface IDomainEventHandler<in TEvent> where TEvent : DomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}
