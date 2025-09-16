using FinTech.WebAPI.Application.Interfaces;
using FinTech.WebAPI.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FinTech.Core.Application.Services.Integrations;
using FinTech.Core.Application.Common.Settings;
using FinTech.Infrastructure.Extensions;
using FinTech.Application;
using FinTech.Application.Interfaces.Services;
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
            services.AddScoped<FinTech.Core.Application.Services.Integrations.IEmailService, EmailService>();
            services.AddScoped<FinTech.WebAPI.Application.Interfaces.IEmailService, EmailServiceAdapter>();
            
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