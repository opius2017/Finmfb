using Microsoft.Extensions.DependencyInjection;

namespace FinTech.Core.Application
{
    public static class AccountingServiceRegistration
    {
        public static IServiceCollection AddAccountingServices(this IServiceCollection services)
        {
            // Register accounting services with full qualification to avoid ambiguity
            // Note: Many of these services are temporarily disabled due to interface/implementation mismatches
            // services.AddScoped<FinTech.Core.Application.Interfaces.Services.IChartOfAccountService, FinTech.Core.Application.Services.Accounting.ChartOfAccountService>();
            // services.AddScoped<FinTech.Core.Application.Interfaces.Services.IJournalEntryService, FinTech.Core.Application.Services.Accounting.JournalEntryService>();
            // services.AddScoped<FinTech.Core.Application.Interfaces.Services.IFiscalYearService, FinTech.Core.Application.Services.Accounting.FiscalYearService>();
            // services.AddScoped<FinTech.Core.Application.Interfaces.Services.IFinancialPeriodService, FinTech.Core.Application.Services.Accounting.FinancialPeriodService>();
            
            // Register new accounting engine services
            // services.AddScoped<FinTech.Core.Application.Interfaces.Services.IGeneralLedgerService, FinTech.Core.Application.Services.Accounting.GeneralLedgerService>();
            // services.AddScoped<FinTech.Core.Application.Interfaces.Services.ITrialBalanceService, FinTech.Core.Application.Services.Accounting.TrialBalanceService>();
            // services.AddScoped<FinTech.Core.Application.Interfaces.Services.IFinancialStatementService, FinTech.Core.Application.Services.Accounting.FinancialStatementService>();
            // services.AddScoped<FinTech.Core.Application.Interfaces.Services.IRegulatoryMappingService, FinTech.Core.Application.Services.Accounting.RegulatoryMappingService>();
            // services.AddScoped<FinTech.Core.Application.Interfaces.Services.IPeriodClosingService, FinTech.Core.Application.Services.Accounting.PeriodClosingService>();
            // services.AddScoped<FinTech.Core.Application.Interfaces.Services.IFinancialAnalyticsService, FinTech.Core.Application.Services.Accounting.FinancialAnalyticsService>();
            // services.AddScoped<FinTech.Core.Application.Interfaces.Services.IBudgetingService, FinTech.Core.Application.Services.Accounting.BudgetingService>();
            
            // Register enhanced fixed asset management service
            // services.AddScoped<FinTech.Core.Application.Interfaces.Services.IFixedAssetService, FinTech.Core.Application.Services.Accounting.FixedAssetService>();
            
            // Register multi-currency support service
            // services.AddScoped<FinTech.Core.Application.Interfaces.Services.ICurrencyService, FinTech.Core.Application.Services.Accounting.CurrencyService>();
            
            // Register tax calculation service
            // services.AddScoped<FinTech.Core.Application.Interfaces.Services.ITaxCalculationService, FinTech.Core.Application.Services.Accounting.TaxCalculationService>();
            
            return services;
        }
    }
}
