using FinTech.Infrastructure.Resilience;
using Microsoft.Extensions.DependencyInjection;

namespace FinTech.Infrastructure
{
    public static class ResilienceServicesRegistration
    {
        public static IServiceCollection AddResilienceServices(this IServiceCollection services)
        {
            // Add Polly resilience policies for HTTP clients
            services.AddResiliencePolicies();

            return services;
        }
    }
}
