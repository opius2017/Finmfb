using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FinTech.Infrastructure.Monitoring
{
    public class MemoryHealthCheck : IHealthCheck
    {
        private readonly long _thresholdBytes;

        public MemoryHealthCheck(long thresholdBytes = 1024 * 1024 * 1024) // 1 GB default threshold
        {
            _thresholdBytes = thresholdBytes;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var usedMemory = GC.GetTotalMemory(forceFullCollection: false);
            
            var memoryInfo = $"Current memory usage: {usedMemory / (1024 * 1024)} MB";
            
            if (usedMemory > _thresholdBytes)
            {
                return Task.FromResult(HealthCheckResult.Degraded(
                    description: $"Memory usage exceeds threshold. {memoryInfo}"));
            }
            
            return Task.FromResult(HealthCheckResult.Healthy(memoryInfo));
        }
    }
    
    public class ApiDependencyHealthCheck : IHealthCheck
    {
        private readonly HttpClient _httpClient;
        
        public ApiDependencyHealthCheck()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(5)
            };
        }
        
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Check all critical external dependencies
                var tasks = new[]
                {
                    CheckDependencyAsync("Payment Gateway", "https://api.payment-gateway.example.com/health"),
                    CheckDependencyAsync("Credit Bureau", "https://api.credit-bureau.example.com/health"),
                    CheckDependencyAsync("Regulatory API", "https://api.regulatory.example.com/health")
                };
                
                var results = await Task.WhenAll(tasks);
                
                bool allHealthy = true;
                string description = string.Empty;
                
                foreach (var result in results)
                {
                    if (!result.healthy)
                    {
                        allHealthy = false;
                        description += $"{result.name}: {result.message}; ";
                    }
                }
                
                if (allHealthy)
                {
                    return HealthCheckResult.Healthy("All external dependencies are healthy");
                }
                
                return HealthCheckResult.Degraded(description);
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"Exception during health check: {ex.Message}");
            }
        }
        
        private async Task<(string name, bool healthy, string message)> CheckDependencyAsync(string name, string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                
                if (response.IsSuccessStatusCode)
                {
                    return (name, true, "Healthy");
                }
                
                return (name, false, $"Returned status code {response.StatusCode}");
            }
            catch (Exception ex)
            {
                return (name, false, ex.Message);
            }
        }
    }
}
