using FinTech.Core.Application.Interfaces.Loans;
using FinTech.Core.Application.Interfaces.Repositories.Loans;
// Removed ambiguous/missing namespace
using FinTech.Core.Application.Services.Loans;
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
            services.AddScoped<ILoanProvisioningService, LoanProvisioningService>();
            
            // Repository registrations are provided by the Infrastructure project
            
            return services;
        }
    }
}
