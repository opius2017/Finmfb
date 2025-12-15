using FinTech.Core.Application.Interfaces.Loans;
using ILoanRepaymentService = FinTech.Core.Application.Interfaces.Loans.ILoanRepaymentService;
using FinTech.Core.Application.Interfaces.Repositories.Loans;
using FinTech.Core.Application.Services.Loans;
// Removed: using FinTech.Infrastructure.Repositories.Loans; - Violates Clean Architecture
using Microsoft.Extensions.DependencyInjection;

namespace FinTech.Core.Application
{
    public static class LoanServiceRegistration
    {
        public static IServiceCollection AddLoanServices(this IServiceCollection services)
        {
            // Register services
            services.AddScoped<ILoanApplicationService, LoanApplicationService>();
            services.AddScoped<ILoanProductService, LoanProductService>();
            services.AddScoped<ILoanService, LoanService>();
            services.AddScoped<ILoanDocumentService, LoanDocumentService>();
            services.AddScoped<ILoanCollectionService, LoanCollectionService>();
            services.AddScoped<ILoanCollateralService, LoanCollateralService>();
            services.AddScoped<ILoanRepaymentService, LoanRepaymentService>();
            services.AddScoped<ILoanProvisioningService, LoanProvisioningService>();
            services.AddScoped<IGuarantorService, GuarantorService>();
            services.AddScoped<ILoanCalculatorService, LoanCalculatorService>();
            services.AddScoped<ILoanRegisterService, LoanRegisterService>();
            services.AddScoped<ILoanEligibilityService, LoanEligibilityService>();
            services.AddScoped<ILoanCommitteeService, LoanCommitteeService>();
            services.AddScoped<IMonthlyThresholdService, MonthlyThresholdService>();
            
            // NOTE: Repository registrations moved to Infrastructure layer to maintain Clean Architecture
            // services.AddScoped<ILoanApplicationRepository, LoanApplicationRepository>();
            // services.AddScoped<ILoanProductRepository, LoanProductRepository>();
            // etc.
            
            return services;
        }
    }
}
