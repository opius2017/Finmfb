using FinTech.Core.Application.Services.Accounting;
using FinTech.Core.Application.Interfaces.Services.Accounting;
using Microsoft.Extensions.DependencyInjection;

namespace FinTech.Core.Application
{
    public static class AccountingServiceRegistration
    {
        public static IServiceCollection AddAccountingServices(this IServiceCollection services)
        {
            // Register accounting services
            services.AddScoped<IChartOfAccountService, ChartOfAccountService>();
            services.AddScoped<IJournalEntryService, JournalEntryService>();
            // Note: IGeneralLedgerService is registered in ServiceRegistration.cs with the main implementation
            
            return services;
        }
    }
}