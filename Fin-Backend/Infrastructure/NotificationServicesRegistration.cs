using FinTech.Application.Common.Interfaces;
using FinTech.Application.Services;
using FinTech.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FinTech.Infrastructure
{
    public static class NotificationServicesRegistration
    {
        public static IServiceCollection AddNotificationServices(this IServiceCollection services)
        {
            // Register notification services
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ISmsService, SmsService>();
            services.AddScoped<IPushNotificationService, PushNotificationService>();
            
            return services;
        }
    }
}