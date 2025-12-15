using Microsoft.Extensions.DependencyInjection;

namespace FinTech.Infrastructure
{
    public static class WorkflowExamplesRegistration
    {
        public static IServiceCollection AddWorkflowExamples(this IServiceCollection services)
        {
            // services.AddBankingWorkflowExample();
            // services.AddLoanWorkflowExample();
            // services.AddPayrollWorkflowExample();
            // services.AddFixedAssetWorkflowExample();
            
            return services;
        }
    }
}
