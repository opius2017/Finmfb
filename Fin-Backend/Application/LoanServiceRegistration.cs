using FinTech.Application.Interfaces.Loans;
using FinTech.Application.Interfaces.Repositories.Loans;
using FinTech.Application.Interfaces.Services.Loans;
using FinTech.Application.Services.Loans;
using FinTech.Infrastructure.Repositories.Loans;
using Microsoft.Extensions.DependencyInjection;

namespace FinTech.Application
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
            
            // Register repositories
            services.AddScoped<ILoanApplicationRepository, LoanApplicationRepository>();
            services.AddScoped<ILoanProductRepository, LoanProductRepository>();
            services.AddScoped<ILoanRepository, LoanRepository>();
            services.AddScoped<ILoanTransactionRepository, LoanTransactionRepository>();
            services.AddScoped<ILoanRepaymentScheduleRepository, LoanRepaymentScheduleRepository>();
            services.AddScoped<ILoanDocumentRepository, LoanDocumentRepository>();
            services.AddScoped<ILoanCollateralRepository, LoanCollateralRepository>();
            services.AddScoped<ILoanGuarantorRepository, LoanGuarantorRepository>();
            services.AddScoped<ILoanCollectionRepository, LoanCollectionRepository>();
            services.AddScoped<ILoanFeeRepository, LoanFeeRepository>();
            services.AddScoped<ILoanCreditCheckRepository, LoanCreditCheckRepository>();
            
            return services;
        }
    }
}