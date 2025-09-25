using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FinTech.Infrastructure.Monitoring
{
    public class PerformanceMonitoringMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PerformanceMonitoringMiddleware> _logger;
        private readonly TelemetryClient _telemetryClient;

        public PerformanceMonitoringMiddleware(
            RequestDelegate next,
            ILogger<PerformanceMonitoringMiddleware> logger,
            TelemetryClient telemetryClient)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var stopwatch = Stopwatch.StartNew();
            var requestPath = context.Request.Path;
            var method = context.Request.Method;
            
            try
            {
                // Track request size
                long? requestLength = context.Request.ContentLength;
                if (requestLength.HasValue)
                {
                    _telemetryClient.TrackMetric("RequestSize", requestLength.Value);
                }
                
                // Execute the request
                await _next(context);
                
                // Calculate and log response time
                stopwatch.Stop();
                var elapsedMs = stopwatch.ElapsedMilliseconds;
                
                // Log as metric to Application Insights
                _telemetryClient.TrackMetric("RequestDuration", elapsedMs);
                
                // Log response code distribution
                _telemetryClient.TrackMetric($"ResponseCode_{context.Response.StatusCode}", 1);
                
                // For slow requests, log additional details
                if (elapsedMs > 1000)
                {
                    _logger.LogWarning(
                        "Slow request detected: {Method} {Path} took {ElapsedMs}ms with status code {StatusCode}",
                        method, requestPath, elapsedMs, context.Response.StatusCode);
                }
                else
                {
                    _logger.LogInformation(
                        "Request: {Method} {Path} took {ElapsedMs}ms with status code {StatusCode}",
                        method, requestPath, elapsedMs, context.Response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                // Stop timer and log exception
                stopwatch.Stop();
                var elapsedMs = stopwatch.ElapsedMilliseconds;
                
                _logger.LogError(
                    ex,
                    "Request {Method} {Path} failed after {ElapsedMs}ms: {ErrorMessage}",
                    method, requestPath, elapsedMs, ex.Message);
                
                // Track exception in Application Insights
                _telemetryClient.TrackException(ex);
                
                // Re-throw to allow the exception middleware to handle it
                throw;
            }
        }
    }
}
