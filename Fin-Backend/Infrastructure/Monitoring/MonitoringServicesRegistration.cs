using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Filters;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;
using System;

namespace FinTech.Infrastructure.Monitoring
{
    public static class MonitoringServicesRegistration
    {
        public static IServiceCollection AddMonitoringServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure Application Insights
            services.AddApplicationInsightsTelemetry(options =>
            {
                options.ConnectionString = configuration["ApplicationInsights:ConnectionString"];
                options.EnableAdaptiveSampling = false;
                options.EnableQuickPulseMetricStream = true;
            });
            
            // Configure Application Insights dependency tracking
            services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, o) =>
            {
                module.EnableSqlCommandTextInstrumentation = true;
            });
            
            // Add Serilog
            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithProperty("Application", "FinTech.API")
                .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.StaticFiles"))
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("System", LogEventLevel.Information)
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File(
                    path: "logs/fintech-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    fileSizeLimitBytes: 10 * 1024 * 1024,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")
                .WriteTo.ApplicationInsights(
                    configuration["ApplicationInsights:ConnectionString"],
                    new TraceTelemetryConverter())
                .CreateLogger();
                
            // Register Serilog as the logger factory
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog(logger, dispose: true);
            });
            
            // Add performance monitoring
            services.AddSingleton<ITelemetryInitializer, CustomTelemetryInitializer>();
            services.AddSingleton<Microsoft.ApplicationInsights.AspNetCore.Extensions.ApplicationInsightsServiceOptions>();
            
            // Add health checks
            services.AddHealthChecks()
                .AddDbContextCheck<Infrastructure.Data.ApplicationDbContext>("Database")
                .AddCheck<MemoryHealthCheck>("Memory")
                .AddCheck<ApiDependencyHealthCheck>("External APIs");
                
            return services;
        }
    }
}
