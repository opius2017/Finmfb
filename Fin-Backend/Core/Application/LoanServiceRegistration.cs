using FinTech.Core.Application.Interfaces.Loans;
using FinTech.Core.Application.Interfaces.Repositories.Loans;
// Removed ambiguous/missing namespace
using FinTech.Core.Application.Services.Loans;
using FinTech.Infrastructure.Repositories.Loans;
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
            
            // Register repositories
            services.AddScoped<ILoanApplicationRepository, LoanApplicationRepository>();
            services.AddScoped<ILoanProductRepository, LoanProductRepository>();
            services.AddScoped<ILoanRepository, LoanRepository>();
            services.AddScoped<ILoanTransactionRepository, LoanTransactionRepository>();
            services.AddScoped<ILoanRepaymentScheduleRepository, LoanRepaymentScheduleRepository>();
            services.AddScoped<ILoanDocumentRepository, LoanDocumentRepository>();
            // Use repository from Interfaces.Repositories.Loans
            services.AddScoped<FinTech.Core.Application.Interfaces.Repositories.Loans.ILoanCollateralRepository, LoanCollateralRepository>();
            services.AddScoped<ILoanGuarantorRepository, LoanGuarantorRepository>();
            services.AddScoped<ILoanCollectionRepository, LoanCollectionRepository>();
            services.AddScoped<ILoanFeeRepository, FinTech.Infrastructure.Repositories.Loans.LoanFeeRepository>();
            services.AddScoped<ILoanCreditCheckRepository, FinTech.Infrastructure.Repositories.Loans.LoanCreditCheckRepository>();
            
            return services;
        }
    }
}
