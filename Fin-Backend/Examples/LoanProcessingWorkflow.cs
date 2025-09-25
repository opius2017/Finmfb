using System;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Infrastructure.Data;
using FinTech.Core.Application.Services.Integration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinTech.Examples
{
    public class LoanProcessingWorkflow
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILoanAccountingIntegrationService _loanAccountingService;
        private readonly ILogger<LoanProcessingWorkflow> _logger;

        public LoanProcessingWorkflow(
            ApplicationDbContext dbContext,
            ILoanAccountingIntegrationService loanAccountingService,
            ILogger<LoanProcessingWorkflow> logger)
        {
            _dbContext = dbContext;
            _loanAccountingService = loanAccountingService;
            _logger = logger;
        }

        public async Task RunLoanLifecycleWorkflowAsync()
        {
            _logger.LogInformation("Starting Loan Lifecycle Workflow Example");
            
            try
            {
                // 1. Create a new loan
                var loan = new Loan(
                    loanNumber: "LN20230001",
                    customerId: 1,
                    principalAmount: 500000,
                    interestRate: 15,
                    loanTermMonths: 24,
                    startDate: DateTime.UtcNow,
                    loanType: "PERSONAL",
                    repaymentFrequency: "MONTHLY",
                    currency: "NGN");

                _logger.LogInformation("Created personal loan for customer ID 1 with principal amount of 500,000 NGN");
                
                // 2. Add loan to context
                _dbContext.Add(loan);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Saved new loan with ID: {LoanId}", loan.Id);

                // 3. Charge processing fee
                loan.ChargeFee(5000, "PROCESSING", "Loan processing fee", "FEE001");
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Charged processing fee of 5,000 NGN for loan {LoanId}", loan.Id);

                // 4. Disburse the loan
                loan.Disburse("Initial disbursement", "DISB001");
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Disbursed loan {LoanId} for 500,000 NGN", loan.Id);

                // NOTE: At this point, the LoanDisbursedEvent has been raised by the Loan entity
                // and automatically processed by the DomainEventService during SaveChangesAsync
                // The LoanAccountingIntegrationService has created journal entries for the disbursement

                // 5. Accrue interest (simulating month-end process)
                loan.AccrueInterest(6250, "Monthly interest accrual", "INT001");
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Accrued interest of 6,250 NGN for loan {LoanId}", loan.Id);

                // 6. Record first repayment
                loan.RecordRepayment(18750, 6250, "First monthly payment", "REP001");
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Recorded repayment of 25,000 NGN (18,750 principal + 6,250 interest) for loan {LoanId}", loan.Id);

                // 7. Accrue interest for second month
                loan.AccrueInterest(6016, "Monthly interest accrual", "INT002");
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Accrued interest of 6,016 NGN for loan {LoanId}", loan.Id);

                // 8. Record second repayment
                loan.RecordRepayment(18984, 6016, "Second monthly payment", "REP002");
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Recorded repayment of 25,000 NGN (18,984 principal + 6,016 interest) for loan {LoanId}", loan.Id);

                // 9. Let's simulate a loan write-off scenario with a different loan
                var badLoan = new Loan(
                    loanNumber: "LN20230002",
                    customerId: 2,
                    principalAmount: 100000,
                    interestRate: 15,
                    loanTermMonths: 12,
                    startDate: DateTime.UtcNow.AddMonths(-3),
                    loanType: "PERSONAL",
                    repaymentFrequency: "MONTHLY",
                    currency: "NGN");

                _dbContext.Add(badLoan);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Created bad loan with ID: {LoanId}", badLoan.Id);

                badLoan.Disburse("Initial disbursement", "DISB002");
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Disbursed bad loan {LoanId} for 100,000 NGN", badLoan.Id);

                badLoan.AccrueInterest(3750, "Monthly interest accrual", "INT003");
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Accrued interest of 3,750 NGN for bad loan {LoanId}", badLoan.Id);

                // 10. Write off the loan after default
                badLoan.WriteOff("Non-performing loan write-off", "WO001");
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Wrote off bad loan {LoanId} with outstanding amount of 103,750 NGN", badLoan.Id);

                // 11. Retrieve loans with transactions to verify final state
                var updatedLoan = await _dbContext.Set<Loan>()
                    .Include(l => l.Transactions)
                    .FirstOrDefaultAsync(l => l.Id == loan.Id);

                _logger.LogInformation("Final loan outstanding principal: {Principal} NGN", updatedLoan.OutstandingPrincipal);
                _logger.LogInformation("Final loan outstanding interest: {Interest} NGN", updatedLoan.OutstandingInterest);
                _logger.LogInformation("Number of loan transactions: {Count}", updatedLoan.Transactions.Count);

                // For verification, you can query the accounting journal entries created by the integration service
                // var journalEntries = await _dbContext.JournalEntries
                //     .Where(je => je.Reference.StartsWith("LN"))
                //     .ToListAsync();
                // _logger.LogInformation("Number of loan-related journal entries: {Count}", journalEntries.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Loan Lifecycle Workflow Example");
                throw;
            }
        }
    }

    public static class LoanProcessingWorkflowExtensions
    {
        public static async Task RunLoanWorkflowExample(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var workflow = scope.ServiceProvider.GetRequiredService<LoanProcessingWorkflow>();
            await workflow.RunLoanLifecycleWorkflowAsync();
        }

        public static IServiceCollection AddLoanWorkflowExample(this IServiceCollection services)
        {
            services.AddTransient<LoanProcessingWorkflow>();
            return services;
        }
    }
}
