using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FinTech.Application.Interfaces.Services;
using FinTech.Application.Services.Integration;
using System.Collections.Generic;

namespace FinTech.Infrastructure.Services.Integration
{
    public class BankingAccountingIntegrationService : IBankingAccountingIntegrationService
    {
        private readonly ILogger<BankingAccountingIntegrationService> _logger;
        private readonly IJournalEntryService _journalEntryService;
        private readonly IChartOfAccountService _chartOfAccountService;

        public BankingAccountingIntegrationService(
            ILogger<BankingAccountingIntegrationService> logger,
            IJournalEntryService journalEntryService,
            IChartOfAccountService chartOfAccountService)
        {
            _logger = logger;
            _journalEntryService = journalEntryService;
            _chartOfAccountService = chartOfAccountService;
        }

        public async Task ProcessDepositAsync(int accountId, decimal amount, string reference, string description)
        {
            try
            {
                _logger.LogInformation("Processing deposit for account {AccountId} with amount {Amount}", accountId, amount);
                
                // Get the bank account and cash account from chart of accounts
                var bankAccountId = await GetBankAccountIdForCustomerAccountAsync(accountId);
                var cashAccountId = await _chartOfAccountService.GetCashAccountIdAsync();
                
                // Create journal entry lines
                var journalLines = new List<JournalEntryLineDto>
                {
                    new JournalEntryLineDto
                    {
                        AccountId = bankAccountId,
                        Description = $"Deposit to account {accountId}",
                        DebitAmount = amount,
                        CreditAmount = 0
                    },
                    new JournalEntryLineDto
                    {
                        AccountId = cashAccountId,
                        Description = $"Cash deposit for account {accountId}",
                        DebitAmount = 0,
                        CreditAmount = amount
                    }
                };
                
                // Create the journal entry
                await _journalEntryService.CreateJournalEntryAsync(
                    new JournalEntryDto
                    {
                        TransactionDate = DateTime.UtcNow,
                        Reference = reference,
                        Description = description,
                        Source = "Banking",
                        Lines = journalLines
                    });
                
                _logger.LogInformation("Successfully processed deposit for account {AccountId}", accountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing deposit for account {AccountId}", accountId);
                throw;
            }
        }

        public async Task ProcessWithdrawalAsync(int accountId, decimal amount, string reference, string description)
        {
            try
            {
                _logger.LogInformation("Processing withdrawal for account {AccountId} with amount {Amount}", accountId, amount);
                
                // Get the bank account and cash account from chart of accounts
                var bankAccountId = await GetBankAccountIdForCustomerAccountAsync(accountId);
                var cashAccountId = await _chartOfAccountService.GetCashAccountIdAsync();
                
                // Create journal entry lines
                var journalLines = new List<JournalEntryLineDto>
                {
                    new JournalEntryLineDto
                    {
                        AccountId = cashAccountId,
                        Description = $"Cash withdrawal for account {accountId}",
                        DebitAmount = amount,
                        CreditAmount = 0
                    },
                    new JournalEntryLineDto
                    {
                        AccountId = bankAccountId,
                        Description = $"Withdrawal from account {accountId}",
                        DebitAmount = 0,
                        CreditAmount = amount
                    }
                };
                
                // Create the journal entry
                await _journalEntryService.CreateJournalEntryAsync(
                    new JournalEntryDto
                    {
                        TransactionDate = DateTime.UtcNow,
                        Reference = reference,
                        Description = description,
                        Source = "Banking",
                        Lines = journalLines
                    });
                
                _logger.LogInformation("Successfully processed withdrawal for account {AccountId}", accountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing withdrawal for account {AccountId}", accountId);
                throw;
            }
        }

        public async Task ProcessTransferAsync(int fromAccountId, int toAccountId, decimal amount, string reference, string description)
        {
            try
            {
                _logger.LogInformation("Processing transfer from account {FromAccountId} to account {ToAccountId} with amount {Amount}", 
                    fromAccountId, toAccountId, amount);
                
                // Get the bank accounts from chart of accounts
                var fromBankAccountId = await GetBankAccountIdForCustomerAccountAsync(fromAccountId);
                var toBankAccountId = await GetBankAccountIdForCustomerAccountAsync(toAccountId);
                
                // Create journal entry lines
                var journalLines = new List<JournalEntryLineDto>
                {
                    new JournalEntryLineDto
                    {
                        AccountId = toBankAccountId,
                        Description = $"Transfer to account {toAccountId}",
                        DebitAmount = amount,
                        CreditAmount = 0
                    },
                    new JournalEntryLineDto
                    {
                        AccountId = fromBankAccountId,
                        Description = $"Transfer from account {fromAccountId}",
                        DebitAmount = 0,
                        CreditAmount = amount
                    }
                };
                
                // Create the journal entry
                await _journalEntryService.CreateJournalEntryAsync(
                    new JournalEntryDto
                    {
                        TransactionDate = DateTime.UtcNow,
                        Reference = reference,
                        Description = description,
                        Source = "Banking",
                        Lines = journalLines
                    });
                
                _logger.LogInformation("Successfully processed transfer between accounts");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing transfer between accounts");
                throw;
            }
        }

        public async Task ProcessFeeChargeAsync(int accountId, decimal amount, string feeType, string reference, string description)
        {
            try
            {
                _logger.LogInformation("Processing fee charge for account {AccountId} with amount {Amount}, fee type {FeeType}", 
                    accountId, amount, feeType);
                
                // Get the accounts from chart of accounts
                var bankAccountId = await GetBankAccountIdForCustomerAccountAsync(accountId);
                var feeIncomeAccountId = await _chartOfAccountService.GetFeeIncomeAccountIdAsync(feeType);
                
                // Create journal entry lines
                var journalLines = new List<JournalEntryLineDto>
                {
                    new JournalEntryLineDto
                    {
                        AccountId = feeIncomeAccountId,
                        Description = $"{feeType} fee income from account {accountId}",
                        DebitAmount = 0,
                        CreditAmount = amount
                    },
                    new JournalEntryLineDto
                    {
                        AccountId = bankAccountId,
                        Description = $"Fee charged to account {accountId}",
                        DebitAmount = amount,
                        CreditAmount = 0
                    }
                };
                
                // Create the journal entry
                await _journalEntryService.CreateJournalEntryAsync(
                    new JournalEntryDto
                    {
                        TransactionDate = DateTime.UtcNow,
                        Reference = reference,
                        Description = description,
                        Source = "Banking",
                        Lines = journalLines
                    });
                
                _logger.LogInformation("Successfully processed fee charge for account {AccountId}", accountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing fee charge for account {AccountId}", accountId);
                throw;
            }
        }

        public async Task ProcessInterestPaymentAsync(int accountId, decimal amount, string reference, string description)
        {
            try
            {
                _logger.LogInformation("Processing interest payment for account {AccountId} with amount {Amount}", 
                    accountId, amount);
                
                // Get the accounts from chart of accounts
                var bankAccountId = await GetBankAccountIdForCustomerAccountAsync(accountId);
                var interestExpenseAccountId = await _chartOfAccountService.GetInterestExpenseAccountIdAsync();
                
                // Create journal entry lines
                var journalLines = new List<JournalEntryLineDto>
                {
                    new JournalEntryLineDto
                    {
                        AccountId = interestExpenseAccountId,
                        Description = $"Interest expense for account {accountId}",
                        DebitAmount = amount,
                        CreditAmount = 0
                    },
                    new JournalEntryLineDto
                    {
                        AccountId = bankAccountId,
                        Description = $"Interest paid to account {accountId}",
                        DebitAmount = 0,
                        CreditAmount = amount
                    }
                };
                
                // Create the journal entry
                await _journalEntryService.CreateJournalEntryAsync(
                    new JournalEntryDto
                    {
                        TransactionDate = DateTime.UtcNow,
                        Reference = reference,
                        Description = description,
                        Source = "Banking",
                        Lines = journalLines
                    });
                
                _logger.LogInformation("Successfully processed interest payment for account {AccountId}", accountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing interest payment for account {AccountId}", accountId);
                throw;
            }
        }

        // Helper method to get the corresponding GL account for a customer bank account
        private async Task<int> GetBankAccountIdForCustomerAccountAsync(int customerAccountId)
        {
            // In a real implementation, this would look up the mapping between
            // a customer's bank account and the corresponding GL account
            // For now, we'll use a placeholder implementation
            return await _chartOfAccountService.GetBankAccountIdForCustomerAsync(customerAccountId);
        }
    }
}