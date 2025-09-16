using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.GeneralLedger;
using FinTech.Core.Application.Interfaces.Repositories;
using FinTech.Core.Application.Interfaces.Services;
using FinTech.Domain.Entities.Accounting;
using FinTech.Domain.Entities.Common;
using Microsoft.Extensions.Logging;

namespace FinTech.Core.Application.Services
{
    public class GeneralLedgerService : IGeneralLedgerService
    {
        private readonly IGeneralLedgerRepository _repository;
        private readonly ILogger<GeneralLedgerService> _logger;

        public GeneralLedgerService(
            IGeneralLedgerRepository repository,
            ILogger<GeneralLedgerService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        #region Journal Entry Management

        public async Task<string> CreateJournalEntryAsync(JournalEntryDto journalEntryDto, string userId)
        {
            try
            {
                // Validate the journal entry
                if (!await ValidateJournalEntryAsync(journalEntryDto))
                {
                    throw new InvalidOperationException("Journal entry validation failed");
                }

                // Check if the accounting period is open
                if (!await IsAccountingPeriodOpenAsync(journalEntryDto.EntryDate))
                {
                    throw new InvalidOperationException($"The accounting period for date {journalEntryDto.EntryDate.ToShortDateString()} is closed");
                }

                // Generate journal entry number
                string journalEntryNumber = await _repository.GenerateJournalEntryNumberAsync();

                // Map from DTO to domain entity
                var journalEntry = new JournalEntry(
                    journalEntryNumber,
                    journalEntryDto.EntryDate,
                    journalEntryDto.Description,
                    (JournalEntryType)journalEntryDto.EntryType,
                    journalEntryDto.Reference,
                    journalEntryDto.SourceDocument,
                    journalEntryDto.FinancialPeriodId,
                    journalEntryDto.ModuleSource,
                    journalEntryDto.IsRecurring,
                    journalEntryDto.RecurrencePattern,
                    journalEntryDto.Notes);

                // Add journal entry lines
                foreach (var lineDto in journalEntryDto.JournalEntryLines)
                {
                    var amount = new Money(lineDto.Amount, lineDto.CurrencyCode);
                    journalEntry.AddJournalLine(
                        lineDto.AccountId,
                        amount,
                        lineDto.IsDebit,
                        lineDto.Description,
                        lineDto.Reference);
                }

                // Create the journal entry in the repository
                var createdJournalEntry = await _repository.CreateJournalEntryAsync(journalEntry);

                _logger.LogInformation($"Journal entry {journalEntryNumber} created by {userId}");

                return createdJournalEntry.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating journal entry: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> SubmitJournalEntryForApprovalAsync(string journalEntryId, string userId)
        {
            try
            {
                var journalEntry = await _repository.GetJournalEntryByIdAsync(journalEntryId);
                if (journalEntry == null)
                {
                    throw new InvalidOperationException($"Journal entry with ID {journalEntryId} not found");
                }

                journalEntry.SubmitForApproval();
                bool result = await _repository.UpdateJournalEntryAsync(journalEntry);

                _logger.LogInformation($"Journal entry {journalEntry.JournalEntryNumber} submitted for approval by {userId}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error submitting journal entry for approval: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> ApproveJournalEntryAsync(string journalEntryId, string userId)
        {
            try
            {
                var journalEntry = await _repository.GetJournalEntryByIdAsync(journalEntryId);
                if (journalEntry == null)
                {
                    throw new InvalidOperationException($"Journal entry with ID {journalEntryId} not found");
                }

                journalEntry.Approve(userId);
                bool result = await _repository.UpdateJournalEntryAsync(journalEntry);

                _logger.LogInformation($"Journal entry {journalEntry.JournalEntryNumber} approved by {userId}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error approving journal entry: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> RejectJournalEntryAsync(string journalEntryId, string userId, string reason)
        {
            try
            {
                var journalEntry = await _repository.GetJournalEntryByIdAsync(journalEntryId);
                if (journalEntry == null)
                {
                    throw new InvalidOperationException($"Journal entry with ID {journalEntryId} not found");
                }

                journalEntry.Reject(userId, reason);
                bool result = await _repository.UpdateJournalEntryAsync(journalEntry);

                _logger.LogInformation($"Journal entry {journalEntry.JournalEntryNumber} rejected by {userId}. Reason: {reason}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error rejecting journal entry: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> PostJournalEntryAsync(string journalEntryId, string userId)
        {
            try
            {
                var journalEntry = await _repository.GetJournalEntryByIdAsync(journalEntryId);
                if (journalEntry == null)
                {
                    throw new InvalidOperationException($"Journal entry with ID {journalEntryId} not found");
                }

                // Check if the accounting period is open
                if (!await IsAccountingPeriodOpenAsync(journalEntry.EntryDate))
                {
                    throw new InvalidOperationException($"The accounting period for date {journalEntry.EntryDate.ToShortDateString()} is closed");
                }

                journalEntry.Post(userId);
                bool result = await _repository.UpdateJournalEntryAsync(journalEntry);

                // Update account balances
                foreach (var line in journalEntry.JournalEntryLines)
                {
                    await _repository.UpdateAccountBalanceAsync(line.AccountId, line.Amount, line.IsDebit);
                }

                _logger.LogInformation($"Journal entry {journalEntry.JournalEntryNumber} posted by {userId}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error posting journal entry: {ex.Message}");
                throw;
            }
        }

        public async Task<string> ReverseJournalEntryAsync(string journalEntryId, string userId, string reason)
        {
            try
            {
                var journalEntry = await _repository.GetJournalEntryByIdAsync(journalEntryId);
                if (journalEntry == null)
                {
                    throw new InvalidOperationException($"Journal entry with ID {journalEntryId} not found");
                }

                // Check if the accounting period is open
                if (!await IsAccountingPeriodOpenAsync(DateTime.UtcNow))
                {
                    throw new InvalidOperationException("The current accounting period is closed");
                }

                // Generate reversal journal entry number
                string reversalJournalNumber = await _repository.GenerateJournalEntryNumberAsync();

                var reversalEntry = journalEntry.CreateReversal(reversalJournalNumber, reason, userId);
                await _repository.CreateJournalEntryAsync(reversalEntry);

                // Update account balances
                foreach (var line in reversalEntry.JournalEntryLines)
                {
                    await _repository.UpdateAccountBalanceAsync(line.AccountId, line.Amount, line.IsDebit);
                }

                _logger.LogInformation($"Journal entry {journalEntry.JournalEntryNumber} reversed by {userId}. Reversal entry: {reversalJournalNumber}");

                return reversalEntry.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error reversing journal entry: {ex.Message}");
                throw;
            }
        }

        public async Task<JournalEntryDto> GetJournalEntryAsync(string journalEntryId)
        {
            try
            {
                var journalEntry = await _repository.GetJournalEntryByIdAsync(journalEntryId);
                if (journalEntry == null)
                {
                    return null;
                }

                return MapJournalEntryToDto(journalEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving journal entry: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<JournalEntryDto>> GetJournalEntriesByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var journalEntries = await _repository.GetJournalEntriesByDateRangeAsync(fromDate, toDate);
                return journalEntries.Select(MapJournalEntryToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving journal entries by date range: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<JournalEntryDto>> GetPendingJournalEntriesAsync()
        {
            try
            {
                var journalEntries = await _repository.GetPendingJournalEntriesAsync();
                return journalEntries.Select(MapJournalEntryToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving pending journal entries: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> ValidateJournalEntryAsync(JournalEntryDto journalEntry)
        {
            // Check if the journal entry has at least two lines
            if (journalEntry.JournalEntryLines == null || journalEntry.JournalEntryLines.Count() < 2)
            {
                return false;
            }

            // Group by currency and check balance for each currency
            var currencyGroups = journalEntry.JournalEntryLines
                .GroupBy(l => l.CurrencyCode)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var currencyGroup in currencyGroups)
            {
                var lines = currencyGroup.Value;

                var totalDebits = lines
                    .Where(l => l.IsDebit)
                    .Sum(l => l.Amount);

                var totalCredits = lines
                    .Where(l => !l.IsDebit)
                    .Sum(l => l.Amount);

                // Journal should be balanced: Total Debits = Total Credits
                if (Math.Abs(totalDebits - totalCredits) > 0.01m) // Allow small rounding differences
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Account Management

        public async Task<bool> UpdateAccountBalanceAsync(string accountId, Money amount, bool isDebit)
        {
            try
            {
                return await _repository.UpdateAccountBalanceAsync(accountId, amount, isDebit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating account balance: {ex.Message}");
                throw;
            }
        }

        public async Task<Money> GetAccountBalanceAsync(string accountId)
        {
            try
            {
                var account = await _repository.GetAccountByIdAsync(accountId);
                if (account == null)
                {
                    throw new InvalidOperationException($"Account with ID {accountId} not found");
                }

                return account.CurrentBalance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving account balance: {ex.Message}");
                throw;
            }
        }

        public async Task<AccountBalanceDto> GetAccountWithBalanceAsync(string accountId)
        {
            try
            {
                var account = await _repository.GetAccountByIdAsync(accountId);
                if (account == null)
                {
                    return null;
                }

                return new AccountBalanceDto
                {
                    AccountId = account.Id,
                    AccountCode = account.AccountCode,
                    AccountName = account.AccountName,
                    Classification = account.Classification,
                    AccountType = account.AccountType,
                    Balance = account.CurrentBalance.Amount,
                    CurrencyCode = account.CurrentBalance.Currency,
                    CBNReportingCode = account.CBNReportingCode,
                    NDICReportingCode = account.NDICReportingCode,
                    IFRSCategory = account.IFRSCategory
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving account with balance: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<AccountBalanceDto>> GetAccountBalancesAsync(IEnumerable<string> accountIds = null)
        {
            try
            {
                IEnumerable<ChartOfAccount> accounts;
                if (accountIds != null && accountIds.Any())
                {
                    accounts = await _repository.GetAccountsByIdsAsync(accountIds);
                }
                else
                {
                    accounts = await _repository.GetAllActiveAccountsAsync();
                }

                return accounts.Select(a => new AccountBalanceDto
                {
                    AccountId = a.Id,
                    AccountCode = a.AccountCode,
                    AccountName = a.AccountName,
                    Classification = a.Classification,
                    AccountType = a.AccountType,
                    Balance = a.CurrentBalance.Amount,
                    CurrencyCode = a.CurrentBalance.Currency,
                    CBNReportingCode = a.CBNReportingCode,
                    NDICReportingCode = a.NDICReportingCode,
                    IFRSCategory = a.IFRSCategory
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving account balances: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<JournalEntryLineDto>> GetAccountLedgerEntriesAsync(string accountId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var account = await _repository.GetAccountByIdAsync(accountId);
                if (account == null)
                {
                    throw new InvalidOperationException($"Account with ID {accountId} not found");
                }

                var journalLines = await _repository.GetJournalLinesForAccountAsync(accountId, fromDate, toDate);
                return journalLines.Select(MapJournalLineToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving account ledger entries: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Financial Period Management

        public async Task<bool> IsAccountingPeriodOpenAsync(DateTime transactionDate)
        {
            try
            {
                var period = await _repository.GetFinancialPeriodByDateAsync(transactionDate);
                if (period == null)
                {
                    throw new InvalidOperationException($"No accounting period found for date {transactionDate.ToShortDateString()}");
                }

                return !period.IsClosed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if accounting period is open: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> ClosePeriodAsync(string fiscalPeriodId, string userId)
        {
            try
            {
                return await _repository.CloseFinancialPeriodAsync(fiscalPeriodId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error closing accounting period: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> ReopenPeriodAsync(string fiscalPeriodId, string userId)
        {
            try
            {
                return await _repository.ReopenFinancialPeriodAsync(fiscalPeriodId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error reopening accounting period: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> CreateNewFiscalYearAsync(FiscalYearDto fiscalYearDto, string userId)
        {
            try
            {
                // Map from DTO to domain entity
                var fiscalYear = new FiscalYear(
                    fiscalYearDto.Year,
                    fiscalYearDto.StartDate,
                    fiscalYearDto.EndDate);

                // Create fiscal year periods based on the start and end dates
                CreateFiscalPeriods(fiscalYear);

                await _repository.CreateFiscalYearAsync(fiscalYear);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating new fiscal year: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> CloseFiscalYearAsync(string fiscalYearId, string userId)
        {
            try
            {
                return await _repository.CloseFiscalYearAsync(fiscalYearId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error closing fiscal year: {ex.Message}");
                throw;
            }
        }

        public async Task<FinancialPeriodDto> GetCurrentPeriodAsync()
        {
            try
            {
                var period = await _repository.GetCurrentFinancialPeriodAsync();
                if (period == null)
                {
                    return null;
                }

                return new FinancialPeriodDto
                {
                    Id = period.Id,
                    PeriodCode = period.PeriodCode,
                    PeriodName = period.PeriodName,
                    StartDate = period.StartDate,
                    EndDate = period.EndDate,
                    IsClosed = period.IsClosed,
                    ClosedDate = period.ClosedDate,
                    ClosedBy = period.ClosedBy,
                    FiscalYear = period.FiscalYear,
                    FiscalMonth = period.FiscalMonth,
                    IsAdjustmentPeriod = period.IsAdjustmentPeriod
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving current period: {ex.Message}");
                throw;
            }
        }

        public async Task<FiscalYearDto> GetCurrentFiscalYearAsync()
        {
            try
            {
                var fiscalYear = await _repository.GetCurrentFiscalYearAsync();
                if (fiscalYear == null)
                {
                    return null;
                }

                return MapFiscalYearToDto(fiscalYear);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving current fiscal year: {ex.Message}");
                throw;
            }
        }

        private void CreateFiscalPeriods(FiscalYear fiscalYear)
        {
            DateTime periodStart = fiscalYear.StartDate;
            int monthCounter = 1;

            while (periodStart < fiscalYear.EndDate)
            {
                DateTime periodEnd = new DateTime(
                    periodStart.Year,
                    periodStart.Month,
                    DateTime.DaysInMonth(periodStart.Year, periodStart.Month),
                    23, 59, 59);

                if (periodEnd > fiscalYear.EndDate)
                {
                    periodEnd = fiscalYear.EndDate;
                }

                string periodCode = $"{fiscalYear.Year}-{monthCounter:D2}";
                string periodName = periodStart.ToString("MMMM yyyy");

                fiscalYear.AddFinancialPeriod(
                    periodCode,
                    periodName,
                    periodStart,
                    periodEnd,
                    fiscalYear.Year,
                    monthCounter,
                    false);

                periodStart = periodEnd.AddDays(1);
                monthCounter++;
            }

            // Add adjustment period if needed
            DateTime adjustmentPeriodStart = fiscalYear.EndDate.AddDays(1);
            DateTime adjustmentPeriodEnd = adjustmentPeriodStart.AddDays(30);
            string adjustmentPeriodCode = $"{fiscalYear.Year}-ADJ";
            string adjustmentPeriodName = $"Year-End Adjustments {fiscalYear.Year}";

            fiscalYear.AddFinancialPeriod(
                adjustmentPeriodCode,
                adjustmentPeriodName,
                adjustmentPeriodStart,
                adjustmentPeriodEnd,
                fiscalYear.Year,
                13,
                true);
        }

        #endregion

        #region Financial Reporting

        public async Task<TrialBalanceDto> GenerateTrialBalanceAsync(DateTime asOfDate, string fiscalPeriodId = null)
        {
            // Placeholder implementation - will implement in future
            throw new NotImplementedException("Trial balance generation not yet implemented");
        }

        public async Task<BalanceSheetDto> GenerateBalanceSheetAsync(DateTime asOfDate)
        {
            // Placeholder implementation - will implement in future
            throw new NotImplementedException("Balance sheet generation not yet implemented");
        }

        public async Task<IncomeStatementDto> GenerateIncomeStatementAsync(DateTime fromDate, DateTime toDate)
        {
            // Placeholder implementation - will implement in future
            throw new NotImplementedException("Income statement generation not yet implemented");
        }

        public async Task<CashFlowStatementDto> GenerateCashFlowStatementAsync(DateTime fromDate, DateTime toDate)
        {
            // Placeholder implementation - will implement in future
            throw new NotImplementedException("Cash flow statement generation not yet implemented");
        }

        #endregion

        #region Regulatory Reporting

        public async Task<CBNReturnsDto> GenerateCBNReturnsAsync(DateTime asOfDate, string returnType)
        {
            // Placeholder implementation - will implement in future
            throw new NotImplementedException("CBN returns generation not yet implemented");
        }

        public async Task<NDICReturnsDto> GenerateNDICReturnsAsync(DateTime asOfDate)
        {
            // Placeholder implementation - will implement in future
            throw new NotImplementedException("NDIC returns generation not yet implemented");
        }

        public async Task<IFRSDisclosureDto> GenerateIFRSDisclosuresAsync(DateTime asOfDate, string disclosureType)
        {
            // Placeholder implementation - will implement in future
            throw new NotImplementedException("IFRS disclosures generation not yet implemented");
        }

        #endregion

        #region Business Module Integration

        public async Task<bool> PostLoanDisbursementAsync(string loanAccountId, Money amount, string userId)
        {
            try
            {
                // Get the current period
                var currentPeriod = await GetCurrentPeriodAsync();
                if (currentPeriod == null || currentPeriod.IsClosed)
                {
                    throw new InvalidOperationException("Cannot post transactions in a closed or non-existent period");
                }

                // Generate journal entry number
                string journalEntryNumber = await _repository.GenerateJournalEntryNumberAsync();

                // Create journal entry
                var journalEntry = new JournalEntry(
                    journalEntryNumber,
                    DateTime.UtcNow,
                    $"Loan Disbursement - Loan Account {loanAccountId}",
                    JournalEntryType.SystemGenerated,
                    loanAccountId,
                    null,
                    currentPeriod.Id,
                    "LoanModule");

                // Determine accounts to use
                // In a real implementation, these would be fetched from account mapping configuration
                string loanReceivableAccountId = "LOAN_RECEIVABLE_ACCOUNT_ID"; // This would be fetched based on loan type
                string cashAccountId = "CASH_ACCOUNT_ID"; // This would be determined by disbursement method

                // Add journal lines - Debit: Loan Receivable, Credit: Cash
                journalEntry.AddJournalLine(
                    loanReceivableAccountId,
                    amount,
                    true, // isDebit = true
                    $"Loan Disbursement - Loan Account {loanAccountId}",
                    loanAccountId);

                journalEntry.AddJournalLine(
                    cashAccountId,
                    amount,
                    false, // isDebit = false
                    $"Loan Disbursement - Loan Account {loanAccountId}",
                    loanAccountId);

                // Auto-approve and post the journal entry
                journalEntry.Approve(userId);
                journalEntry.Post(userId);

                // Save the journal entry
                await _repository.CreateJournalEntryAsync(journalEntry);

                // Update account balances
                foreach (var line in journalEntry.JournalEntryLines)
                {
                    await _repository.UpdateAccountBalanceAsync(line.AccountId, line.Amount, line.IsDebit);
                }

                _logger.LogInformation($"Loan disbursement posted for loan account {loanAccountId} by {userId}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error posting loan disbursement: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> PostLoanRepaymentAsync(string loanAccountId, Money principal, Money interest, string userId)
        {
            try
            {
                // Get the current period
                var currentPeriod = await GetCurrentPeriodAsync();
                if (currentPeriod == null || currentPeriod.IsClosed)
                {
                    throw new InvalidOperationException("Cannot post transactions in a closed or non-existent period");
                }

                // Generate journal entry number
                string journalEntryNumber = await _repository.GenerateJournalEntryNumberAsync();

                // Create journal entry
                var journalEntry = new JournalEntry(
                    journalEntryNumber,
                    DateTime.UtcNow,
                    $"Loan Repayment - Loan Account {loanAccountId}",
                    JournalEntryType.SystemGenerated,
                    loanAccountId,
                    null,
                    currentPeriod.Id,
                    "LoanModule");

                // Determine accounts to use
                // In a real implementation, these would be fetched from account mapping configuration
                string loanReceivableAccountId = "LOAN_RECEIVABLE_ACCOUNT_ID"; // This would be fetched based on loan type
                string interestIncomeAccountId = "INTEREST_INCOME_ACCOUNT_ID"; // This would be determined by loan type
                string cashAccountId = "CASH_ACCOUNT_ID"; // This would be determined by repayment method

                // Calculate total amount
                Money totalAmount = principal.Add(interest);

                // Add journal lines
                // Debit: Cash (for the total amount)
                journalEntry.AddJournalLine(
                    cashAccountId,
                    totalAmount,
                    true, // isDebit = true
                    $"Loan Repayment - Loan Account {loanAccountId}",
                    loanAccountId);

                // Credit: Loan Receivable (for the principal amount)
                journalEntry.AddJournalLine(
                    loanReceivableAccountId,
                    principal,
                    false, // isDebit = false
                    $"Loan Principal Repayment - Loan Account {loanAccountId}",
                    loanAccountId);

                // Credit: Interest Income (for the interest amount)
                journalEntry.AddJournalLine(
                    interestIncomeAccountId,
                    interest,
                    false, // isDebit = false
                    $"Loan Interest Income - Loan Account {loanAccountId}",
                    loanAccountId);

                // Auto-approve and post the journal entry
                journalEntry.Approve(userId);
                journalEntry.Post(userId);

                // Save the journal entry
                await _repository.CreateJournalEntryAsync(journalEntry);

                // Update account balances
                foreach (var line in journalEntry.JournalEntryLines)
                {
                    await _repository.UpdateAccountBalanceAsync(line.AccountId, line.Amount, line.IsDebit);
                }

                _logger.LogInformation($"Loan repayment posted for loan account {loanAccountId} by {userId}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error posting loan repayment: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> PostDepositTransactionAsync(string depositAccountId, Money amount, bool isDeposit, string userId)
        {
            try
            {
                // Get the current period
                var currentPeriod = await GetCurrentPeriodAsync();
                if (currentPeriod == null || currentPeriod.IsClosed)
                {
                    throw new InvalidOperationException("Cannot post transactions in a closed or non-existent period");
                }

                string transactionType = isDeposit ? "Deposit" : "Withdrawal";

                // Generate journal entry number
                string journalEntryNumber = await _repository.GenerateJournalEntryNumberAsync();

                // Create journal entry
                var journalEntry = new JournalEntry(
                    journalEntryNumber,
                    DateTime.UtcNow,
                    $"Customer {transactionType} - Account {depositAccountId}",
                    JournalEntryType.SystemGenerated,
                    depositAccountId,
                    null,
                    currentPeriod.Id,
                    "DepositModule");

                // Determine accounts to use
                // In a real implementation, these would be fetched from account mapping configuration
                string customerDepositAccountId = "CUSTOMER_DEPOSIT_ACCOUNT_ID"; // This would be fetched based on deposit account type
                string cashAccountId = "CASH_ACCOUNT_ID"; // This would be determined by transaction method

                if (isDeposit)
                {
                    // Debit: Cash, Credit: Customer Deposits
                    journalEntry.AddJournalLine(
                        cashAccountId,
                        amount,
                        true, // isDebit = true
                        $"Customer Deposit - Account {depositAccountId}",
                        depositAccountId);

                    journalEntry.AddJournalLine(
                        customerDepositAccountId,
                        amount,
                        false, // isDebit = false
                        $"Customer Deposit - Account {depositAccountId}",
                        depositAccountId);
                }
                else
                {
                    // Debit: Customer Deposits, Credit: Cash
                    journalEntry.AddJournalLine(
                        customerDepositAccountId,
                        amount,
                        true, // isDebit = true
                        $"Customer Withdrawal - Account {depositAccountId}",
                        depositAccountId);

                    journalEntry.AddJournalLine(
                        cashAccountId,
                        amount,
                        false, // isDebit = false
                        $"Customer Withdrawal - Account {depositAccountId}",
                        depositAccountId);
                }

                // Auto-approve and post the journal entry
                journalEntry.Approve(userId);
                journalEntry.Post(userId);

                // Save the journal entry
                await _repository.CreateJournalEntryAsync(journalEntry);

                // Update account balances
                foreach (var line in journalEntry.JournalEntryLines)
                {
                    await _repository.UpdateAccountBalanceAsync(line.AccountId, line.Amount, line.IsDebit);
                }

                _logger.LogInformation($"Deposit transaction ({transactionType}) posted for account {depositAccountId} by {userId}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error posting deposit transaction: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> PostInterestAccrualAsync(string accountId, Money interestAmount, string userId)
        {
            try
            {
                // Get the current period
                var currentPeriod = await GetCurrentPeriodAsync();
                if (currentPeriod == null || currentPeriod.IsClosed)
                {
                    throw new InvalidOperationException("Cannot post transactions in a closed or non-existent period");
                }

                // Generate journal entry number
                string journalEntryNumber = await _repository.GenerateJournalEntryNumberAsync();

                // Create journal entry
                var journalEntry = new JournalEntry(
                    journalEntryNumber,
                    DateTime.UtcNow,
                    $"Interest Accrual - Account {accountId}",
                    JournalEntryType.SystemGenerated,
                    accountId,
                    null,
                    currentPeriod.Id,
                    "InterestAccrualModule");

                // Determine accounts to use
                // In a real implementation, these would be fetched from account mapping configuration
                string interestReceivableAccountId = "INTEREST_RECEIVABLE_ACCOUNT_ID";
                string interestIncomeAccountId = "INTEREST_INCOME_ACCOUNT_ID";

                // Add journal lines - Debit: Interest Receivable, Credit: Interest Income
                journalEntry.AddJournalLine(
                    interestReceivableAccountId,
                    interestAmount,
                    true, // isDebit = true
                    $"Interest Accrual - Account {accountId}",
                    accountId);

                journalEntry.AddJournalLine(
                    interestIncomeAccountId,
                    interestAmount,
                    false, // isDebit = false
                    $"Interest Accrual - Account {accountId}",
                    accountId);

                // Auto-approve and post the journal entry
                journalEntry.Approve(userId);
                journalEntry.Post(userId);

                // Save the journal entry
                await _repository.CreateJournalEntryAsync(journalEntry);

                // Update account balances
                foreach (var line in journalEntry.JournalEntryLines)
                {
                    await _repository.UpdateAccountBalanceAsync(line.AccountId, line.Amount, line.IsDebit);
                }

                _logger.LogInformation($"Interest accrual posted for account {accountId} by {userId}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error posting interest accrual: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> PostFeeIncomeAsync(string accountId, Money feeAmount, string feeType, string userId)
        {
            try
            {
                // Get the current period
                var currentPeriod = await GetCurrentPeriodAsync();
                if (currentPeriod == null || currentPeriod.IsClosed)
                {
                    throw new InvalidOperationException("Cannot post transactions in a closed or non-existent period");
                }

                // Generate journal entry number
                string journalEntryNumber = await _repository.GenerateJournalEntryNumberAsync();

                // Create journal entry
                var journalEntry = new JournalEntry(
                    journalEntryNumber,
                    DateTime.UtcNow,
                    $"{feeType} Fee Income - Account {accountId}",
                    JournalEntryType.SystemGenerated,
                    accountId,
                    null,
                    currentPeriod.Id,
                    "FeeModule");

                // Determine accounts to use
                // In a real implementation, these would be fetched from account mapping configuration
                string feeReceivableAccountId = "FEE_RECEIVABLE_ACCOUNT_ID";
                string feeIncomeAccountId = GetFeeIncomeAccountIdByFeeType(feeType); // This would determine the fee income account based on the fee type

                // Add journal lines - Debit: Fee Receivable, Credit: Fee Income
                journalEntry.AddJournalLine(
                    feeReceivableAccountId,
                    feeAmount,
                    true, // isDebit = true
                    $"{feeType} Fee Income - Account {accountId}",
                    accountId);

                journalEntry.AddJournalLine(
                    feeIncomeAccountId,
                    feeAmount,
                    false, // isDebit = false
                    $"{feeType} Fee Income - Account {accountId}",
                    accountId);

                // Auto-approve and post the journal entry
                journalEntry.Approve(userId);
                journalEntry.Post(userId);

                // Save the journal entry
                await _repository.CreateJournalEntryAsync(journalEntry);

                // Update account balances
                foreach (var line in journalEntry.JournalEntryLines)
                {
                    await _repository.UpdateAccountBalanceAsync(line.AccountId, line.Amount, line.IsDebit);
                }

                _logger.LogInformation($"{feeType} fee income posted for account {accountId} by {userId}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error posting fee income: {ex.Message}");
                throw;
            }
        }

        private string GetFeeIncomeAccountIdByFeeType(string feeType)
        {
            // In a real implementation, this would fetch the appropriate fee income account based on the fee type
            // from a configuration or mapping table
            return "DEFAULT_FEE_INCOME_ACCOUNT_ID";
        }

        #endregion

        #region Helper Methods

        private JournalEntryDto MapJournalEntryToDto(JournalEntry journalEntry)
        {
            return new JournalEntryDto
            {
                Id = journalEntry.Id,
                JournalEntryNumber = journalEntry.JournalEntryNumber,
                EntryDate = journalEntry.EntryDate,
                Description = journalEntry.Description,
                Status = (int)journalEntry.Status,
                EntryType = (int)journalEntry.EntryType,
                Reference = journalEntry.Reference,
                SourceDocument = journalEntry.SourceDocument,
                ApprovedBy = journalEntry.ApprovedBy,
                ApprovalDate = journalEntry.ApprovalDate,
                PostedBy = journalEntry.PostedBy,
                PostedDate = journalEntry.PostedDate,
                FinancialPeriodId = journalEntry.FinancialPeriodId,
                ModuleSource = journalEntry.ModuleSource,
                IsRecurring = journalEntry.IsRecurring,
                RecurrencePattern = journalEntry.RecurrencePattern,
                Notes = journalEntry.Notes,
                JournalEntryLines = journalEntry.JournalEntryLines.Select(MapJournalLineToDto).ToList()
            };
        }

        private JournalEntryLineDto MapJournalLineToDto(JournalEntryLine line)
        {
            return new JournalEntryLineDto
            {
                Id = line.Id,
                JournalEntryId = line.JournalEntryId,
                AccountId = line.AccountId,
                AccountCode = line.Account?.AccountCode,
                AccountName = line.Account?.AccountName,
                Amount = line.Amount.Amount,
                CurrencyCode = line.Amount.Currency,
                IsDebit = line.IsDebit,
                Description = line.Description,
                Reference = line.Reference
            };
        }

        private FiscalYearDto MapFiscalYearToDto(FiscalYear fiscalYear)
        {
            return new FiscalYearDto
            {
                Id = fiscalYear.Id,
                Year = fiscalYear.Year,
                StartDate = fiscalYear.StartDate,
                EndDate = fiscalYear.EndDate,
                IsClosed = fiscalYear.IsClosed,
                ClosedDate = fiscalYear.ClosedDate,
                ClosedBy = fiscalYear.ClosedBy,
                IsCurrentYear = true, // This would be determined based on the current date
                Periods = fiscalYear.Periods.Select(p => new FinancialPeriodDto
                {
                    Id = p.Id,
                    PeriodCode = p.PeriodCode,
                    PeriodName = p.PeriodName,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    IsClosed = p.IsClosed,
                    ClosedDate = p.ClosedDate,
                    ClosedBy = p.ClosedBy,
                    FiscalYear = p.FiscalYear,
                    FiscalMonth = p.FiscalMonth,
                    IsAdjustmentPeriod = p.IsAdjustmentPeriod
                }).ToList()
            };
        }

        #endregion
    }
}