using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using FinTech.Core.Application.Services;
using FinTech.Core.Application.Interfaces.Services;
using FinTech.Core.Application.Services.ClientPortal;
using FinTech.Infrastructure.Services;
// using FinTech.Infrastructure.Services.Notifications; // Removed invalid namespace
using FinTech.Core.Application.Services;
using FinTech.Core.Application.Interfaces;

namespace FinTech.Infrastructure.Extensions
{
    public static class ClientPortalServicesRegistration
    {
        public static IServiceCollection AddClientPortalServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register client portal core services
            services.AddScoped<IClientDashboardService, ClientDashboardService>();
            services.AddScoped<IClientAccountService, ClientAccountService>();
            services.AddScoped<IClientTransactionService, ClientTransactionService>();
            services.AddScoped<IClientLoanService, ClientLoanService>();
            services.AddScoped<IClientPaymentService, ClientPaymentService>();
            services.AddScoped<IClientProfileService, ClientProfileService>();
            services.AddScoped<IClientDocumentService, ClientDocumentService>();
            services.AddScoped<IClientSupportService, ClientSupportService>();
            services.AddScoped<IClientSavingsGoalService, ClientSavingsGoalService>();
            
            // Register notification services
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IEmailNotificationService, EmailNotificationService>();
            services.AddScoped<ISmsNotificationService, SmsNotificationService>();
            services.AddScoped<IPushNotificationService, PushNotificationService>();
            
            // Register background services
            services.AddHostedService<NotificationProcessingService>();
            
            // Register client portal security services
            services.AddScoped<IClientAuthService, ClientAuthService>();
            services.AddScoped<IClientSessionService, ClientSessionService>();
            services.AddScoped<IClientSecurityService, ClientSecurityService>();
            
            // Register file storage services
            services.AddScoped<IFileStorageService, AzureBlobStorageService>();
            
            // Configure client portal settings
            services.Configure<ClientPortalSettings>(configuration.GetSection("ClientPortal"));
            services.Configure<NotificationSettings>(configuration.GetSection("Notifications"));
            
            return services;
        }
    }
    
    public class ClientPortalSettings
    {
        public string BaseUrl { get; set; }
        public int SessionTimeoutMinutes { get; set; } = 30;
        public bool RequireMfa { get; set; } = true;
        public int MaxFailedLoginAttempts { get; set; } = 5;
        public int LockoutDurationMinutes { get; set; } = 30;
        public bool EnableDeviceTracking { get; set; } = true;
        public bool EnableLocationTracking { get; set; } = true;
        public string[] AllowedFileTypes { get; set; } = { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx", ".xls", ".xlsx" };
        public long MaxFileSize { get; set; } = 10485760; // 10MB
    }
}
