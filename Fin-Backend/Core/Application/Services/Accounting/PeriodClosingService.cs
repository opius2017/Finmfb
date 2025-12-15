using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Accounting;
using FinTech.Core.Domain.Repositories.Accounting;
using FinTech.Core.Application.Interfaces.Services;

namespace FinTech.Core.Application.Services.Accounting
{
    public interface IPeriodClosingService
    {
        Task<PeriodClosingStatusDto> GetPeriodClosingStatusAsync(
            string financialPeriodId, 
            CancellationToken cancellationToken = default);
            
        Task<PeriodClosingStatusDto> InitiatePeriodClosingAsync(
            string financialPeriodId, 
            CancellationToken cancellationToken = default);
            
        Task<PeriodClosingStatusDto> ValidatePeriodClosingAsync(
            string financialPeriodId, 
            CancellationToken cancellationToken = default);
            
        Task<PeriodClosingStatusDto> PostClosingEntriesAsync(
            string financialPeriodId, 
            CancellationToken cancellationToken = default);
            
        Task<PeriodClosingStatusDto> CompletePeriodClosingAsync(
            string financialPeriodId, 
            CancellationToken cancellationToken = default);
            
        Task<PeriodClosingStatusDto> RollBackPeriodClosingAsync(
            string financialPeriodId, 
            string reason, 
            CancellationToken cancellationToken = default);
    }

    public class PeriodClosingService : IPeriodClosingService
    {
        private readonly IFinancialPeriodRepository _financialPeriodRepository;
        private readonly IJournalEntryRepository _journalEntryRepository;
        private readonly IChartOfAccountRepository _chartOfAccountRepository;
        private readonly IGeneralLedgerService _generalLedgerService;
        private readonly ITrialBalanceService _trialBalanceService;
        private readonly IFinancialStatementService _financialStatementService;
        
        public PeriodClosingService(
            IFinancialPeriodRepository financialPeriodRepository,
            IJournalEntryRepository journalEntryRepository,
            IChartOfAccountRepository chartOfAccountRepository,
            IGeneralLedgerService generalLedgerService,
            ITrialBalanceService trialBalanceService,
            IFinancialStatementService financialStatementService)
        {
            _financialPeriodRepository = financialPeriodRepository ?? throw new ArgumentNullException(nameof(financialPeriodRepository));
            _journalEntryRepository = journalEntryRepository ?? throw new ArgumentNullException(nameof(journalEntryRepository));
            _chartOfAccountRepository = chartOfAccountRepository ?? throw new ArgumentNullException(nameof(chartOfAccountRepository));
            _generalLedgerService = generalLedgerService ?? throw new ArgumentNullException(nameof(generalLedgerService));
            _trialBalanceService = trialBalanceService ?? throw new ArgumentNullException(nameof(trialBalanceService));
            _financialStatementService = financialStatementService ?? throw new ArgumentNullException(nameof(financialStatementService));
        }

        /// <summary>
        /// Gets the current status of the period closing process for a financial period
        /// </summary>
        public async Task<PeriodClosingStatusDto> GetPeriodClosingStatusAsync(
            string financialPeriodId, 
            CancellationToken cancellationToken = default)
        {
            var financialPeriod = await _financialPeriodRepository.GetByIdAsync(financialPeriodId, cancellationToken);
            if (financialPeriod == null)
            {
                throw new InvalidOperationException($"Financial period with ID {financialPeriodId} not found");
            }
            
            // Return the period closing status
            return CreatePeriodClosingStatusDto(financialPeriod);
        }

        /// <summary>
        /// Initiates the period closing process for a financial period
        /// </summary>
        public async Task<PeriodClosingStatusDto> InitiatePeriodClosingAsync(
            string financialPeriodId, 
            CancellationToken cancellationToken = default)
        {
            var financialPeriod = await _financialPeriodRepository.GetByIdAsync(financialPeriodId, cancellationToken);
            if (financialPeriod == null)
            {
                throw new InvalidOperationException($"Financial period with ID {financialPeriodId} not found");
            }
            
            // Check if the period is already closed or in the process of closing
            if (financialPeriod.IsClosed)
            {
                throw new InvalidOperationException($"Financial period {financialPeriod.Name} is already closed");
            }
            
            if (financialPeriod.ClosingStatus != ClosingStatus.NotStarted && 
                financialPeriod.ClosingStatus != ClosingStatus.Failed)
            {
                throw new InvalidOperationException($"Financial period {financialPeriod.Name} is already in the process of closing");
            }
            
            // Check if there are any unposted journal entries
            var unpostedEntries = await _journalEntryRepository.GetUnpostedEntriesByPeriodAsync(financialPeriodId, cancellationToken);
            if (unpostedEntries.Any())
            {
                throw new InvalidOperationException($"There are {unpostedEntries.Count} unposted journal entries for this period. All entries must be posted before closing.");
            }
            
            // Update the period status to Initiated
            financialPeriod.StartClosingProcess();
            await _financialPeriodRepository.UpdateAsync(financialPeriod, cancellationToken);
            
            // Return the updated period status
            return CreatePeriodClosingStatusDto(financialPeriod);
        }

        /// <summary>
        /// Validates the period closing process by checking for various conditions
        /// </summary>
        public async Task<PeriodClosingStatusDto> ValidatePeriodClosingAsync(
            string financialPeriodId, 
            CancellationToken cancellationToken = default)
        {
            var financialPeriod = await _financialPeriodRepository.GetByIdAsync(financialPeriodId, cancellationToken);
            if (financialPeriod == null)
            {
                throw new InvalidOperationException($"Financial period with ID {financialPeriodId} not found");
            }
            
            // Check if the period is in the right state for validation
            if (financialPeriod.ClosingStatus != ClosingStatus.Initiated)
            {
                throw new InvalidOperationException($"Financial period {financialPeriod.Name} is not in the Initiated state");
            }
            
            // Start validation
            var validationErrors = new List<string>();
            
            // Validate all journal entries are posted
            var unpostedEntries = await _journalEntryRepository.GetUnpostedEntriesByPeriodAsync(financialPeriodId, cancellationToken);
            if (unpostedEntries.Any())
            {
                validationErrors.Add($"There are {unpostedEntries.Count} unposted journal entries for this period.");
            }
            
            // Validate trial balance is in balance
            var trialBalance = await _trialBalanceService.GenerateTrialBalanceByFinancialPeriodAsync(financialPeriodId, cancellationToken: cancellationToken);
            if (Math.Abs(trialBalance.TotalDebits - trialBalance.TotalCredits) > 0.01m) // Allow small rounding differences
            {
                validationErrors.Add($"Trial balance is not in balance. Debits: {trialBalance.TotalDebits}, Credits: {trialBalance.TotalCredits}");
            }
            
            // Check for accounts with negative balances that shouldn't have them
            foreach (var account in trialBalance.Accounts)
            {
                bool hasNegativeBalance = false;
                
                switch (account.Classification)
                {
                    case "Asset":
                        // Asset accounts normally have debit balances, so a negative balance means credits > debits
                        hasNegativeBalance = (account.DebitBalance - account.CreditBalance) < 0;
                        break;
                    case "Liability":
                    case "Equity":
                        // Liability and Equity accounts normally have credit balances, so a negative balance means debits > credits
                        hasNegativeBalance = (account.CreditBalance - account.DebitBalance) < 0;
                        break;
                    case "Income":
                        // Income accounts normally have credit balances, so a negative balance means debits > credits
                        hasNegativeBalance = (account.CreditBalance - account.DebitBalance) < 0;
                        break;
                    case "Expense":
                        // Expense accounts normally have debit balances, so a negative balance means credits > debits
                        hasNegativeBalance = (account.DebitBalance - account.CreditBalance) < 0;
                        break;
                }
                
                if (hasNegativeBalance)
                {
                    validationErrors.Add($"Account {account.AccountNumber} ({account.AccountName}) has an abnormal negative balance.");
                }
            }
            
            // Update the period status based on validation results
            if (validationErrors.Any())
            {
                financialPeriod.SetValidationErrors(string.Join("\n", validationErrors));
                await _financialPeriodRepository.UpdateAsync(financialPeriod, cancellationToken);
                
                return CreatePeriodClosingStatusDto(financialPeriod, validationErrors);
            }
            else
            {
                financialPeriod.CompleteValidation();
                await _financialPeriodRepository.UpdateAsync(financialPeriod, cancellationToken);
                
                return CreatePeriodClosingStatusDto(financialPeriod);
            }
        }

        /// <summary>
        /// Posts the closing entries for the financial period
        /// </summary>
        public async Task<PeriodClosingStatusDto> PostClosingEntriesAsync(
            string financialPeriodId, 
            CancellationToken cancellationToken = default)
        {
            var financialPeriod = await _financialPeriodRepository.GetByIdAsync(financialPeriodId, cancellationToken);
            if (financialPeriod == null)
            {
                throw new InvalidOperationException($"Financial period with ID {financialPeriodId} not found");
            }
            
            // Check if the period is in the right state for posting closing entries
            if (financialPeriod.ClosingStatus != ClosingStatus.Validated)
            {
                throw new InvalidOperationException($"Financial period {financialPeriod.Name} is not in the Validated state");
            }
            
            // Get the income statement for this period
            var incomeStatement = await _financialStatementService.GenerateIncomeStatementAsync(
                financialPeriodId,
                false,
                false,
                null,
                cancellationToken);
                
            // Get a retained earnings account
            var retainedEarningsAccount = await _chartOfAccountRepository.GetByAccountNumberAsync("3100", cancellationToken);
            if (retainedEarningsAccount == null)
            {
                throw new InvalidOperationException("Retained earnings account (3100) not found");
            }
            
            // Get income and expense accounts
            var incomeAccounts = await _chartOfAccountRepository.GetByClassificationAsync(AccountClassification.Revenue, cancellationToken); // FinTech Best Practice: Use Revenue instead of Income
            var expenseAccounts = await _chartOfAccountRepository.GetByClassificationAsync(AccountClassification.Expense, cancellationToken);
            
            // Create a closing entry for income accounts
            if (incomeAccounts.Any())
            {
                var incomeClosingEntry = new JournalEntry(
                    $"CL-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}",
                    DateTime.UtcNow,
                    "Closing entry for income accounts",
                    JournalEntryType.YearEndClosing,
                    "Period End Closing",
                    null,
                    financialPeriodId,
                    "GeneralLedger");
                    
                foreach (var account in incomeAccounts)
                {
                    // Get the account balance
                    var accountBalance = await _generalLedgerService.GetAccountBalanceAsync(
                        account.Id, 
                        financialPeriod.StartDate,
                        financialPeriod.EndDate);
                        
                    if (accountBalance.Amount != 0)
                    {
                        // Income accounts normally have credit balances, so debit them to close
                        incomeClosingEntry.AddLine(
                            account.Id,
                            accountBalance.Amount,
                            0,
                            $"Closing {account.AccountName}");
                    }
                }
                
                // Add the offset to retained earnings
                incomeClosingEntry.AddLine(
                    retainedEarningsAccount.Id,
                    0,
                    incomeStatement.Income.Total,
                    "Closing income accounts to retained earnings");
                    
                // Post the journal entry
                await _journalEntryRepository.AddAsync(incomeClosingEntry, cancellationToken);
                incomeClosingEntry.MarkAsPosted("System");
                await _journalEntryRepository.UpdateAsync(incomeClosingEntry, cancellationToken);
            }
            
            // Create a closing entry for expense accounts
            if (expenseAccounts.Any())
            {
                var expenseClosingEntry = new JournalEntry(
                    $"CL-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}",
                    DateTime.UtcNow,
                    "Closing entry for expense accounts",
                    JournalEntryType.YearEndClosing,
                    "Period End Closing",
                    null,
                    financialPeriodId,
                    "GeneralLedger");
                    
                foreach (var account in expenseAccounts)
                {
                    // Get the account balance
                    var accountBalance = await _generalLedgerService.GetAccountBalanceAsync(
                        account.Id, 
                        financialPeriod.EndDate, 
                        financialPeriod.EndDate); // FinTech Best Practice: Correct argument order - DateTime then CancellationToken
                        
                    if (accountBalance.Amount != 0)
                    {
                        // Expense accounts normally have debit balances, so credit them to close
                        expenseClosingEntry.AddLine(
                            account.Id,
                            0,
                            accountBalance.Amount,
                            $"Closing {account.AccountName}");
                    }
                }
                
                // Add the offset to retained earnings
                expenseClosingEntry.AddLine(
                    retainedEarningsAccount.Id,
                    incomeStatement.Expenses.Total,
                    0,
                    "Closing expense accounts to retained earnings");
                    
                // Post the journal entry
                await _journalEntryRepository.AddAsync(expenseClosingEntry, cancellationToken);
                expenseClosingEntry.MarkAsPosted("System");
                await _journalEntryRepository.UpdateAsync(expenseClosingEntry, cancellationToken);
            }
            
            // Update the period status
            financialPeriod.CompleteClosingEntries();
            await _financialPeriodRepository.UpdateAsync(financialPeriod, cancellationToken);
            
            return CreatePeriodClosingStatusDto(financialPeriod);
        }

        /// <summary>
        /// Completes the period closing process
        /// </summary>
        public async Task<PeriodClosingStatusDto> CompletePeriodClosingAsync(
            string financialPeriodId, 
            CancellationToken cancellationToken = default)
        {
            var financialPeriod = await _financialPeriodRepository.GetByIdAsync(financialPeriodId, cancellationToken);
            if (financialPeriod == null)
            {
                throw new InvalidOperationException($"Financial period with ID {financialPeriodId} not found");
            }
            
            // Check if the period is in the right state for completion
            if (financialPeriod.ClosingStatus != ClosingStatus.ClosingEntriesPosted)
            {
                throw new InvalidOperationException($"Financial period {financialPeriod.Name} is not in the ClosingEntriesPosted state");
            }
            
            // Get the next period and ensure it exists
            var nextPeriod = await _financialPeriodRepository.GetNextPeriodAsync(financialPeriodId, cancellationToken);
            if (nextPeriod == null)
            {
                throw new InvalidOperationException("Next financial period not found. Please create the next period before closing this one.");
            }
            
            // Get all balance sheet accounts (Assets, Liabilities, Equity)
            var balanceSheetAccounts = new List<ChartOfAccount>();
            balanceSheetAccounts.AddRange(await _chartOfAccountRepository.GetByClassificationAsync(AccountClassification.Asset, cancellationToken));
            balanceSheetAccounts.AddRange(await _chartOfAccountRepository.GetByClassificationAsync(AccountClassification.Liability, cancellationToken));
            balanceSheetAccounts.AddRange(await _chartOfAccountRepository.GetByClassificationAsync(AccountClassification.Equity, cancellationToken));
            
            // Create an opening balance entry for the next period
            var openingBalanceEntry = new JournalEntry(
                $"OP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}",
                nextPeriod.StartDate,
                "Opening balances",
                JournalEntryType.SystemGenerated,
                "Period Opening",
                null,
                nextPeriod.Id,
                "GeneralLedger");
                
            decimal totalDebits = 0;
            decimal totalCredits = 0;
            
            // Add opening balances for each balance sheet account
            foreach (var account in balanceSheetAccounts)
            {
                // Get the account balance
                var accountBalance = await _generalLedgerService.GetAccountBalanceAsync(
                    account.Id, 
                    financialPeriod.EndDate, 
                    financialPeriod.EndDate); // FinTech Best Practice: Correct argument order
                    
                if (accountBalance.Amount != 0)
                {
                    switch (account.Classification)
                    {
                        case AccountClassification.Asset: // FinTech Best Practice: Compare enum to enum
                            // Asset accounts normally have debit balances
                            openingBalanceEntry.AddLine(
                                account.Id,
                                accountBalance.Amount,
                                0,
                                $"Opening balance for {account.AccountName}");
                                
                            totalDebits += accountBalance.Amount;
                            break;
                            
                        case AccountClassification.Liability: // FinTech Best Practice: Compare enum to enum
                        case AccountClassification.Equity:
                            // Liability and Equity accounts normally have credit balances
                            openingBalanceEntry.AddLine(
                                account.Id,
                                0,
                                accountBalance.Amount,
                                $"Opening balance for {account.AccountName}");
                                
                            totalCredits += accountBalance.Amount;
                            break;
                    }
                }
            }
            
            // Check if the opening balance entry is in balance
            if (Math.Abs(totalDebits - totalCredits) > 0.01m) // Allow small rounding differences
            {
                throw new InvalidOperationException($"Opening balance entry is not in balance. Debits: {totalDebits}, Credits: {totalCredits}");
            }
            
            // Post the opening balance entry
            await _journalEntryRepository.AddAsync(openingBalanceEntry, cancellationToken);
            openingBalanceEntry.MarkAsPosted("System");
            await _journalEntryRepository.UpdateAsync(openingBalanceEntry, cancellationToken);
            
            // Complete the period closing
            financialPeriod.Close();
            await _financialPeriodRepository.UpdateAsync(financialPeriod, cancellationToken);
            
            return CreatePeriodClosingStatusDto(financialPeriod);
        }

        /// <summary>
        /// Rolls back the period closing process
        /// </summary>
        public async Task<PeriodClosingStatusDto> RollBackPeriodClosingAsync(
            string financialPeriodId, 
            string reason, 
            CancellationToken cancellationToken = default)
        {
            var financialPeriod = await _financialPeriodRepository.GetByIdAsync(financialPeriodId, cancellationToken);
            if (financialPeriod == null)
            {
                throw new InvalidOperationException($"Financial period with ID {financialPeriodId} not found");
            }
            
            // Check if the period is fully closed - cannot roll back a fully closed period
            if (financialPeriod.IsClosed)
            {
                throw new InvalidOperationException($"Financial period {financialPeriod.Name} is fully closed and cannot be rolled back");
            }
            
            // Check if the period has already started the closing process
            if (financialPeriod.ClosingStatus == ClosingStatus.NotStarted)
            {
                throw new InvalidOperationException($"Financial period {financialPeriod.Name} has not started the closing process");
            }
            
            // If closing entries have been posted, delete them
            if (financialPeriod.ClosingStatus == ClosingStatus.ClosingEntriesPosted)
            {
                // Find and delete the closing entries
                var closingEntries = await _journalEntryRepository.GetByReferenceAsync(
                    financialPeriodId, 
                    "Period End Closing", 
                    cancellationToken);
                    
                foreach (var entry in closingEntries)
                {
                    await _journalEntryRepository.DeleteAsync(entry, cancellationToken); // FinTech Best Practice: Pass JournalEntry object not string Id
                }
            }
            
            // Reset the period status
            financialPeriod.RollBackClosingProcess(reason);
            await _financialPeriodRepository.UpdateAsync(financialPeriod, cancellationToken);
            
            return CreatePeriodClosingStatusDto(financialPeriod);
        }

        // Helper method to create a PeriodClosingStatusDto
        private PeriodClosingStatusDto CreatePeriodClosingStatusDto(
            FinancialPeriod financialPeriod,
            List<string> validationErrors = null)
        {
            return new PeriodClosingStatusDto
            {
                FinancialPeriodId = financialPeriod.Id,
                FinancialPeriodName = financialPeriod.Name,
                StartDate = financialPeriod.StartDate,
                EndDate = financialPeriod.EndDate,
                ClosingStatus = financialPeriod.ClosingStatus.ToString(),
                IsClosed = financialPeriod.IsClosed,
                ClosedAt = financialPeriod.ClosedDate,
                ClosedBy = financialPeriod.ClosedBy,
                ValidationErrors = validationErrors ?? (
                    string.IsNullOrEmpty(financialPeriod.ValidationErrors) 
                        ? new List<string>() 
                        : financialPeriod.ValidationErrors.Split('\n').ToList()
                ),
                CanRollBack = !financialPeriod.IsClosed && financialPeriod.ClosingStatus != ClosingStatus.NotStarted
            };
        }
    }

    // DTOs
    public class PeriodClosingStatusDto
    {
        public string FinancialPeriodId { get; set; }
        public string FinancialPeriodName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ClosingStatus { get; set; }
        public bool IsClosed { get; set; }
        public DateTime? ClosedAt { get; set; }
        public string ClosedBy { get; set; }
        public List<string> ValidationErrors { get; set; }
        public bool CanRollBack { get; set; }
    }
}

