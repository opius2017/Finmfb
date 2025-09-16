using FinTech.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace FinTech.Infrastructure.Security
{
    public static class SecurityServicesRegistration
    {
        public static IServiceCollection AddSecurityServices(this IServiceCollection services)
        {
            // Register authorization handler
            services.AddScoped<IAuthorizationHandler, ResourceAuthorizationHandler>();
            
            // Configure authorization policies
            services.AddAuthorization(options =>
            {
                // Banking policies
                ConfigureResourcePolicies(options, "BankAccount");
                ConfigureResourcePolicies(options, "Transaction");
                ConfigureResourcePolicies(options, "CustomerAccount");
                
                // Loan policies
                ConfigureResourcePolicies(options, "Loan");
                ConfigureResourcePolicies(options, "LoanApplication");
                ConfigureResourcePolicies(options, "LoanDisbursement");
                ConfigureResourcePolicies(options, "LoanRepayment");
                
                // Accounting policies
                ConfigureResourcePolicies(options, "JournalEntry");
                ConfigureResourcePolicies(options, "ChartOfAccount");
                ConfigureResourcePolicies(options, "GeneralLedger");
                
                // Fixed Asset policies
                ConfigureResourcePolicies(options, "FixedAsset");
                ConfigureResourcePolicies(options, "AssetCategory");
                ConfigureResourcePolicies(options, "AssetDepreciation");
                
                // Payroll policies
                ConfigureResourcePolicies(options, "Payroll");
                ConfigureResourcePolicies(options, "PayrollRun");
                ConfigureResourcePolicies(options, "PayrollItem");
                ConfigureResourcePolicies(options, "Employee");
                
                // System policies
                ConfigureResourcePolicies(options, "User");
                ConfigureResourcePolicies(options, "Role");
                ConfigureResourcePolicies(options, "SystemConfiguration");
                ConfigureResourcePolicies(options, "SecuritySettings");
            });
            
            return services;
        }
        
        private static void ConfigureResourcePolicies(AuthorizationOptions options, string resource)
        {
            foreach (ResourceOperation operation in Enum.GetValues(typeof(ResourceOperation)))
            {
                var policyName = $"{resource}_{operation}";
                options.AddPolicy(policyName, policy =>
                    policy.Requirements.Add(new ResourceAuthorizationRequirement(resource, operation)));
            }
        }
    }
}