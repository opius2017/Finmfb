using FinTech.Application.Common.Interfaces;
using FinTech.Application.Services;
using FinTech.Infrastructure.Services;
using FinTech.WebAPI.Application.Interfaces;
using FinTech.WebAPI.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinTech.Infrastructure
{
    public static class NotificationServicesRegistration
    {
        public static IServiceCollection AddNotificationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register notification services
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ISmsService, SmsService>();
            services.AddScoped<IPushNotificationService, PushNotificationService>();
            
            // Register MFA notification service
            services.AddScoped<IMfaNotificationService, MfaNotificationService>();
            
            return services;
        }
    }
}