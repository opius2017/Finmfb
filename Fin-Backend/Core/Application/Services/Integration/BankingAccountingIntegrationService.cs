using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Application.Services;
using DtoJournalEntryService = FinTech.Core.Application.Interfaces.Services.IJournalEntryService;
using FinTech.Core.Domain.Entities.Deposits;

namespace FinTech.Core.Application.Services.Integration
{
    public interface IBankingAccountingIntegrationService
    {
        Task<string> ProcessDepositTransactionAsync(
            DepositTransaction transaction, 
            string financialPeriodId, 
            CancellationToken cancellationToken = default);
            
        Task<string> ProcessWithdrawalTransactionAsync(
            DepositTransaction transaction, 
            string financialPeriodId, 
            CancellationToken cancellationToken = default);
            
        Task<string> ProcessTransferTransactionAsync(
            DepositTransaction sourceTransaction,
            DepositTransaction destinationTransaction,
            string financialPeriodId,
            CancellationToken cancellationToken = default);
            
        Task<string> ProcessInterestAccrualAsync(
            IEnumerable<DepositTransaction> interestTransactions, 
            string financialPeriodId, 
            DateTime accrualDate,
            CancellationToken cancellationToken = default);

        // Methods from the separate interface file that need primitive parameters
        Task ProcessDepositAsync(int accountId, decimal amount, string reference, string description);
        Task ProcessWithdrawalAsync(int accountId, decimal amount, string reference, string description);
        Task ProcessTransferAsync(int fromAccountId, int toAccountId, decimal amount, string reference, string description);
        Task ProcessFeeChargeAsync(int accountId, decimal amount, string feeType, string reference, string description);
        Task ProcessInterestPaymentAsync(int accountId, decimal amount, string reference, string description);
    }

    public class BankingAccountingIntegrationService : IBankingAccountingIntegrationService
    {
        private readonly DtoJournalEntryService _journalEntryService;

        public BankingAccountingIntegrationService(
            DtoJournalEntryService journalEntryService)
        {
            _journalEntryService = journalEntryService ?? throw new ArgumentNullException(nameof(journalEntryService));
        }

        public async Task<string> ProcessDepositTransactionAsync(
            DepositTransaction transaction, 
            string financialPeriodId, 
            CancellationToken cancellationToken = default)
        {
            // Validate inputs
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            if (transaction.Amount <= 0)
                throw new ArgumentException("Deposit amount must be greater than zero", nameof(transaction));

            // Create journal entry using DTO
            var journalEntry = new JournalEntryDto
            {
                Description = $"Deposit to account {transaction.AccountId} - {transaction.TransactionReference}",
                TransactionDate = transaction.TransactionDate,
                Reference = transaction.TransactionReference,
                Source = "BankingIntegration",
                Lines = new List<JournalEntryLineDto>
                {
                    // Debit Cash (using hardcoded account ID for now)
                    new JournalEntryLineDto
                    {
                        AccountId = 1010, // Cash account
                        Description = $"Deposit to account {transaction.AccountId}",
                        DebitAmount = transaction.Amount,
                        CreditAmount = 0
                    },
                    // Credit Customer Deposits
                    new JournalEntryLineDto
                    {
                        AccountId = 2010, // Customer deposits liability
                        Description = $"Customer deposit for account {transaction.AccountId}",
                        DebitAmount = 0,
                        CreditAmount = transaction.Amount
                    }
                }
            };

            // Create the journal entry
            var journalEntryDto = await _journalEntryService.CreateJournalEntryAsync(journalEntry);
            
            // Return the ID as string (convert from int if needed)
            return journalEntryDto.Id.ToString();
        }

        public async Task<string> ProcessWithdrawalTransactionAsync(
            DepositTransaction transaction, 
            string financialPeriodId, 
            CancellationToken cancellationToken = default)
        {
            // Validate inputs
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            if (transaction.Amount <= 0)
                throw new ArgumentException("Withdrawal amount must be greater than zero", nameof(transaction));

            // Create journal entry using DTO
            var journalEntry = new JournalEntryDto
            {
                Description = $"Withdrawal from account {transaction.AccountId} - {transaction.TransactionReference}",
                TransactionDate = transaction.TransactionDate,
                Reference = transaction.TransactionReference,
                Source = "BankingIntegration",
                Lines = new List<JournalEntryLineDto>
                {
                    // Debit Customer Deposits
                    new JournalEntryLineDto
                    {
                        AccountId = 2010, // Customer deposits liability
                        Description = $"Withdrawal from account {transaction.AccountId}",
                        DebitAmount = transaction.Amount,
                        CreditAmount = 0
                    },
                    // Credit Cash
                    new JournalEntryLineDto
                    {
                        AccountId = 1010, // Cash account
                        Description = $"Cash withdrawal from account {transaction.AccountId}",
                        DebitAmount = 0,
                        CreditAmount = transaction.Amount
                    }
                }
            };

            // Create the journal entry
            var journalEntryDto = await _journalEntryService.CreateJournalEntryAsync(journalEntry);
            
            // Return the ID as string (convert from int if needed)
            return journalEntryDto.Id.ToString();
        }

        public async Task<string> ProcessTransferTransactionAsync(
            DepositTransaction sourceTransaction,
            DepositTransaction destinationTransaction,
            string financialPeriodId,
            CancellationToken cancellationToken = default)
        {
            // Validate inputs
            if (sourceTransaction == null)
                throw new ArgumentNullException(nameof(sourceTransaction));
            
            if (destinationTransaction == null)
                throw new ArgumentNullException(nameof(destinationTransaction));

            if (sourceTransaction.Amount <= 0 || destinationTransaction.Amount <= 0)
                throw new ArgumentException("Transfer amount must be greater than zero");

            if (sourceTransaction.Amount != destinationTransaction.Amount)
                throw new ArgumentException("Source and destination amounts must be equal");

            // Create journal entry using DTO
            var journalEntry = new JournalEntryDto
            {
                Description = $"Transfer from account {sourceTransaction.AccountId} to account {destinationTransaction.AccountId} - {sourceTransaction.TransactionReference}",
                TransactionDate = sourceTransaction.TransactionDate,
                Reference = sourceTransaction.TransactionReference,
                Source = "BankingIntegration",
                Lines = new List<JournalEntryLineDto>
                {
                    // Debit source account (reduce deposit liability)
                    new JournalEntryLineDto
                    {
                        AccountId = 2010, // Customer deposits liability
                        Description = $"Transfer from account {sourceTransaction.AccountId}",
                        DebitAmount = sourceTransaction.Amount,
                        CreditAmount = 0
                    },
                    // Credit destination account (increase deposit liability)
                    new JournalEntryLineDto
                    {
                        AccountId = 2010, // Customer deposits liability  
                        Description = $"Transfer to account {destinationTransaction.AccountId}",
                        DebitAmount = 0,
                        CreditAmount = destinationTransaction.Amount
                    }
                }
            };

            // Create the journal entry
            var journalEntryDto = await _journalEntryService.CreateJournalEntryAsync(journalEntry);
            
            // Return the ID as string (convert from int if needed)
            return journalEntryDto.Id.ToString();
        }

        public async Task<string> ProcessInterestAccrualAsync(
            IEnumerable<DepositTransaction> interestTransactions, 
            string financialPeriodId, 
            DateTime accrualDate,
            CancellationToken cancellationToken = default)
        {
            // Validate inputs
            if (interestTransactions == null || !interestTransactions.Any())
                throw new ArgumentException("Interest transactions cannot be null or empty", nameof(interestTransactions));

            // Calculate total interest
            decimal totalInterest = interestTransactions.Sum(t => t.Amount);

            // Create journal entry using DTO
            var journalEntry = new JournalEntryDto
            {
                Description = $"Interest accrual for deposit accounts - {accrualDate:yyyy-MM-dd}",
                TransactionDate = accrualDate,
                Reference = $"INT-{accrualDate:yyyyMMdd}",
                Source = "BankingIntegration",
                Lines = new List<JournalEntryLineDto>
                {
                    // Debit Interest Expense
                    new JournalEntryLineDto
                    {
                        AccountId = 5020, // Interest expense
                        Description = $"Interest accrual for deposit accounts - {accrualDate:yyyy-MM-dd}",
                        DebitAmount = totalInterest,
                        CreditAmount = 0
                    },
                    // Credit Customer Deposits
                    new JournalEntryLineDto
                    {
                        AccountId = 2010, // Customer deposits liability
                        Description = $"Interest accrual for deposit accounts - {accrualDate:yyyy-MM-dd}",
                        DebitAmount = 0,
                        CreditAmount = totalInterest
                    }
                }
            };

            // Create the journal entry
            var journalEntryDto = await _journalEntryService.CreateJournalEntryAsync(journalEntry);
            
            // Return the ID as string (convert from int if needed)
            return journalEntryDto.Id.ToString();
        }

        // Interface methods that take primitive parameters (from IBankingAccountingIntegrationService.cs)
        public async Task ProcessDepositAsync(int accountId, decimal amount, string reference, string description)
        {
            // Create journal entry using DTO
            var journalEntry = new JournalEntryDto
            {
                Description = description,
                TransactionDate = DateTime.Now,
                Reference = reference,
                Source = "BankingIntegration",
                Lines = new List<JournalEntryLineDto>
                {
                    new JournalEntryLineDto
                    {
                        AccountId = 1010, // Cash account
                        Description = description,
                        DebitAmount = amount,
                        CreditAmount = 0
                    },
                    new JournalEntryLineDto
                    {
                        AccountId = 2010, // Customer deposits
                        Description = description,
                        DebitAmount = 0,
                        CreditAmount = amount
                    }
                }
            };

            await _journalEntryService.CreateJournalEntryAsync(journalEntry);
        }

        public async Task ProcessWithdrawalAsync(int accountId, decimal amount, string reference, string description)
        {
            // Create journal entry using DTO
            var journalEntry = new JournalEntryDto
            {
                Description = description,
                TransactionDate = DateTime.Now,
                Reference = reference,
                Source = "BankingIntegration",
                Lines = new List<JournalEntryLineDto>
                {
                    new JournalEntryLineDto
                    {
                        AccountId = 2010, // Customer deposits
                        Description = description,
                        DebitAmount = amount,
                        CreditAmount = 0
                    },
                    new JournalEntryLineDto
                    {
                        AccountId = 1010, // Cash account
                        Description = description,
                        DebitAmount = 0,
                        CreditAmount = amount
                    }
                }
            };

            await _journalEntryService.CreateJournalEntryAsync(journalEntry);
        }

        public async Task ProcessTransferAsync(int fromAccountId, int toAccountId, decimal amount, string reference, string description)
        {
            // For simplicity, transfers are internal movements between customer deposit accounts
            var journalEntry = new JournalEntryDto
            {
                Description = description,
                TransactionDate = DateTime.Now,
                Reference = reference,
                Source = "BankingIntegration",
                Lines = new List<JournalEntryLineDto>
                {
                    new JournalEntryLineDto
                    {
                        AccountId = 2010, // Customer deposits (from)
                        Description = $"Transfer from account {fromAccountId}",
                        DebitAmount = amount,
                        CreditAmount = 0
                    },
                    new JournalEntryLineDto
                    {
                        AccountId = 2010, // Customer deposits (to)
                        Description = $"Transfer to account {toAccountId}",
                        DebitAmount = 0,
                        CreditAmount = amount
                    }
                }
            };

            await _journalEntryService.CreateJournalEntryAsync(journalEntry);
        }

        public async Task ProcessFeeChargeAsync(int accountId, decimal amount, string feeType, string reference, string description)
        {
            var journalEntry = new JournalEntryDto
            {
                Description = description,
                TransactionDate = DateTime.Now,
                Reference = reference,
                Source = "BankingIntegration",
                Lines = new List<JournalEntryLineDto>
                {
                    new JournalEntryLineDto
                    {
                        AccountId = 2010, // Customer deposits
                        Description = description,
                        DebitAmount = amount,
                        CreditAmount = 0
                    },
                    new JournalEntryLineDto
                    {
                        AccountId = 4010, // Fee income
                        Description = description,
                        DebitAmount = 0,
                        CreditAmount = amount
                    }
                }
            };

            await _journalEntryService.CreateJournalEntryAsync(journalEntry);
        }

        public async Task ProcessInterestPaymentAsync(int accountId, decimal amount, string reference, string description)
        {
            var journalEntry = new JournalEntryDto
            {
                Description = description,
                TransactionDate = DateTime.Now,
                Reference = reference,
                Source = "BankingIntegration",
                Lines = new List<JournalEntryLineDto>
                {
                    new JournalEntryLineDto
                    {
                        AccountId = 5020, // Interest expense
                        Description = description,
                        DebitAmount = amount,
                        CreditAmount = 0
                    },
                    new JournalEntryLineDto
                    {
                        AccountId = 2010, // Customer deposits
                        Description = description,
                        DebitAmount = 0,
                        CreditAmount = amount
                    }
                }
            };

            await _journalEntryService.CreateJournalEntryAsync(journalEntry);
        }
    }
}
