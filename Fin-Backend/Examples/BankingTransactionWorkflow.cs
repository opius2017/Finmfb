using System;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Banking;
using FinTech.Infrastructure.Data;
using FinTech.Core.Application.Services.Integration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinTech.Examples
{
    public class BankingTransactionWorkflow
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IBankingAccountingIntegrationService _bankingAccountingService;
        private readonly ILogger<BankingTransactionWorkflow> _logger;

        public BankingTransactionWorkflow(
            ApplicationDbContext dbContext,
            IBankingAccountingIntegrationService bankingAccountingService,
            ILogger<BankingTransactionWorkflow> logger)
        {
            _dbContext = dbContext;
            _bankingAccountingService = bankingAccountingService;
            _logger = logger;
        }

        public async Task RunDepositWorkflowAsync()
        {
            _logger.LogInformation("Starting Banking Deposit Workflow Example");
            
            try
            {
                // 1. Create a new bank account
                var account = new BankAccount(
                    accountNumber: "1234567890",
                    accountName: "John Doe",
                    accountType: "SAVINGS",
                    customerId: 1,
                    currency: "NGN",
                    initialDeposit: 1000);

                _logger.LogInformation("Created bank account for John Doe with initial deposit of 1,000 NGN");
                
                // 2. Add account to context
                _dbContext.Add(account);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Saved new bank account with ID: {AccountId}", account.Id);

                // NOTE: At this point, the DepositCompletedEvent has been raised by the BankAccount entity
                // and automatically processed by the DomainEventService during SaveChangesAsync
                // The BankingAccountingIntegrationService has created journal entries for the deposit

                // 3. Make another deposit
                account.Deposit(2000, "Customer deposit", "DEP001");
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Processed deposit of 2,000 NGN to account {AccountId}", account.Id);

                // 4. Make a withdrawal
                account.Withdraw(500, "ATM withdrawal", "WIT001");
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Processed withdrawal of 500 NGN from account {AccountId}", account.Id);

                // 5. Create another account for transfer demonstration
                var destinationAccount = new BankAccount(
                    accountNumber: "0987654321",
                    accountName: "Jane Smith",
                    accountType: "CURRENT",
                    customerId: 2,
                    currency: "NGN");

                _dbContext.Add(destinationAccount);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Created destination account for Jane Smith with ID: {AccountId}", destinationAccount.Id);

                // 6. Perform a transfer between accounts
                account.Transfer(destinationAccount, 300, "Transfer to Jane", "TRF001");
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Processed transfer of 300 NGN from account {SourceId} to account {DestinationId}",
                    account.Id, destinationAccount.Id);

                // 7. Charge a fee
                account.ChargeFee(25, "MAINTENANCE", "Monthly maintenance fee", "FEE001");
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Charged maintenance fee of 25 NGN to account {AccountId}", account.Id);

                // 8. Pay interest
                account.PayInterest(15.75m, "Monthly interest payment", "INT001");
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Paid interest of 15.75 NGN to account {AccountId}", account.Id);

                // 9. Retrieve account with transactions to verify final state
                var updatedAccount = await _dbContext.Set<BankAccount>()
                    .Include(a => a.Transactions)
                    .FirstOrDefaultAsync(a => a.Id == account.Id);

                _logger.LogInformation("Final account balance: {Balance} NGN", updatedAccount.Balance);
                _logger.LogInformation("Number of transactions: {Count}", updatedAccount.Transactions.Count);

                // For verification, you can query the accounting journal entries created by the integration service
                // var journalEntries = await _dbContext.JournalEntries
                //     .Where(je => je.Reference.StartsWith("BNK"))
                //     .ToListAsync();
                // _logger.LogInformation("Number of banking-related journal entries: {Count}", journalEntries.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Banking Deposit Workflow Example");
                throw;
            }
        }
    }

    public static class BankingTransactionWorkflowExtensions
    {
        public static async Task RunBankingWorkflowExample(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var workflow = scope.ServiceProvider.GetRequiredService<BankingTransactionWorkflow>();
            await workflow.RunDepositWorkflowAsync();
        }

        public static IServiceCollection AddBankingWorkflowExample(this IServiceCollection services)
        {
            services.AddTransient<BankingTransactionWorkflow>();
            return services;
        }
    }
}
