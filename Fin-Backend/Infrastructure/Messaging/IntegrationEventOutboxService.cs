using FinTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace FinTech.Infrastructure.Messaging
{
    /// <summary>
    /// Implementation of the integration event outbox service
    /// </summary>
    public class IntegrationEventOutboxService : IIntegrationEventOutboxService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IEventBus _eventBus;
        private readonly ILogger<IntegrationEventOutboxService> _logger;

        /// <summary>
        /// Creates a new integration event outbox service
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="eventBus">The event bus</param>
        /// <param name="logger">The logger</param>
        public IntegrationEventOutboxService(
            ApplicationDbContext dbContext,
            IEventBus eventBus,
            ILogger<IntegrationEventOutboxService> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Publishes all pending events from the outbox
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task PublishEventsAsync()
        {
            var pendingEvents = await _dbContext.IntegrationEventOutbox
                .Where(e => e.State == IntegrationEventState.NotPublished)
                .OrderBy(e => e.CreationTime)
                .Take(50) // Process in batches
                .ToListAsync();

            foreach (var outboxEvent in pendingEvents)
            {
                try
                {
                    var eventType = Type.GetType(outboxEvent.EventTypeFullName);
                    if (eventType == null)
                    {
                        _logger.LogError("Event type {EventType} not found", outboxEvent.EventTypeFullName);
                        outboxEvent.State = IntegrationEventState.Failed;
                        outboxEvent.ErrorMessage = $"Event type {outboxEvent.EventTypeFullName} not found";
                        continue;
                    }

                    var integrationEvent = JsonSerializer.Deserialize(outboxEvent.Content, eventType, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) as IIntegrationEvent;

                    if (integrationEvent == null)
                    {
                        _logger.LogError("Failed to deserialize event {EventId}", outboxEvent.EventId);
                        outboxEvent.State = IntegrationEventState.Failed;
                        outboxEvent.ErrorMessage = "Failed to deserialize event";
                        continue;
                    }

                    try
                    {
                        await _eventBus.PublishAsync(integrationEvent);
                        outboxEvent.State = IntegrationEventState.Published;
                        outboxEvent.PublishTime = DateTime.UtcNow;
                        _logger.LogInformation("Published event {EventId} ({EventName})", outboxEvent.EventId, outboxEvent.EventTypeName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to publish event {EventId}", outboxEvent.EventId);
                        outboxEvent.State = IntegrationEventState.PublishingError;
                        outboxEvent.ErrorMessage = ex.Message;
                        outboxEvent.RetryCount++;
                        outboxEvent.LastRetryTime = DateTime.UtcNow;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing event {EventId}", outboxEvent.EventId);
                    outboxEvent.State = IntegrationEventState.Failed;
                    outboxEvent.ErrorMessage = ex.Message;
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Saves an integration event to the outbox
        /// </summary>
        /// <param name="event">The integration event</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task SaveEventAsync(IIntegrationEvent @event)
        {
            var eventType = @event.GetType();
            var outboxEvent = new IntegrationEventOutboxItem
            {
                EventId = @event.Id,
                CreationTime = @event.CreationDate,
                Content = JsonSerializer.Serialize(@event, eventType, new JsonSerializerOptions { WriteIndented = true }),
                State = IntegrationEventState.NotPublished,
                EventTypeFullName = eventType.AssemblyQualifiedName,
                EventTypeName = eventType.Name,
                RetryCount = 0
            };

            await _dbContext.IntegrationEventOutbox.AddAsync(outboxEvent);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Event {EventId} ({EventName}) saved to outbox", @event.Id, eventType.Name);
        }
    }

    /// <summary>
    /// Integration event state
    /// </summary>
    public enum IntegrationEventState
    {
        /// <summary>
        /// Not yet published
        /// </summary>
        NotPublished = 0,
        
        /// <summary>
        /// Published successfully
        /// </summary>
        Published = 1,
        
        /// <summary>
        /// Error during publishing
        /// </summary>
        PublishingError = 2,
        
        /// <summary>
        /// Failed to process
        /// </summary>
        Failed = 3
    }

    /// <summary>
    /// Integration event outbox item
    /// </summary>
    public class IntegrationEventOutboxItem
    {
        /// <summary>
        /// Gets or sets the event ID
        /// </summary>
        public Guid EventId { get; set; }
        
        /// <summary>
        /// Gets or sets the event type name
        /// </summary>
        public string EventTypeName { get; set; }
        
        /// <summary>
        /// Gets or sets the full name of the event type
        /// </summary>
        public string EventTypeFullName { get; set; }
        
        /// <summary>
        /// Gets or sets the event content
        /// </summary>
        public string Content { get; set; }
        
        /// <summary>
        /// Gets or sets the event state
        /// </summary>
        public IntegrationEventState State { get; set; }
        
        /// <summary>
        /// Gets or sets the retry count
        /// </summary>
        public int RetryCount { get; set; }
        
        /// <summary>
        /// Gets or sets the time when the event was created
        /// </summary>
        public DateTime CreationTime { get; set; }
        
        /// <summary>
        /// Gets or sets the time when the event was published
        /// </summary>
        public DateTime? PublishTime { get; set; }
        
        /// <summary>
        /// Gets or sets the last time when the event was retried
        /// </summary>
        public DateTime? LastRetryTime { get; set; }
        
        /// <summary>
        /// Gets or sets the error message
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
