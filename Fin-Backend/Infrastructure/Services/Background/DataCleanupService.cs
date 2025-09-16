using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FinTech.Application.Common.Interfaces;

namespace FinTech.Infrastructure.Services.Background
{
    public class DataCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DataCleanupService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromDays(1); // Run daily

        public DataCleanupService(
            IServiceProvider serviceProvider,
            ILogger<DataCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Data Cleanup Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupOldDataAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during data cleanup");
                }

                // Wait until tomorrow
                var delay = CalculateDelayUntilNextRun();
                await Task.Delay(delay, stoppingToken);
            }

            _logger.LogInformation("Data Cleanup Service stopped");
        }

        private TimeSpan CalculateDelayUntilNextRun()
        {
            // Calculate time until 3:00 AM tomorrow
            var now = DateTime.UtcNow;
            var nextRun = new DateTime(now.Year, now.Month, now.Day, 3, 0, 0, DateTimeKind.Utc).AddDays(1);
            return nextRun - now;
        }

        private async Task CleanupOldDataAsync()
        {
            _logger.LogInformation("Starting data cleanup process");

            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
                var now = DateTime.UtcNow;

                // Cleanup old notification delivery records (older than 90 days)
                await CleanupNotificationDeliveryRecordsAsync(dbContext, now.AddDays(-90));

                // Cleanup old client portal activities (older than 90 days)
                await CleanupClientPortalActivitiesAsync(dbContext, now.AddDays(-90));

                // Cleanup old inactive sessions (older than 30 days)
                await CleanupInactiveSessionsAsync(dbContext, now.AddDays(-30));

                // Cleanup read notifications (older than 30 days)
                await CleanupReadNotificationsAsync(dbContext, now.AddDays(-30));
            }

            _logger.LogInformation("Data cleanup process completed");
        }

        private async Task CleanupNotificationDeliveryRecordsAsync(IApplicationDbContext dbContext, DateTime cutoffDate)
        {
            try
            {
                var oldRecords = await dbContext.NotificationDeliveryRecords
                    .Where(r => r.CreatedAt < cutoffDate)
                    .ToListAsync();

                if (oldRecords.Any())
                {
                    _logger.LogInformation("Deleting {Count} old notification delivery records", oldRecords.Count);
                    
                    foreach (var record in oldRecords)
                    {
                        dbContext.NotificationDeliveryRecords.Remove(record);
                    }
                    
                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up notification delivery records");
            }
        }

        private async Task CleanupClientPortalActivitiesAsync(IApplicationDbContext dbContext, DateTime cutoffDate)
        {
            try
            {
                var oldActivities = await dbContext.ClientPortalActivities
                    .Where(a => a.ActivityDate < cutoffDate)
                    .ToListAsync();

                if (oldActivities.Any())
                {
                    _logger.LogInformation("Deleting {Count} old client portal activities", oldActivities.Count);
                    
                    foreach (var activity in oldActivities)
                    {
                        dbContext.ClientPortalActivities.Remove(activity);
                    }
                    
                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up client portal activities");
            }
        }

        private async Task CleanupInactiveSessionsAsync(IApplicationDbContext dbContext, DateTime cutoffDate)
        {
            try
            {
                var oldSessions = await dbContext.ClientSessions
                    .Where(s => !s.IsActive && s.LogoutTime.HasValue && s.LogoutTime.Value < cutoffDate)
                    .ToListAsync();

                if (oldSessions.Any())
                {
                    _logger.LogInformation("Deleting {Count} old inactive sessions", oldSessions.Count);
                    
                    foreach (var session in oldSessions)
                    {
                        dbContext.ClientSessions.Remove(session);
                    }
                    
                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up inactive sessions");
            }
        }

        private async Task CleanupReadNotificationsAsync(IApplicationDbContext dbContext, DateTime cutoffDate)
        {
            try
            {
                var oldNotifications = await dbContext.ClientNotifications
                    .Where(n => n.IsRead && n.ReadAt.HasValue && n.ReadAt.Value < cutoffDate)
                    .ToListAsync();

                if (oldNotifications.Any())
                {
                    _logger.LogInformation("Deleting {Count} old read notifications", oldNotifications.Count);
                    
                    foreach (var notification in oldNotifications)
                    {
                        dbContext.ClientNotifications.Remove(notification);
                    }
                    
                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up read notifications");
            }
        }
    }
}