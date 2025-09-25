using FinTech.Core.Application.Interfaces.Repositories;
using FinTech.Core.Application.Interfaces.Services;
using FinTech.Core.Application.Services;
using FinTech.Infrastructure.Repositories;
using FinTech.Core.Application.Interfaces.Repositories;
using FinTech.Core.Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FinTech.Core.Application
{
    public static class ServiceRegistration
    {
        public static void AddApplicationServices(this IServiceCollection services)
        {
            // Register application services
            services.AddScoped<IFixedAssetService, FixedAssetService>();
            services.AddScoped<IGeneralLedgerService, GeneralLedgerService>();
            services.AddScoped<IInterestCalculationService, InterestCalculationService>();
            services.AddScoped<ILoanService, LoanService>();
            services.AddScoped<IMakerCheckerService, MakerCheckerService>();
            services.AddScoped<IRegulatoryReportingService, RegulatoryReportingService>();
            
            // Register repositories
            services.AddScoped<IFixedAssetRepository, FixedAssetRepository>();
            services.AddScoped<IRegulatoryReportingRepository, RegulatoryReportingRepository>();
            services.AddScoped<ICurrencyRepository, CurrencyRepository>();
            services.AddScoped<ITaxRepository, TaxRepository>();
            
            // Configure AutoMapper
            services.AddAutoMapper(typeof(ServiceRegistration).Assembly);
        }
    }
}
