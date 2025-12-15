using FinTech.Core.Application.Common.Interfaces;
using FinTech.Core.Application.Services;
using FinTech.Core.Application.Interfaces.Services;
using FinTech.Core.Application.Services.ClientPortal;
using FinTech.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FinTech.Infrastructure
{
    public static class ClientSupportServicesRegistration
    {
        public static IServiceCollection AddClientSupportServices(this IServiceCollection services)
        {
            // Register client support services
            services.AddScoped<IClientSupportService, ClientSupportService>();
            services.AddScoped<IAzureBlobStorageService, AzureBlobStorageServiceV2>();
            
            return services;
        }
    }
}
