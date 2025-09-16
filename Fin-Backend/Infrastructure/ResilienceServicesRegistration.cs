using Fin_Backend.Infrastructure.Resilience;
using Microsoft.Extensions.DependencyInjection;

namespace Fin_Backend.Infrastructure
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