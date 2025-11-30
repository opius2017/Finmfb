using System;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinTech.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Background service for monthly threshold rollover
    /// Runs on the 1st day of each month at 00:01 AM
    /// </summary>
    public class MonthlyRolloverJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MonthlyRolloverJob> _logger;
        private Timer _timer;
        
        public MonthlyRolloverJob(
            IServiceProvider serviceProvider,
            ILogger<MonthlyRolloverJob> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Monthly Rollover Job started");
            
            // Calculate time until next run (1st of next month at 00:01)
            var now = DateTime.UtcNow;
            var nextRun = GetNextRunTime(now);
            var timeUntilNextRun = nextRun - now;
            
            _logger.LogInformation(
                "Next rollover scheduled for: {NextRun} (in {Hours} hours)",
                nextRun, timeUntilNextRun.TotalHours);
            
            // Set up timer to run at the calculated time
            _timer = new Timer(
                async _ => await ExecuteRolloverAsync(),
                null,
                timeUntilNextRun,
                TimeSpan.FromDays(30)); // Run approximately monthly
            
            return Task.CompletedTask;
        }
        
        private async Task ExecuteRolloverAsync()
        {
            try
            {
                _logger.LogInformation("Starting monthly rollover process");
                
                using (var scope = _serviceProvider.CreateScope())
                {
                    var thresholdManager = scope.ServiceProvider.GetRequiredService<IThresholdManager>();
                    
                    await thresholdManager.ProcessMonthlyRolloverAsync();
                    
                    _logger.LogInformation("Monthly rollover completed successfully");
                }
                
                // Schedule next run
                var now = DateTime.UtcNow;
                var nextRun = GetNextRunTime(now);
                var timeUntilNextRun = nextRun - now;
                
                _logger.LogInformation(
                    "Next rollover scheduled for: {NextRun}",
                    nextRun);
                
                _timer?.Change(timeUntilNextRun, TimeSpan.FromDays(30));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during monthly rollover process");
            }
        }
        
        /// <summary>
        /// Calculate next run time (1st of next month at 00:01)
        /// </summary>
        private DateTime GetNextRunTime(DateTime from)
        {
            var nextMonth = from.AddMonths(1);
            var firstOfNextMonth = new DateTime(nextMonth.Year, nextMonth.Month, 1, 0, 1, 0, DateTimeKind.Utc);
            
            // If we're already past the 1st of current month, use next month
            var firstOfCurrentMonth = new DateTime(from.Year, from.Month, 1, 0, 1, 0, DateTimeKind.Utc);
            
            if (from < firstOfCurrentMonth)
            {
                return firstOfCurrentMonth;
            }
            
            return firstOfNextMonth;
        }
        
        public override void Dispose()
        {
            _timer?.Dispose();
            base.Dispose();
        }
    }
}
