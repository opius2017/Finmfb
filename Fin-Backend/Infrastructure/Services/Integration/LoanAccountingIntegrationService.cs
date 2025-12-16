using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FinTech.Core.Application.Interfaces.Services;
using FinTech.Core.Application.Interfaces.Services.Integration;
using FinTech.Core.Application.Interfaces.Services.Accounting;
using System.Collections.Generic;
using System.Collections.Generic;
// using FinTech.Core.Application.Interfaces.Integration;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Core.Application.DTOs.GeneralLedger.Journal;

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

        public async Task ProcessLoanDisbursementAsync(string loanId, decimal amount, string reference, string description)
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

        public async Task ProcessLoanRepaymentAsync(string loanId, decimal principalAmount, decimal interestAmount, string reference, string description)
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

        public async Task ProcessLoanWriteOffAsync(string loanId, decimal amount, string reference, string description)
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

        public async Task ProcessLoanInterestAccrualAsync(string loanId, decimal amount, string reference, string description)
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

        public async Task ProcessLoanFeeChargeAsync(string loanId, decimal amount, string feeType, string reference, string description)
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

        async Task ILoanAccountingIntegrationService.ProcessLoanDisbursementAsync(LoanAccount loanAccount, LoanTransaction transaction, string tenantId, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing loan disbursement for loan {LoanId}, Amount: {Amount}", 
                    loanAccount.Id, transaction.Amount);

                // Get chart of accounts
                var loanReceivableAccountId = await _chartOfAccountService.GetLoanReceivableAccountIdAsync();
                var cashAccountId = await _chartOfAccountService.GetCashAccountIdAsync();

                // Create journal entry for loan disbursement
                var journalEntry = new
                {
                    Description = $"Loan disbursement for {loanAccount.AccountNumber}",
                    Reference = transaction.TransactionReference,
                    TransactionDate = transaction.TransactionDate,
                    Lines = new List<object>
                    {
                        // Debit: Loan Receivable
                        new { AccountId = loanReceivableAccountId, Debit = transaction.Amount, Credit = 0m },
                        // Credit: Cash
                        new { AccountId = cashAccountId, Debit = 0m, Credit = transaction.Amount }
                    }
                };

                await _journalEntryService.CreateJournalEntryAsync(journalEntry, tenantId);
                
                _logger.LogInformation("Successfully processed loan disbursement for loan {LoanId}", loanAccount.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing loan disbursement for loan {LoanId}", loanAccount.Id);
                throw;
            }
        }

        async Task ILoanAccountingIntegrationService.ProcessLoanRepaymentAsync(LoanAccount loanAccount, LoanTransaction transaction, string tenantId, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing loan repayment for loan {LoanId}, Amount: {Amount}", 
                    loanAccount.Id, transaction.Amount);

                // Get chart of accounts
                var cashAccountId = await _chartOfAccountService.GetCashAccountIdAsync();
                var loanReceivableAccountId = await _chartOfAccountService.GetLoanReceivableAccountIdAsync();
                var interestIncomeAccountId = await _chartOfAccountService.GetInterestIncomeAccountIdAsync();

                var principalAmount = transaction.PrincipalAmount;
                var interestAmount = transaction.InterestAmount;

                var lines = new List<object>
                {
                    // Debit: Cash
                    new { AccountId = cashAccountId, Debit = transaction.Amount, Credit = 0m }
                };

                // Credit: Loan Receivable (Principal)
                if (principalAmount > 0)
                {
                    lines.Add(new { AccountId = loanReceivableAccountId, Debit = 0m, Credit = principalAmount });
                }

                // Credit: Interest Income
                if (interestAmount > 0)
                {
                    lines.Add(new { AccountId = interestIncomeAccountId, Debit = 0m, Credit = interestAmount });
                }

                // Create journal entry for loan repayment
                var journalEntry = new
                {
                    Description = $"Loan repayment for {loanAccount.AccountNumber}",
                    Reference = transaction.TransactionReference,
                    TransactionDate = transaction.TransactionDate,
                    Lines = lines
                };

                await _journalEntryService.CreateJournalEntryAsync(journalEntry, tenantId);
                
                _logger.LogInformation("Successfully processed loan repayment for loan {LoanId}", loanAccount.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing loan repayment for loan {LoanId}", loanAccount.Id);
                throw;
            }
        }

        async Task ILoanAccountingIntegrationService.ProcessInterestAccrualAsync(IEnumerable<LoanAccount> loanAccounts, DateTime accrualDate, string tenantId, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing interest accrual for {Count} loans on {Date}", 
                    loanAccounts.Count(), accrualDate);

                // Get chart of accounts
                var interestReceivableAccountId = await _chartOfAccountService.GetInterestReceivableAccountIdAsync();
                var interestIncomeAccountId = await _chartOfAccountService.GetInterestIncomeAccountIdAsync();

                foreach (var loanAccount in loanAccounts)
                {
                    // Calculate daily interest accrual
                    var dailyInterest = CalculateDailyInterest(loanAccount);

                    if (dailyInterest <= 0) continue;

                    // Create journal entry for interest accrual
                    var journalEntry = new
                    {
                        Description = $"Interest accrual for loan {loanAccount.AccountNumber} - {accrualDate:yyyy-MM-dd}",
                        Reference = $"INT-ACCR-{loanAccount.Id}-{accrualDate:yyyyMMdd}",
                        TransactionDate = accrualDate,
                        Lines = new List<object>
                        {
                            // Debit: Interest Receivable
                            new { AccountId = interestReceivableAccountId, Debit = dailyInterest, Credit = 0m },
                            // Credit: Interest Income
                            new { AccountId = interestIncomeAccountId, Debit = 0m, Credit = dailyInterest }
                        }
                    };

                    await _journalEntryService.CreateJournalEntryAsync(journalEntry, tenantId);
                }
                
                _logger.LogInformation("Successfully processed interest accrual for {Count} loans", loanAccounts.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing interest accrual");
                throw;
            }
        }

        async Task ILoanAccountingIntegrationService.ProcessLoanWriteOffAsync(LoanAccount loanAccount, decimal writeOffAmount, string tenantId, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing loan write-off for loan {LoanId}, Amount: {Amount}", 
                    loanAccount.Id, writeOffAmount);

                // Get chart of accounts
                var badDebtExpenseAccountId = await _chartOfAccountService.GetBadDebtExpenseAccountIdAsync();
                var loanReceivableAccountId = await _chartOfAccountService.GetLoanReceivableAccountIdAsync();
                var loanLossProvisionAccountId = await _chartOfAccountService.GetLoanLossProvisionAccountIdAsync();

                // Create journal entry for loan write-off
                var journalEntry = new
                {
                    Description = $"Loan write-off for {loanAccount.AccountNumber}",
                    Reference = $"WO-{loanAccount.Id}-{DateTime.UtcNow:yyyyMMdd}",
                    TransactionDate = DateTime.UtcNow,
                    Lines = new List<object>
                    {
                        // Debit: Bad Debt Expense
                        new { AccountId = badDebtExpenseAccountId, Debit = writeOffAmount, Credit = 0m },
                        // Debit: Loan Loss Provision (if exists)
                        new { AccountId = loanLossProvisionAccountId, Debit = 0m, Credit = 0m },
                        // Credit: Loan Receivable
                        new { AccountId = loanReceivableAccountId, Debit = 0m, Credit = writeOffAmount }
                    }
                };

                await _journalEntryService.CreateJournalEntryAsync(journalEntry, tenantId);
                
                _logger.LogInformation("Successfully processed loan write-off for loan {LoanId}", loanAccount.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing loan write-off for loan {LoanId}", loanAccount.Id);
                throw;
            }
        }

        async Task ILoanAccountingIntegrationService.ProcessLoanFeeChargeAsync(LoanAccount loanAccount, decimal feeAmount, string feeType, string tenantId, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing loan fee charge for loan {LoanId}, Fee: {FeeType}, Amount: {Amount}", 
                    loanAccount.Id, feeType, feeAmount);

                // Get chart of accounts
                var feeReceivableAccountId = await _chartOfAccountService.GetFeeReceivableAccountIdAsync();
                var feeIncomeAccountId = await _chartOfAccountService.GetFeeIncomeAccountIdAsync();

                // Create journal entry for fee charge
                var journalEntry = new
                {
                    Description = $"{feeType} fee for loan {loanAccount.AccountNumber}",
                    Reference = $"FEE-{loanAccount.Id}-{DateTime.UtcNow:yyyyMMdd}",
                    TransactionDate = DateTime.UtcNow,
                    Lines = new List<object>
                    {
                        // Debit: Fee Receivable
                        new { AccountId = feeReceivableAccountId, Debit = feeAmount, Credit = 0m },
                        // Credit: Fee Income
                        new { AccountId = feeIncomeAccountId, Debit = 0m, Credit = feeAmount }
                    }
                };

                await _journalEntryService.CreateJournalEntryAsync(journalEntry, tenantId);
                
                _logger.LogInformation("Successfully processed loan fee charge for loan {LoanId}", loanAccount.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing loan fee charge for loan {LoanId}", loanAccount.Id);
                throw;
            }
        }

        private decimal CalculateDailyInterest(LoanAccount loanAccount)
        {
            // Calculate daily interest: (Principal * Annual Rate) / 365
            var annualRate = (loanAccount.Loan?.InterestRate ?? 0m) / 100m;
            var dailyRate = annualRate / 365m;
            return (loanAccount.Loan?.PrincipalAmount ?? 0m) * dailyRate;
        }
    }
}
