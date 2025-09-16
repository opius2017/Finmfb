using FinTech.Application.Services.Accounting;
using FinTech.Core.Application.Interfaces.Services;
using FinTech.Core.Application.Services;
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
            
            // Register new accounting engine services
            services.AddScoped<IGeneralLedgerService, GeneralLedgerService>();
            services.AddScoped<ITrialBalanceService, TrialBalanceService>();
            services.AddScoped<IFinancialStatementService, FinancialStatementService>();
            
            return services;
        }
    }
}