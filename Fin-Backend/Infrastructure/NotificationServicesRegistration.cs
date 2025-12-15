using FinTech.Core.Application.Common.Interfaces;
using FinTech.Core.Application.Interfaces;
using FinTech.Infrastructure.Services;
using FinTech.Core.Application.Interfaces.Services; // For IMfaProviderNotificationService if needed? No, likely IMfaNotificationService which is in Interfaces.
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
            services.AddScoped<FinTech.Core.Application.Interfaces.IEmailService, FinTech.Infrastructure.Services.EmailService>();
            services.AddScoped<ISmsService, SmsService>();
            services.AddScoped<IPushNotificationService, PushNotificationService>();
            
            // Register MFA notification service
            services.AddScoped<IMfaNotificationService, MfaNotificationService>();
            
            return services;
        }
    }
}
