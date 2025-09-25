using FinTech.Core.Application.Common.Interfaces;
using FinTech.Core.Application.Services;
using FinTech.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FinTech.Infrastructure
{
    public static class ClientPortalServicesRegistration
    {
        public static IServiceCollection AddClientPortalServices(this IServiceCollection services)
        {
            // Register all client portal services
            services.AddScoped<IClientLoanService, ClientLoanService>();
            services.AddScoped<IClientPaymentService, ClientPaymentService>();
            services.AddScoped<IClientProfileService, ClientProfileService>();
            services.AddScoped<IClientSupportService, ClientSupportService>();
            services.AddScoped<INotificationService, NotificationService>();
            
            // Register supporting services
            services.AddScoped<IAzureBlobStorageService, AzureBlobStorageServiceV2>();
            services.AddScoped<IFileStorageService, AzureBlobStorageService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ISmsService, SmsService>();
            services.AddScoped<IPushNotificationService, PushNotificationService>();
            
            return services;
        }
    }
}
