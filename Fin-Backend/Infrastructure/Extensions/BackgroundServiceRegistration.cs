using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using FinTech.Infrastructure.Services.Background;
using FinTech.Infrastructure.Services;

namespace FinTech.Infrastructure.Extensions
{
    public static class BackgroundServiceRegistration
    {
        public static IServiceCollection AddBackgroundServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register background services for the application
            services.AddHostedService<NotificationProcessingService>();
            services.AddHostedService<RecurringPaymentProcessingService>();
            services.AddHostedService<DataCleanupService>();
            services.AddHostedService<SessionCleanupService>();
            
            return services;
        }
    }
}
