using FinTech.Domain.Entities.Accounting;
using FinTech.Domain.Repositories.Accounting;
using FinTech.Infrastructure.Data;
using FinTech.Infrastructure.Repositories.Accounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FinTech.Infrastructure
{
    public static class AccountingInfrastructureRegistration
    {
        public static IServiceCollection AddAccountingInfrastructure(this IServiceCollection services)
        {
            // Register repositories
            services.AddScoped<IChartOfAccountRepository, ChartOfAccountRepository>();
            services.AddScoped<IJournalEntryRepository, JournalEntryRepository>();
            services.AddScoped<IFinancialPeriodRepository, FinancialPeriodRepository>();
            services.AddScoped<IFiscalYearRepository, FiscalYearRepository>();
            
            // Register Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            
            return services;
        }
    }
}