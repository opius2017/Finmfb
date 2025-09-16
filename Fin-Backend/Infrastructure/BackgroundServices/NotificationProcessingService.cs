using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FinTech.Application.Services;

namespace FinTech.Infrastructure.BackgroundServices
{
    public class NotificationProcessingService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NotificationProcessingService> _logger;
        private readonly TimeSpan _processingInterval;

        public NotificationProcessingService(
            IServiceScopeFactory scopeFactory,
            ILogger<NotificationProcessingService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _processingInterval = TimeSpan.FromMinutes(1); // Process notifications every minute
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification Processing Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingNotificationsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing notifications.");
                }

                await Task.Delay(_processingInterval, stoppingToken);
            }

            _logger.LogInformation("Notification Processing Service is stopping.");
        }

        private async Task ProcessPendingNotificationsAsync()
        {
            _logger.LogInformation("Processing pending notifications...");

            using (var scope = _scopeFactory.CreateScope())
            {
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                await notificationService.ProcessPendingNotificationDeliveriesAsync();
            }

            _logger.LogInformation("Finished processing pending notifications.");
        }
    }
}