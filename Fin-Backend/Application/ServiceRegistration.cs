using Microsoft.Extensions.DependencyInjection;

namespace FinTech.Core.Application
{
    public static class ServiceRegistration
    {
        public static void AddApplicationServices(this IServiceCollection services)
        {
            // Register application services with full qualification to avoid ambiguity
            // Note: Many of these services are temporarily disabled due to interface/implementation mismatches
            // services.AddScoped<FinTech.Core.Application.Interfaces.Services.IFixedAssetService, FinTech.Core.Application.Services.FixedAssetService>();
            // services.AddScoped<FinTech.Core.Application.Interfaces.Services.IGeneralLedgerService, FinTech.Core.Application.Services.GeneralLedgerService>();
            // services.AddScoped<FinTech.Core.Application.Interfaces.Services.IInterestCalculationService, FinTech.Core.Application.Services.InterestCalculationService>();
            // services.AddScoped<FinTech.Core.Application.Interfaces.Services.ILoanService, FinTech.Core.Application.Services.LoanService>();
            // services.AddScoped<FinTech.Core.Application.Interfaces.Services.IMakerCheckerService, FinTech.Core.Application.Services.MakerCheckerService>();
            // services.AddScoped<FinTech.Core.Application.Interfaces.Services.IRegulatoryReportingService, FinTech.Core.Application.Services.RegulatoryReportingService>();
            
            // Repository registrations are provided by the Infrastructure project
            
            // Configure AutoMapper
            services.AddAutoMapper(typeof(ServiceRegistration).Assembly);
        }
    }
}
