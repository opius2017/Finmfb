using Microsoft.Extensions.DependencyInjection;
using FinTech.Core.Application.Interfaces.Services;
using FinTech.Core.Application.Services;
using FinTech.Core.Application.Interfaces.Repositories;
using FinTech.Infrastructure.Repositories;

namespace FinTech.Core.Application
{
    public static class AccountingServiceRegistration
    {
        public static IServiceCollection AddAccountingServices(this IServiceCollection services)
        {
            // Register repositories
            services.AddScoped<IGeneralLedgerRepository, GeneralLedgerRepository>();

            // Register services
            services.AddScoped<IGeneralLedgerService, GeneralLedgerService>();

            return services;
        }
    }
}