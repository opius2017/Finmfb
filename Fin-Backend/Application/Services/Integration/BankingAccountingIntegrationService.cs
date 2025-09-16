using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Application.Services.Accounting;
using FinTech.Domain.Entities.Accounting;
using FinTech.Domain.Entities.Deposits;
using FinTech.Domain.Repositories.Accounting;

namespace FinTech.Application.Services.Integration
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
    }

    public class BankingAccountingIntegrationService : IBankingAccountingIntegrationService
    {
        private readonly IJournalEntryService _journalEntryService;
        private readonly IChartOfAccountService _chartOfAccountService;
        private readonly IFinancialPeriodService _financialPeriodService;
        private readonly IChartOfAccountRepository _chartOfAccountRepository;

        public BankingAccountingIntegrationService(
            IJournalEntryService journalEntryService,
            IChartOfAccountService chartOfAccountService,
            IFinancialPeriodService financialPeriodService,
            IChartOfAccountRepository chartOfAccountRepository)
        {
            _journalEntryService = journalEntryService ?? throw new ArgumentNullException(nameof(journalEntryService));
            _chartOfAccountService = chartOfAccountService ?? throw new ArgumentNullException(nameof(chartOfAccountService));
            _financialPeriodService = financialPeriodService ?? throw new ArgumentNullException(nameof(financialPeriodService));
            _chartOfAccountRepository = chartOfAccountRepository ?? throw new ArgumentNullException(nameof(chartOfAccountRepository));
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

            // Get the relevant GL accounts
            var cashAccount = await _chartOfAccountRepository.GetByAccountNumberAsync("1010", cancellationToken);  // Cash account
            var depositAccount = await _chartOfAccountRepository.GetByAccountNumberAsync("2010", cancellationToken);  // Customer deposits liability

            if (cashAccount == null || depositAccount == null)
                throw new InvalidOperationException("Required GL accounts not found");

            // Create journal entry
            var journalEntry = new JournalEntry
            {
                Description = $"Deposit to account {transaction.AccountNumber} - {transaction.TransactionReference}",
                EntryDate = transaction.TransactionDate,
                EntryType = JournalEntryType.Standard,
                FinancialPeriodId = financialPeriodId,
                IsSystemGenerated = true,
                CreatedBy = "BankingIntegration",
                JournalEntryLines = new List<JournalEntryLine>
                {
                    // Debit Cash
                    new JournalEntryLine
                    {
                        AccountId = cashAccount.Id,
                        Description = $"Deposit to account {transaction.AccountNumber}",
                        DebitAmount = transaction.Amount,
                        CreditAmount = 0,
                        CreatedBy = "BankingIntegration"
                    },
                    // Credit Customer Deposits
                    new JournalEntryLine
                    {
                        AccountId = depositAccount.Id,
                        Description = $"Deposit to account {transaction.AccountNumber}",
                        DebitAmount = 0,
                        CreditAmount = transaction.Amount,
                        CreatedBy = "BankingIntegration"
                    }
                }
            };

            // Create, submit, approve, and post the journal entry
            var journalEntryId = await _journalEntryService.CreateJournalEntryAsync(journalEntry, cancellationToken);
            await _journalEntryService.SubmitForApprovalAsync(journalEntryId, "BankingIntegration", cancellationToken);
            await _journalEntryService.ApproveJournalEntryAsync(journalEntryId, "BankingIntegration", cancellationToken);
            await _journalEntryService.PostJournalEntryAsync(journalEntryId, "BankingIntegration", cancellationToken);

            return journalEntryId;
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

            // Get the relevant GL accounts
            var cashAccount = await _chartOfAccountRepository.GetByAccountNumberAsync("1010", cancellationToken);  // Cash account
            var depositAccount = await _chartOfAccountRepository.GetByAccountNumberAsync("2010", cancellationToken);  // Customer deposits liability

            if (cashAccount == null || depositAccount == null)
                throw new InvalidOperationException("Required GL accounts not found");

            // Create journal entry
            var journalEntry = new JournalEntry
            {
                Description = $"Withdrawal from account {transaction.AccountNumber} - {transaction.TransactionReference}",
                EntryDate = transaction.TransactionDate,
                EntryType = JournalEntryType.Standard,
                FinancialPeriodId = financialPeriodId,
                IsSystemGenerated = true,
                CreatedBy = "BankingIntegration",
                JournalEntryLines = new List<JournalEntryLine>
                {
                    // Debit Customer Deposits
                    new JournalEntryLine
                    {
                        AccountId = depositAccount.Id,
                        Description = $"Withdrawal from account {transaction.AccountNumber}",
                        DebitAmount = transaction.Amount,
                        CreditAmount = 0,
                        CreatedBy = "BankingIntegration"
                    },
                    // Credit Cash
                    new JournalEntryLine
                    {
                        AccountId = cashAccount.Id,
                        Description = $"Withdrawal from account {transaction.AccountNumber}",
                        DebitAmount = 0,
                        CreditAmount = transaction.Amount,
                        CreatedBy = "BankingIntegration"
                    }
                }
            };

            // Create, submit, approve, and post the journal entry
            var journalEntryId = await _journalEntryService.CreateJournalEntryAsync(journalEntry, cancellationToken);
            await _journalEntryService.SubmitForApprovalAsync(journalEntryId, "BankingIntegration", cancellationToken);
            await _journalEntryService.ApproveJournalEntryAsync(journalEntryId, "BankingIntegration", cancellationToken);
            await _journalEntryService.PostJournalEntryAsync(journalEntryId, "BankingIntegration", cancellationToken);

            return journalEntryId;
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

            // Get the relevant GL accounts - for transfers, we use a transfer clearing account
            var transferClearingAccount = await _chartOfAccountRepository.GetByAccountNumberAsync("2090", cancellationToken);  // Transfer clearing account
            var depositAccount = await _chartOfAccountRepository.GetByAccountNumberAsync("2010", cancellationToken);  // Customer deposits liability

            if (transferClearingAccount == null || depositAccount == null)
                throw new InvalidOperationException("Required GL accounts not found");

            // Create journal entry
            var journalEntry = new JournalEntry
            {
                Description = $"Transfer from {sourceTransaction.AccountNumber} to {destinationTransaction.AccountNumber} - {sourceTransaction.TransactionReference}",
                EntryDate = sourceTransaction.TransactionDate,
                EntryType = JournalEntryType.Standard,
                FinancialPeriodId = financialPeriodId,
                IsSystemGenerated = true,
                CreatedBy = "BankingIntegration",
                JournalEntryLines = new List<JournalEntryLine>
                {
                    // Debit Customer Deposits (Source Account)
                    new JournalEntryLine
                    {
                        AccountId = depositAccount.Id,
                        Description = $"Transfer from account {sourceTransaction.AccountNumber}",
                        DebitAmount = sourceTransaction.Amount,
                        CreditAmount = 0,
                        CreatedBy = "BankingIntegration"
                    },
                    // Credit Transfer Clearing
                    new JournalEntryLine
                    {
                        AccountId = transferClearingAccount.Id,
                        Description = $"Transfer from {sourceTransaction.AccountNumber} to {destinationTransaction.AccountNumber}",
                        DebitAmount = 0,
                        CreditAmount = sourceTransaction.Amount,
                        CreatedBy = "BankingIntegration"
                    },
                    // Debit Transfer Clearing
                    new JournalEntryLine
                    {
                        AccountId = transferClearingAccount.Id,
                        Description = $"Transfer from {sourceTransaction.AccountNumber} to {destinationTransaction.AccountNumber}",
                        DebitAmount = destinationTransaction.Amount,
                        CreditAmount = 0,
                        CreatedBy = "BankingIntegration"
                    },
                    // Credit Customer Deposits (Destination Account)
                    new JournalEntryLine
                    {
                        AccountId = depositAccount.Id,
                        Description = $"Transfer to account {destinationTransaction.AccountNumber}",
                        DebitAmount = 0,
                        CreditAmount = destinationTransaction.Amount,
                        CreatedBy = "BankingIntegration"
                    }
                }
            };

            // Create, submit, approve, and post the journal entry
            var journalEntryId = await _journalEntryService.CreateJournalEntryAsync(journalEntry, cancellationToken);
            await _journalEntryService.SubmitForApprovalAsync(journalEntryId, "BankingIntegration", cancellationToken);
            await _journalEntryService.ApproveJournalEntryAsync(journalEntryId, "BankingIntegration", cancellationToken);
            await _journalEntryService.PostJournalEntryAsync(journalEntryId, "BankingIntegration", cancellationToken);

            return journalEntryId;
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

            // Get the relevant GL accounts
            var interestExpenseAccount = await _chartOfAccountRepository.GetByAccountNumberAsync("5020", cancellationToken);  // Interest expense
            var depositAccount = await _chartOfAccountRepository.GetByAccountNumberAsync("2010", cancellationToken);  // Customer deposits liability

            if (interestExpenseAccount == null || depositAccount == null)
                throw new InvalidOperationException("Required GL accounts not found");

            // Calculate total interest
            decimal totalInterest = interestTransactions.Sum(t => t.Amount);

            // Create journal entry
            var journalEntry = new JournalEntry
            {
                Description = $"Interest accrual for deposit accounts - {accrualDate:yyyy-MM-dd}",
                EntryDate = accrualDate,
                EntryType = JournalEntryType.Standard,
                FinancialPeriodId = financialPeriodId,
                IsSystemGenerated = true,
                CreatedBy = "BankingIntegration",
                JournalEntryLines = new List<JournalEntryLine>
                {
                    // Debit Interest Expense
                    new JournalEntryLine
                    {
                        AccountId = interestExpenseAccount.Id,
                        Description = $"Interest accrual for deposit accounts - {accrualDate:yyyy-MM-dd}",
                        DebitAmount = totalInterest,
                        CreditAmount = 0,
                        CreatedBy = "BankingIntegration"
                    },
                    // Credit Customer Deposits
                    new JournalEntryLine
                    {
                        AccountId = depositAccount.Id,
                        Description = $"Interest accrual for deposit accounts - {accrualDate:yyyy-MM-dd}",
                        DebitAmount = 0,
                        CreditAmount = totalInterest,
                        CreatedBy = "BankingIntegration"
                    }
                }
            };

            // Create, submit, approve, and post the journal entry
            var journalEntryId = await _journalEntryService.CreateJournalEntryAsync(journalEntry, cancellationToken);
            await _journalEntryService.SubmitForApprovalAsync(journalEntryId, "BankingIntegration", cancellationToken);
            await _journalEntryService.ApproveJournalEntryAsync(journalEntryId, "BankingIntegration", cancellationToken);
            await _journalEntryService.PostJournalEntryAsync(journalEntryId, "BankingIntegration", cancellationToken);

            return journalEntryId;
        }
    }
}