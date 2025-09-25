using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FinTech.Core.Application.Common.Interfaces;
using FinTech.Infrastructure.Extensions;

namespace FinTech.Infrastructure.Services.Background
{
    public class SessionCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SessionCleanupService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(1);

        public SessionCleanupService(
            IServiceProvider serviceProvider,
            ILogger<SessionCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Session Cleanup Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredSessionsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during session cleanup");
                }

                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("Session Cleanup Service stopped");
        }

        private async Task CleanupExpiredSessionsAsync()
        {
            _logger.LogInformation("Starting session cleanup");

            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
                var clientPortalSettings = scope.ServiceProvider.GetService<IOptions<ClientPortalSettings>>();

                var now = DateTime.UtcNow;

                // Find expired sessions that are still marked as active
                var expiredSessions = await dbContext.ClientSessions
                    .Where(s => s.IsActive && s.ExpiresAt < now)
                    .ToListAsync();

                if (expiredSessions.Any())
                {
                    _logger.LogInformation("Found {Count} expired sessions to clean up", expiredSessions.Count);

                    foreach (var session in expiredSessions)
                    {
                        session.IsActive = false;
                        session.LogoutTime = now;
                    }

                    await dbContext.SaveChangesAsync();
                }
                else
                {
                    _logger.LogInformation("No expired sessions found");
                }
            }
        }
    }
}
