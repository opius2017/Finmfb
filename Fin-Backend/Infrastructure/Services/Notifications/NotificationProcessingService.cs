using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinTech.Infrastructure.Services
{
    public class NotificationProcessingService : BackgroundService
    {
        private readonly ILogger<NotificationProcessingService> _logger;

        public NotificationProcessingService(ILogger<NotificationProcessingService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification Processing Service running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                // Dummy work
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
