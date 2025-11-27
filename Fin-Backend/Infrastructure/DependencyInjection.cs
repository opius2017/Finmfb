using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FinTech.Core.Application.Common.Settings;
using FinTech.Infrastructure.Extensions;
using FinTech.Core.Application;
using FinTech.Core.Application.Interfaces.Services;
using FinTech.Infrastructure.Services;
using Microsoft.AspNetCore.Http;

namespace FinTech.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register Email Service and Settings
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.AddScoped<IEmailService, EmailService>();
            
            // Register MFA Services
            services.AddScoped<IMfaNotificationService, MfaNotificationService>();
            services.AddScoped<IMfaService, MfaService>();
            
            // Register Client Portal Services
            services.AddClientPortalServices(configuration);
            
            // Add Background Services
            services.AddBackgroundServices(configuration);
            
            // Add Core Accounting Infrastructure Services
            services.AddAccountingInfrastructure();
            
            // Add Event and Integration Services
            services.AddEventServices();
            services.AddIntegrationServices();
            
            // Add user context services
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            
            // Add other infrastructure services here
            
            return services;
        }
    }
}
