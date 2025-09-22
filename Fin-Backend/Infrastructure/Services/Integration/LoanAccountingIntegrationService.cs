using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FinTech.Application.Interfaces.Services;
using FinTech.Application.Services.Integration;
using System.Collections.Generic;
using FinTech.Application.Interfaces.Integration;

namespace FinTech.Infrastructure.Services.Integration
{
    public class LoanAccountingIntegrationService : ILoanAccountingIntegrationService
    {
        private readonly ILogger<LoanAccountingIntegrationService> _logger;
        private readonly IJournalEntryService _journalEntryService;
        private readonly IChartOfAccountService _chartOfAccountService;

        public LoanAccountingIntegrationService(
            ILogger<LoanAccountingIntegrationService> logger,
            IJournalEntryService journalEntryService,
            IChartOfAccountService chartOfAccountService)
        {
            _logger = logger;
            _journalEntryService = journalEntryService;
            _chartOfAccountService = chartOfAccountService;
        }

        public async Task ProcessLoanDisbursementAsync(int loanId, decimal amount, string reference, string description)
        {
            try
            {
                _logger.LogInformation("Processing loan disbursement for loan {LoanId} with amount {Amount}", loanId, amount);
                
                // Get the accounts from chart of accounts
                var loanReceivableAccountId = await _chartOfAccountService.GetLoanReceivableAccountIdAsync();
                var cashAccountId = await _chartOfAccountService.GetCashAccountIdAsync();
                
                // Create journal entry lines
                var journalLines = new List<JournalEntryLineDto>
                {
                    new JournalEntryLineDto
                    {
                        AccountId = loanReceivableAccountId,
                        Description = $"Loan receivable for loan {loanId}",
                        DebitAmount = amount,
                        CreditAmount = 0
                    },
                    new JournalEntryLineDto
                    {
                        AccountId = cashAccountId,
                        Description = $"Cash disbursed for loan {loanId}",
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
                        Source = "Loans",
                        Lines = journalLines
                    });
                
                _logger.LogInformation("Successfully processed loan disbursement for loan {LoanId}", loanId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing loan disbursement for loan {LoanId}", loanId);
                throw;
            }
        }

        public async Task ProcessLoanRepaymentAsync(int loanId, decimal principalAmount, decimal interestAmount, string reference, string description)
        {
            try
            {
                decimal totalAmount = principalAmount + interestAmount;
                _logger.LogInformation("Processing loan repayment for loan {LoanId} with principal {PrincipalAmount} and interest {InterestAmount}", 
                    loanId, principalAmount, interestAmount);
                
                // Get the accounts from chart of accounts
                var loanReceivableAccountId = await _chartOfAccountService.GetLoanReceivableAccountIdAsync();
                var interestIncomeAccountId = await _chartOfAccountService.GetInterestIncomeAccountIdAsync();
                var cashAccountId = await _chartOfAccountService.GetCashAccountIdAsync();
                
                // Create journal entry lines
                var journalLines = new List<JournalEntryLineDto>
                {
                    new JournalEntryLineDto
                    {
                        AccountId = cashAccountId,
                        Description = $"Cash received for loan repayment {loanId}",
                        DebitAmount = totalAmount,
                        CreditAmount = 0
                    },
                    new JournalEntryLineDto
                    {
                        AccountId = loanReceivableAccountId,
                        Description = $"Principal repayment for loan {loanId}",
                        DebitAmount = 0,
                        CreditAmount = principalAmount
                    },
                    new JournalEntryLineDto
                    {
                        AccountId = interestIncomeAccountId,
                        Description = $"Interest income for loan {loanId}",
                        DebitAmount = 0,
                        CreditAmount = interestAmount
                    }
                };
                
                // Create the journal entry
                await _journalEntryService.CreateJournalEntryAsync(
                    new JournalEntryDto
                    {
                        TransactionDate = DateTime.UtcNow,
                        Reference = reference,
                        Description = description,
                        Source = "Loans",
                        Lines = journalLines
                    });
                
                _logger.LogInformation("Successfully processed loan repayment for loan {LoanId}", loanId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing loan repayment for loan {LoanId}", loanId);
                throw;
            }
        }

        public async Task ProcessLoanWriteOffAsync(int loanId, decimal amount, string reference, string description)
        {
            try
            {
                _logger.LogInformation("Processing loan write-off for loan {LoanId} with amount {Amount}", loanId, amount);
                
                // Get the accounts from chart of accounts
                var loanReceivableAccountId = await _chartOfAccountService.GetLoanReceivableAccountIdAsync();
                var writeOffAccountId = await _chartOfAccountService.GetLoanWriteOffAccountIdAsync();
                
                // Create journal entry lines
                var journalLines = new List<JournalEntryLineDto>
                {
                    new JournalEntryLineDto
                    {
                        AccountId = writeOffAccountId,
                        Description = $"Loan write-off expense for loan {loanId}",
                        DebitAmount = amount,
                        CreditAmount = 0
                    },
                    new JournalEntryLineDto
                    {
                        AccountId = loanReceivableAccountId,
                        Description = $"Loan receivable write-off for loan {loanId}",
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
                        Source = "Loans",
                        Lines = journalLines
                    });
                
                _logger.LogInformation("Successfully processed loan write-off for loan {LoanId}", loanId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing loan write-off for loan {LoanId}", loanId);
                throw;
            }
        }

        public async Task ProcessLoanInterestAccrualAsync(int loanId, decimal amount, string reference, string description)
        {
            try
            {
                _logger.LogInformation("Processing loan interest accrual for loan {LoanId} with amount {Amount}", loanId, amount);
                
                // Get the accounts from chart of accounts
                var interestReceivableAccountId = await _chartOfAccountService.GetBankAccountIdForCustomerAsync(loanId); // Placeholder for interest receivable
                var interestIncomeAccountId = await _chartOfAccountService.GetInterestIncomeAccountIdAsync();
                
                // Create journal entry lines
                var journalLines = new List<JournalEntryLineDto>
                {
                    new JournalEntryLineDto
                    {
                        AccountId = interestReceivableAccountId,
                        Description = $"Interest receivable for loan {loanId}",
                        DebitAmount = amount,
                        CreditAmount = 0
                    },
                    new JournalEntryLineDto
                    {
                        AccountId = interestIncomeAccountId,
                        Description = $"Interest income accrual for loan {loanId}",
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
                        Source = "Loans",
                        Lines = journalLines
                    });
                
                _logger.LogInformation("Successfully processed loan interest accrual for loan {LoanId}", loanId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing loan interest accrual for loan {LoanId}", loanId);
                throw;
            }
        }

        public async Task ProcessLoanFeeChargeAsync(int loanId, decimal amount, string feeType, string reference, string description)
        {
            try
            {
                _logger.LogInformation("Processing loan fee charge for loan {LoanId} with amount {Amount}, fee type {FeeType}", 
                    loanId, amount, feeType);
                
                // Get the accounts from chart of accounts
                var feeIncomeAccountId = await _chartOfAccountService.GetFeeIncomeAccountIdAsync(feeType);
                var cashAccountId = await _chartOfAccountService.GetCashAccountIdAsync();
                
                // Create journal entry lines
                var journalLines = new List<JournalEntryLineDto>
                {
                    new JournalEntryLineDto
                    {
                        AccountId = cashAccountId,
                        Description = $"Cash received for {feeType} fee from loan {loanId}",
                        DebitAmount = amount,
                        CreditAmount = 0
                    },
                    new JournalEntryLineDto
                    {
                        AccountId = feeIncomeAccountId,
                        Description = $"{feeType} fee income from loan {loanId}",
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
                        Source = "Loans",
                        Lines = journalLines
                    });
                
                _logger.LogInformation("Successfully processed loan fee charge for loan {LoanId}", loanId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing loan fee charge for loan {LoanId}", loanId);
                throw;
            }
        }

        Task ILoanAccountingIntegrationService.ProcessLoanDisbursementAsync(LoanAccount loanAccount, LoanTransaction transaction, string tenantId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task ILoanAccountingIntegrationService.ProcessLoanRepaymentAsync(LoanAccount loanAccount, LoanTransaction transaction, string tenantId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task ILoanAccountingIntegrationService.ProcessInterestAccrualAsync(IEnumerable<LoanAccount> loanAccounts, DateTime accrualDate, string tenantId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task ILoanAccountingIntegrationService.ProcessLoanWriteOffAsync(LoanAccount loanAccount, decimal writeOffAmount, string tenantId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task ILoanAccountingIntegrationService.ProcessLoanFeeChargeAsync(LoanAccount loanAccount, decimal feeAmount, string feeType, string tenantId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}