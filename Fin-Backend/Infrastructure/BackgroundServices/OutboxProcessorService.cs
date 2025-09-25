using FinTech.Core.Domain.Entities.Common;
using FinTech.Infrastructure.Data;
using Microsoft.ApplicationInsights;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Application.Interfaces.Services;

namespace FinTech.Infrastructure.BackgroundServices
{
    public class OutboxProcessorService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OutboxProcessorService> _logger;
        private readonly TelemetryClient _telemetryClient;
        private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(10);
        private readonly int _batchSize = 50;

        public OutboxProcessorService(
            IServiceScopeFactory scopeFactory,
            ILogger<OutboxProcessorService> logger,
            TelemetryClient telemetryClient)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _telemetryClient = telemetryClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Outbox Processor Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessOutboxMessages(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing outbox messages");
                    _telemetryClient.TrackException(ex);
                }

                await Task.Delay(_pollingInterval, stoppingToken);
            }

            _logger.LogInformation("Outbox Processor Service stopped");
        }

        private async Task ProcessOutboxMessages(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var domainEventService = scope.ServiceProvider.GetRequiredService<IDomainEventService>();
            
            // Get unprocessed messages in batches
            var messages = await dbContext.OutboxMessages
                .Where(m => m.ProcessedAt == null)
                .OrderBy(m => m.CreatedAt)
                .Take(_batchSize)
                .ToListAsync(stoppingToken);

            if (!messages.Any())
            {
                return; // No messages to process
            }

            _logger.LogInformation("Processing {MessageCount} outbox messages", messages.Count);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            foreach (var message in messages)
            {
                try
                {
                    var messageStopwatch = System.Diagnostics.Stopwatch.StartNew();
                    
                    // Deserialize and process the event
                    Type eventType = Type.GetType(message.EventType);
                    if (eventType == null)
                    {
                        _logger.LogWarning("Could not resolve type {EventType}", message.EventType);
                        message.Error = $"Could not resolve type {message.EventType}";
                        message.ProcessedAt = DateTime.UtcNow;
                        continue;
                    }

                    var domainEvent = JsonConvert.DeserializeObject(message.Content, eventType);
                    if (domainEvent == null)
                    {
                        _logger.LogWarning("Could not deserialize event {EventId} of type {EventType}", 
                            message.Id, message.EventType);
                        message.Error = "Could not deserialize event content";
                        message.ProcessedAt = DateTime.UtcNow;
                        continue;
                    }

                    // Process the event
                    await domainEventService.PublishAsync(domainEvent, stoppingToken);
                    
                    // Mark as processed
                    message.ProcessedAt = DateTime.UtcNow;
                    
                    messageStopwatch.Stop();
                    _telemetryClient.TrackMetric($"OutboxProcessor.ProcessMessage.Duration", messageStopwatch.ElapsedMilliseconds);
                    
                    _logger.LogInformation("Processed outbox message {MessageId} of type {EventType} in {ElapsedMs}ms", 
                        message.Id, eventType.Name, messageStopwatch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing outbox message {MessageId}", message.Id);
                    _telemetryClient.TrackException(ex);
                    
                    message.Error = ex.ToString();
                    message.ProcessedAt = DateTime.UtcNow; // Mark as processed with error
                }
            }

            // Save the updated messages
            await dbContext.SaveChangesAsync(stoppingToken);
            
            stopwatch.Stop();
            _telemetryClient.TrackMetric("OutboxProcessor.BatchProcessing.Duration", stopwatch.ElapsedMilliseconds);
            _telemetryClient.TrackMetric("OutboxProcessor.BatchProcessing.MessageCount", messages.Count);
            
            _logger.LogInformation("Processed {MessageCount} outbox messages in {ElapsedMs}ms", 
                messages.Count, stopwatch.ElapsedMilliseconds);
        }
    }
}
