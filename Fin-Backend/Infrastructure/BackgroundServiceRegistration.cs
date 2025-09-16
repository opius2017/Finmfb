using FinTech.Infrastructure.BackgroundServices;
using Microsoft.Extensions.DependencyInjection;

namespace FinTech.Infrastructure
{
    public static class BackgroundServiceRegistration
    {
        public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
        {
            services.AddHostedService<NotificationProcessingService>();
            
            return services;
        }
    }
}