using FinTech.Application.Services.Integration;
using Microsoft.Extensions.DependencyInjection;

namespace FinTech.Application
{
    public static class IntegrationServiceRegistration
    {
        public static IServiceCollection AddIntegrationServices(this IServiceCollection services)
        {
            // Register integration services
            services.AddScoped<IBankingAccountingIntegrationService, BankingAccountingIntegrationService>();
            services.AddScoped<ILoanAccountingIntegrationService, LoanAccountingIntegrationService>();
            services.AddScoped<IPayrollAccountingIntegrationService, PayrollAccountingIntegrationService>();
            services.AddScoped<IFixedAssetAccountingIntegrationService, FixedAssetAccountingIntegrationService>();
            
            return services;
        }
    }
}