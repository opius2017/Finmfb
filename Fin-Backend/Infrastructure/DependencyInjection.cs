using FinTech.Core.Application.Services.Loans;
using FinTech.Infrastructure.Jobs;
using FinTech.Infrastructure.Services;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace FinTech.Infrastructure
{
    /// <summary>
    /// Extension methods for registering infrastructure services
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Add infrastructure services to the service collection
        /// </summary>
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // Register integration services
            services.AddScoped<ExcelExportService>();
            services.AddScoped<ExcelImportService>();
            services.AddScoped<QRCodeService>();

            // Register background jobs
            services.AddScoped<DailyDelinquencyCheckJob>();
            services.AddScoped<VoucherExpiryJob>();
            services.AddScoped<MonthlyDeductionScheduleJob>();

            // Configure Hangfire
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(
                    configuration.GetConnectionString("HangfireConnection"),
                    new SqlServerStorageOptions
                    {
                        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                        QueuePollInterval = TimeSpan.Zero,
                        UseRecommendedIsolationLevel = true,
                        DisableGlobalLocks = true
                    }));

            // Add Hangfire server
            services.AddHangfireServer(options =>
            {
                options.WorkerCount = Environment.ProcessorCount * 2;
                options.Queues = new[] { "default", "critical", "low" };
            });

            return services;
        }

        /// <summary>
        /// Initialize background jobs
        /// </summary>
        public static void InitializeBackgroundJobs(IServiceProvider serviceProvider)
        {
            JobConfiguration.RegisterRecurringJobs(serviceProvider);
        }
    }
}
