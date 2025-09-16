using FinTech.Application.Services.Accounting;
using Microsoft.Extensions.DependencyInjection;

namespace FinTech.Application
{
    public static class AccountingServiceRegistration
    {
        public static IServiceCollection AddAccountingServices(this IServiceCollection services)
        {
            // Register accounting services
            services.AddScoped<IChartOfAccountService, ChartOfAccountService>();
            services.AddScoped<IJournalEntryService, JournalEntryService>();
            services.AddScoped<IFiscalYearService, FiscalYearService>();
            services.AddScoped<IFinancialPeriodService, FinancialPeriodService>();
            
            return services;
        }
    }
}