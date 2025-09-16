using Microsoft.Extensions.DependencyInjection;
using FinTech.Application.Services;

namespace FinTech.WebAPI.Extensions
{
    public static class ClientPortalServiceExtensions
    {
        public static IServiceCollection AddClientPortalServices(this IServiceCollection services)
        {
            services.AddScoped<IClientPortalService, ClientPortalService>();
            
            return services;
        }
    }
}