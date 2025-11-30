using FinTech.Core.Application.Common.Interfaces;
using FinTech.Core.Application.Services;
using FinTech.Core.Domain.Services;
using FinTech.Infrastructure.BackgroundServices;
using FinTech.Infrastructure.Data;
using FinTech.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinTech.Infrastructure
{
    public static class ServiceRegistration
    {
        public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            // Register ApplicationDbContext as IApplicationDbContext
            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
            
            // Register Cooperative Loan Management Services
            services.AddScoped<ILoanCalculator, LoanCalculatorService>();
            services.AddScoped<ILoanEligibilityChecker, LoanEligibilityCheckerService>();
            services.AddScoped<IGuarantorService, GuarantorService>();
            services.AddScoped<ICommitteeReviewService, CommitteeReviewService>();
            services.AddScoped<ILoanRegisterService, LoanRegisterService>();
            services.AddScoped<IThresholdManager, ThresholdManagerService>();
            
            // Register Background Services
            services.AddHostedService<MonthlyRolloverJob>();
        }
    }
}
