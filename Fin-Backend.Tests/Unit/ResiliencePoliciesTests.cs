using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;
using Polly.Registry;
using Polly.Retry;
using Polly.Timeout;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace FinTech.Tests.Unit
{
    public class ResiliencePoliciesTests : IDisposable
    {
        private readonly WireMockServer _mockServer;
        private readonly HttpClient _httpClient;
        private readonly IServiceProvider _serviceProvider;

        public ResiliencePoliciesTests()
        {
            // Start WireMock server to simulate external API
            _mockServer = WireMockServer.Start();

            // Setup services with Polly policies
            var services = new ServiceCollection();

            // Configure Polly registry with resilience policies
            services.AddSingleton<IPolicyRegistry<string>, PolicyRegistry>();
            services.AddSingleton<IReadOnlyPolicyRegistry<string>>(provider => provider.GetRequiredService<IPolicyRegistry<string>>());

            var registry = new PolicyRegistry();

            // Retry policy
            var retryPolicy = Policy
                .Handle<HttpRequestException>()
                .Or<TimeoutRejectedException>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        Console.WriteLine($"Retry {retryCount} after {timeSpan.TotalSeconds}s due to: {exception.Message}");
                    });

            registry.Add("RetryPolicy", retryPolicy);

            // Circuit breaker policy
            var circuitBreakerPolicy = Policy
                .Handle<HttpRequestException>()
                .Or<TimeoutRejectedException>()
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: 2,
                    durationOfBreak: TimeSpan.FromSeconds(5),
                    onBreak: (exception, timespan) =>
                    {
                        Console.WriteLine($"Circuit broken for {timespan.TotalSeconds}s due to: {exception.Message}");
                    },
                    onReset: () =>
                    {
                        Console.WriteLine("Circuit reset");
                    },
                    onHalfOpen: () =>
                    {
                        Console.WriteLine("Circuit half-open");
                    });

            registry.Add("CircuitBreakerPolicy", circuitBreakerPolicy);

            // Timeout policy
            var timeoutPolicy = Policy
                .TimeoutAsync(5, TimeoutStrategy.Pessimistic, onTimeoutAsync: (context, timespan, task) =>
                {
                    Console.WriteLine($"Timeout after {timespan.TotalSeconds}s");
                    return Task.CompletedTask;
                });

            registry.Add("TimeoutPolicy", timeoutPolicy);

            // Combine policies
            var combinedPolicy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, timeoutPolicy);
            registry.Add("CombinedPolicy", combinedPolicy);

            services.AddSingleton(registry);

            // Create HTTP client
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_mockServer.Urls[0])
            };

            services.AddSingleton(_httpClient);

            _serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public async Task RetryPolicy_TemporaryFailure_EventuallySucceeds()
        {
            // Arrange
            const string endpoint = "/api/temporary-failure";
            var callCount = 0;

            // Setup WireMock to fail twice then succeed
            _mockServer
                .Given(Request.Create().WithPath(endpoint).UsingGet())
                .RespondWith(request =>
                {
                    callCount++;
                    if (callCount < 3)
                    {
                        return Response.Create()
                            .WithStatusCode(500)
                            .WithBody("Server Error");
                    }
                    else
                    {
                        return Response.Create()
                            .WithStatusCode(200)
                            .WithBody("Success");
                    }
                });

            // Get the retry policy
            var registry = _serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            var retryPolicy = registry.Get<IAsyncPolicy>("RetryPolicy");

            // Act
            var result = await retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _httpClient.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            });

            // Assert
            result.Should().Be("Success");
            callCount.Should().Be(3);
        }

        [Fact]
        public async Task CircuitBreakerPolicy_TooManyFailures_BreaksCircuit()
        {
            // Arrange
            const string endpoint = "/api/circuit-breaker";

            // Setup WireMock to always fail
            _mockServer
                .Given(Request.Create().WithPath(endpoint).UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(500)
                    .WithBody("Server Error"));

            // Get the circuit breaker policy
            var registry = _serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            var circuitBreakerPolicy = registry.Get<IAsyncPolicy>("CircuitBreakerPolicy");

            // Act & Assert
            // First call - should just fail
            await Assert.ThrowsAsync<HttpRequestException>(async () =>
            {
                await circuitBreakerPolicy.ExecuteAsync(async () =>
                {
                    var response = await _httpClient.GetAsync(endpoint);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                });
            });

            // Second call - should fail and trip the circuit
            await Assert.ThrowsAsync<HttpRequestException>(async () =>
            {
                await circuitBreakerPolicy.ExecuteAsync(async () =>
                {
                    var response = await _httpClient.GetAsync(endpoint);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                });
            });

            // Third call - circuit should be open
            await Assert.ThrowsAsync<BrokenCircuitException>(async () =>
            {
                await circuitBreakerPolicy.ExecuteAsync(async () =>
                {
                    var response = await _httpClient.GetAsync(endpoint);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                });
            });
        }

        [Fact]
        public async Task TimeoutPolicy_SlowOperation_ThrowsTimeoutException()
        {
            // Arrange
            const string endpoint = "/api/slow-operation";

            // Setup WireMock to respond with a delay
            _mockServer
                .Given(Request.Create().WithPath(endpoint).UsingGet())
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBody("This took too long")
                    .WithDelay(TimeSpan.FromSeconds(10))); // Delay longer than our timeout

            // Get the timeout policy
            var registry = _serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            var timeoutPolicy = registry.Get<IAsyncPolicy>("TimeoutPolicy");

            // Act & Assert
            await Assert.ThrowsAsync<TimeoutRejectedException>(async () =>
            {
                await timeoutPolicy.ExecuteAsync(async () =>
                {
                    var response = await _httpClient.GetAsync(endpoint);
                    return await response.Content.ReadAsStringAsync();
                });
            });
        }

        [Fact]
        public async Task CombinedPolicy_ComplexScenario_HandlesAllCases()
        {
            // Arrange
            const string endpoint = "/api/complex-scenario";
            var callCount = 0;

            // Setup WireMock to simulate a complex scenario
            _mockServer
                .Given(Request.Create().WithPath(endpoint).UsingGet())
                .RespondWith(request =>
                {
                    callCount++;
                    
                    // First call - timeout (slow response)
                    if (callCount == 1)
                    {
                        return Response.Create()
                            .WithStatusCode(200)
                            .WithDelay(TimeSpan.FromSeconds(10)); // Triggers timeout
                    }
                    // Second call (retry) - server error
                    else if (callCount == 2)
                    {
                        return Response.Create()
                            .WithStatusCode(500)
                            .WithBody("Server Error");
                    }
                    // Third call (retry) - success
                    else
                    {
                        return Response.Create()
                            .WithStatusCode(200)
                            .WithBody("Final Success");
                    }
                });

            // Get the combined policy
            var registry = _serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            var combinedPolicy = registry.Get<IAsyncPolicy>("CombinedPolicy");

            // Act
            var result = await combinedPolicy.ExecuteAsync(async () =>
            {
                var response = await _httpClient.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            });

            // Assert
            result.Should().Be("Final Success");
            callCount.Should().Be(3);
        }

        public void Dispose()
        {
            _mockServer?.Dispose();
            _httpClient?.Dispose();
        }
    }
}