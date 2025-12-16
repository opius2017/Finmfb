using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Retry;
using Polly.Timeout;
using System;
using System.Net.Http;

namespace FinTech.Infrastructure.Resilience
{
    public static class ResiliencePolicies
    {
        /// <summary>
        /// Adds resilience policies to HTTP clients using Polly
        /// </summary>
        public static IServiceCollection AddResiliencePolicies(this IServiceCollection services)
        {
            // Define a retry policy with exponential backoff
            static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount = 3)
            {
                return HttpPolicyExtensions
                    .HandleTransientHttpError() // HttpRequestException, 5XX and 408 status codes
                    .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests) // 429 status code
                    .WaitAndRetryAsync(
                        retryCount: retryCount,
                        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                        onRetry: (outcome, timespan, retryAttempt, context) =>
                        {
                            // Log retry attempts
                            /*
                            if (logger != null)
                            {
                                logger.LogWarning("Retrying request {RequestUri} due to {Reason}. Attempt {Attempt}",
                                    outcome?.Result?.RequestMessage?.RequestUri,
                                    outcome?.Exception?.Message ?? $"Status code {outcome?.Result?.StatusCode}",
                                    retryAttempt);
                            }
                            */
                        });
            }

            // Define a circuit breaker policy
            static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(int exceptionsAllowedBeforeBreaking = 5)
            {
                return HttpPolicyExtensions
                    .HandleTransientHttpError() // HttpRequestException, 5XX and 408 status codes
                    .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests) // 429 status code
                    .CircuitBreakerAsync(
                        handledEventsAllowedBeforeBreaking: exceptionsAllowedBeforeBreaking,
                        durationOfBreak: TimeSpan.FromSeconds(30),
                        onBreak: (outcome, timespan, context) =>
                        {
                            /*
                            if (logger != null)
                            {
                                logger.LogWarning("Circuit breaker opened for {Duration}ms due to {Reason}",
                                    timespan.TotalMilliseconds,
                                    outcome?.Exception?.Message ?? $"Status code {outcome?.Result?.StatusCode}");
                            }
                            */
                        },
                        onReset: (context) =>
                        {
                            /*
                            if (logger != null)
                            {
                                logger.LogInformation("Circuit breaker reset");
                            }
                            */
                        },
                        onHalfOpen: () =>
                        {
                            /*
                            if (logger != null)
                            {
                                logger.LogInformation("Circuit breaker half-open");
                            }
                            */
                        });
            }

            // Define a timeout policy
            static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
            {
                return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));
            }

            // Define a bulkhead policy to limit concurrent requests
            static IAsyncPolicy<HttpResponseMessage> GetBulkheadPolicy()
            {
                return Policy.BulkheadAsync<HttpResponseMessage>(
                    maxParallelization: 20,
                    maxQueuingActions: 10,
                    onBulkheadRejectedAsync: context =>
                    {
                        /*
                        var logger = context.GetLogger();
                        if (logger != null)
                        {
                            logger.LogWarning("Bulkhead rejected request");
                        }
                        */
                        return System.Threading.Tasks.Task.CompletedTask;
                    });
            }

            // Register HttpClient with resilience policies
            services.AddHttpClient("default")
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy())
                .AddPolicyHandler(GetTimeoutPolicy())
                .AddPolicyHandler(GetBulkheadPolicy());

            // Register specific HttpClient for banking integration with resilience policies
            services.AddHttpClient("banking-integration")
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy())
                .AddPolicyHandler(GetTimeoutPolicy())
                .AddPolicyHandler(GetBulkheadPolicy());

            return services;
        }
    }

    /// <summary>
    /// Extension method to get the logger from the Polly context
    /// </summary>
    public static class PollyContextExtensions
    {
        public static Microsoft.Extensions.Logging.ILogger GetLogger(this Polly.Context context)
        {
            if (context.TryGetValue("logger", out var logger))
            {
                return logger as Microsoft.Extensions.Logging.ILogger;
            }

            return null;
        }
    }
}
