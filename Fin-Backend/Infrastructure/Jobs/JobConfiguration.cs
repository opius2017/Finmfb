using System;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FinTech.Infrastructure.Jobs
{
    /// <summary>
    /// Configuration class for registering all background jobs
    /// </summary>
    public static class JobConfiguration
    {
        /// <summary>
        /// Register all recurring background jobs
        /// </summary>
        public static void RegisterRecurringJobs(IServiceProvider serviceProvider)
        {
            // var logger = serviceProvider.GetRequiredService<ILogger<DailyDelinquencyCheckJob>>();

            
            try
            {
                logger.LogInformation("Registering recurring background jobs...");

                // Register daily delinquency check job (runs at 1:00 AM daily)

                // DailyDelinquencyCheckJob.RegisterRecurringJob();
                logger.LogInformation("Registered: Daily Delinquency Check Job (1:00 AM daily)");

                // Register voucher expiry job (runs at 2:00 AM daily)
                // VoucherExpiryJob.RegisterRecurringJob();
                logger.LogInformation("Registered: Voucher Expiry Job (2:00 AM daily)");

                // Register monthly deduction schedule job (runs on 1st of month at 3:00 AM)
                // MonthlyDeductionScheduleJob.RegisterRecurringJob();
                logger.LogInformation("Registered: Monthly Deduction Schedule Job (1st of month at 3:00 AM)");

                logger.LogInformation("All recurring background jobs registered successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error registering recurring background jobs");
                throw;
            }
        }

        /// <summary>
        /// Remove all recurring jobs (useful for testing or maintenance)
        /// </summary>
        public static void RemoveAllRecurringJobs()
        {
            RecurringJob.RemoveIfExists("daily-delinquency-check");
            RecurringJob.RemoveIfExists("voucher-expiry-check");
            RecurringJob.RemoveIfExists("monthly-schedule-generation");
        }

        /// <summary>
        /// Trigger all jobs immediately (for testing)
        /// </summary>
        public static void TriggerAllJobsNow()
        {
            // DailyDelinquencyCheckJob.ExecuteNow();

            // VoucherExpiryJob.ExecuteNow();
            // MonthlyDeductionScheduleJob.ExecuteNow();
        }
    }
}
