using System;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Application.Interfaces.Services;
using FinTech.Core.Domain.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FinTech.Infrastructure.Services
{
    public class DomainEventService : IDomainEventService
    {
        private readonly ILogger<DomainEventService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public DomainEventService(ILogger<DomainEventService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task PublishAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Publishing domain event. Event - {event}", domainEvent.GetType().Name);
            
            // Get the handler type for this domain event
            Type handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
            
            // Get all handlers for this domain event
            dynamic handlers = _serviceProvider.GetServices(handlerType);
            
            foreach (var handler in handlers)
            {
                try
                {
                    await handler.HandleAsync((dynamic)domainEvent, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling domain event {EventType}", domainEvent.GetType().Name);
                }
            }
        }
    }
}
