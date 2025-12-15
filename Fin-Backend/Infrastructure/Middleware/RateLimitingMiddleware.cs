using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using Microsoft.AspNetCore.Builder;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace FinTech.Infrastructure.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly IDistributedCache _cache;
        private readonly RateLimitingOptions _options;

        public RateLimitingMiddleware(
            RequestDelegate next,
            ILogger<RateLimitingMiddleware> logger,
            IDistributedCache cache,
            IOptions<RateLimitingOptions> options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!_options.IsEnabled)
            {
                await _next(context);
                return;
            }

            // Skip rate limiting for certain paths
            if (ShouldSkip(context.Request.Path))
            {
                await _next(context);
                return;
            }

            // Get client identifier (IP address or API key)
            string clientId = GetClientIdentifier(context);
            if (string.IsNullOrEmpty(clientId))
            {
                _logger.LogWarning("Could not identify client for rate limiting");
                await _next(context);
                return;
            }

            // Check if client is in the whitelist
            if (IsClientWhitelisted(clientId))
            {
                await _next(context);
                return;
            }

            // Get endpoint-specific limits or default limits
            (int limit, TimeSpan period) = GetLimits(context.Request.Path, context.Request.Method);

            // Check if client has exceeded the rate limit
            string cacheKey = $"rate_limit:{clientId}:{context.Request.Path}:{context.Request.Method}";
            RateLimitCounter counter = await GetCounterAsync(cacheKey, period);

            if (counter.Count >= limit)
            {
                _logger.LogWarning("Rate limit exceeded for client {ClientId} on {Path} {Method}",
                    clientId, context.Request.Path, context.Request.Method);

                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.Headers.Add("Retry-After", period.TotalSeconds.ToString());

                var response = new
                {
                    error = "Too many requests",
                    retryAfter = period.TotalSeconds,
                    limit = limit,
                    remaining = 0,
                    reset = counter.ResetAt.ToUniversalTime().ToString("o")
                };

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                return;
            }

            // Increment counter
            counter.Count++;
            await SaveCounterAsync(cacheKey, counter, period);

            // Add rate limit headers
            context.Response.Headers.Add("X-RateLimit-Limit", limit.ToString());
            context.Response.Headers.Add("X-RateLimit-Remaining", (limit - counter.Count).ToString());
            context.Response.Headers.Add("X-RateLimit-Reset", counter.ResetAt.ToUniversalTime().ToString("o"));

            await _next(context);
        }

        private bool ShouldSkip(string path)
        {
            foreach (var excludedPath in _options.ExcludedPaths)
            {
                if (path.StartsWith(excludedPath, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private string GetClientIdentifier(HttpContext context)
        {
            // Try to get API key from header
            if (context.Request.Headers.TryGetValue("X-API-Key", out var apiKey) && !string.IsNullOrEmpty(apiKey))
            {
                return $"apikey:{apiKey}";
            }

            // Try to get user ID if authenticated
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirst("sub")?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    return $"user:{userId}";
                }
            }

            // Fall back to IP address
            string ipAddress = context.Connection.RemoteIpAddress?.ToString();
            if (!string.IsNullOrEmpty(ipAddress))
            {
                return $"ip:{ipAddress}";
            }

            return null;
        }

        private bool IsClientWhitelisted(string clientId)
        {
            foreach (var whitelistedClient in _options.WhitelistedClients)
            {
                if (clientId.Equals(whitelistedClient, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private (int limit, TimeSpan period) GetLimits(string path, string method)
        {
            // Check for path-specific limits
            foreach (var endpointLimit in _options.EndpointLimits)
            {
                if (path.StartsWith(endpointLimit.Path, StringComparison.OrdinalIgnoreCase) &&
                    (string.IsNullOrEmpty(endpointLimit.Method) || method.Equals(endpointLimit.Method, StringComparison.OrdinalIgnoreCase)))
                {
                    return (endpointLimit.Limit, TimeSpan.FromSeconds(endpointLimit.PeriodInSeconds));
                }
            }

            // Fall back to default limits
            return (_options.DefaultLimit, TimeSpan.FromSeconds(_options.DefaultPeriodInSeconds));
        }

        private async Task<RateLimitCounter> GetCounterAsync(string cacheKey, TimeSpan period)
        {
            var counterBytes = await _cache.GetAsync(cacheKey);
            if (counterBytes != null)
            {
                try
                {
                    return JsonSerializer.Deserialize<RateLimitCounter>(counterBytes);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deserializing rate limit counter");
                }
            }

            return new RateLimitCounter
            {
                Count = 0,
                ResetAt = DateTime.UtcNow.Add(period)
            };
        }

        private async Task SaveCounterAsync(string cacheKey, RateLimitCounter counter, TimeSpan period)
        {
            var counterBytes = JsonSerializer.SerializeToUtf8Bytes(counter);
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = period
            };

            await _cache.SetAsync(cacheKey, counterBytes, cacheOptions);
        }

        private class RateLimitCounter
        {
            public int Count { get; set; }
            public DateTime ResetAt { get; set; }
        }
    }

    public class RateLimitingOptions
    {
        public bool IsEnabled { get; set; } = true;
        public int DefaultLimit { get; set; } = 100;
        public int DefaultPeriodInSeconds { get; set; } = 60;
        public string[] ExcludedPaths { get; set; } = new string[] { "/health", "/metrics" };
        public string[] WhitelistedClients { get; set; } = new string[] { };
        public EndpointLimit[] EndpointLimits { get; set; } = new EndpointLimit[] { };
    }

    public class EndpointLimit
    {
        public string Path { get; set; }
        public string Method { get; set; }
        public int Limit { get; set; }
        public int PeriodInSeconds { get; set; }
    }

    public static class RateLimitingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitingMiddleware>();
        }
    }
}
