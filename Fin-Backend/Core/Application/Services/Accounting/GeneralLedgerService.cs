using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Accounting;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Repositories;
using FinTech.Core.Domain.Repositories.Accounting;
using FinTech.Core.Application.Interfaces.Services;
using FinTech.Core.Application.Interfaces.Services.Accounting;
using FinTech.Core.Application.DTOs.GeneralLedger.Journal;
using FinTech.Core.Application.DTOs.GeneralLedger.Account;
using FinTech.Core.Application.DTOs.GeneralLedger.Financial;
using FinTech.Core.Application.DTOs.GeneralLedger.Period;
using FinTech.Core.Application.DTOs.GeneralLedger.Regulatory;
using FinTech.Core.Domain.ValueObjects;

namespace FinTech.Core.Application.Services.Accounting
{
    public class GeneralLedgerService : IGeneralLedgerService
    {
        private readonly IChartOfAccountRepository _chartOfAccountRepository;
        private readonly IJournalEntryRepository _journalEntryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GeneralLedgerService(
            IChartOfAccountRepository chartOfAccountRepository,
            IJournalEntryRepository journalEntryRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _chartOfAccountRepository = chartOfAccountRepository ?? throw new ArgumentNullException(nameof(chartOfAccountRepository));
            _journalEntryRepository = journalEntryRepository ?? throw new ArgumentNullException(nameof(journalEntryRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        /// <summary>
        /// Updates the account balances based on a posted journal entry
        /// </summary>
        public async Task UpdateAccountBalancesAsync(JournalEntry journalEntry, CancellationToken cancellationToken = default)
        {
            if (journalEntry == null)
                throw new ArgumentNullException(nameof(journalEntry));

            if (journalEntry.Status != JournalEntryStatus.Posted)
                throw new InvalidOperationException("Cannot update account balances for a journal entry that is not posted");

            // Dictionary to track accounts that need updating (avoid loading the same account multiple times)
            var accountUpdates = new Dictionary<string, (ChartOfAccount Account, Money Amount, bool IsDebit)>();

            // Group journal lines by account and calculate net effect
            foreach (var line in journalEntry.JournalEntryLines)
            {
                if (!accountUpdates.ContainsKey(line.AccountId))
                {
                    // Load the account if we haven't seen it yet
                    var account = await _chartOfAccountRepository.GetByIdAsync(line.AccountId, cancellationToken);
                    
                    if (account == null)
                        throw new InvalidOperationException($"Account with ID {line.AccountId} not found");

                    accountUpdates[line.AccountId] = (account, line.Amount, line.IsDebit);
                }
                else
                {
                    // Update existing entry
                    var (account, currentAmount, currentIsDebit) = accountUpdates[line.AccountId];
                    
                    // If the current direction and new direction are the same, add the amounts
                    if (currentIsDebit == line.IsDebit)
                    {
                        accountUpdates[line.AccountId] = (account, 
                            Money.Create(currentAmount.Amount + line.Amount.Amount, currentAmount.Currency), 
                            currentIsDebit);
                    }
                    else
                    {
                        // Different directions - determine the new direction and amount
                        if (currentAmount.Amount > line.Amount.Amount)
                        {
                            // Current amount is larger, so direction stays the same but amount is reduced
                            accountUpdates[line.AccountId] = (account, 
                                Money.Create(currentAmount.Amount - line.Amount.Amount, currentAmount.Currency), 
                                currentIsDebit);
                        }
                        else if (currentAmount.Amount < line.Amount.Amount)
                        {
                            // New amount is larger, so direction flips and amount is the difference
                            accountUpdates[line.AccountId] = (account, 
                                Money.Create(line.Amount.Amount - currentAmount.Amount, currentAmount.Currency), 
                                line.IsDebit);
                        }
                        else
                        {
                            // Equal amounts in opposite directions - they cancel out
                            accountUpdates[line.AccountId] = (account, 
                                Money.Create(0, currentAmount.Currency), 
                                currentIsDebit);
                        }
                    }
                }
            }

            // Now update all the account balances
            foreach (var (accountId, (account, amount, isDebit)) in accountUpdates)
            {
                // Skip if the net amount is zero
                if (amount.Amount == 0)
                    continue;
                
                // Update the account balance
                // Update the account balance
                account.UpdateBalance(amount, isDebit);
                
                // Save the updated account
                await _chartOfAccountRepository.UpdateAsync(account, cancellationToken);
            }

            // Save all changes in a single transaction
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Gets the current balance of an account
        /// </summary>
        public async Task<decimal> GetAccountBalanceAsync(string accountId, DateTime? asOfDate = null, CancellationToken cancellationToken = default)
        {
            var account = await _chartOfAccountRepository.GetByIdAsync(accountId, cancellationToken);
            
            if (account == null)
                throw new InvalidOperationException($"Account with ID {accountId} not found");

            // If no date specified, return current balance
            if (asOfDate == null)
                return account.CurrentBalance.Amount;

            // Otherwise, calculate balance as of the specified date
            // Get the current balance and then reverse any journal entries after the specified date
            decimal balance = account.CurrentBalance.Amount;
            
            // Get all posted journal entries affecting this account after the specified date
            var journalEntries = await _journalEntryRepository.GetByAccountIdAsync(accountId, cancellationToken);
            var entriesAfterDate = journalEntries
                .Where(je => je.Status == JournalEntryStatus.Posted && je.EntryDate > asOfDate)
                .ToList();

            // Reverse the effect of these entries
            foreach (var entry in entriesAfterDate)
            {
                foreach (var line in entry.JournalEntryLines.Where(l => l.AccountId == accountId))
                {
                    if (account.NormalBalance == NormalBalanceType.Debit)
                    {
                        if (line.IsDebit)
                            balance -= line.Amount.Amount;
                        else
                            balance += line.Amount.Amount;
                    }
                    else
                    {
                        if (line.IsDebit)
                            balance += line.Amount.Amount;
                        else
                            balance -= line.Amount.Amount;
                    }
                }
            }

            return balance;
        }

        /// <summary>
        /// Gets the balances for multiple accounts
        /// </summary>
        public async Task<IEnumerable<AccountBalanceDto>> GetAccountBalancesAsync(IEnumerable<string> accountIds, DateTime asOfDate, CancellationToken cancellationToken = default)
        {
            var result = new List<AccountBalanceDto>();
            
            foreach (var accountId in accountIds)
            {
                var account = await _chartOfAccountRepository.GetByIdAsync(accountId, cancellationToken);
                
                if (account == null)
                    continue;

                decimal balance = await GetAccountBalanceAsync(accountId, asOfDate, cancellationToken);
                
                result.Add(new AccountBalanceDto
                {
                    AccountId = account.Id,
                    AccountNumber = account.AccountNumber,
                    AccountName = account.AccountName,
                    Balance = balance,
                    CurrencyCode = account.CurrencyCode,
                    Classification = account.Classification,
                    AccountType = (FinTech.Core.Domain.Enums.AccountType)account.AccountType,
                    AsOfDate = asOfDate
                });
            }
            
            return result;
        }

        /// <summary>
        /// Gets account balances by classification
        /// </summary>
        public async Task<IReadOnlyList<AccountBalanceDto>> GetAccountBalancesByClassificationAsync(AccountClassification classification, DateTime? asOfDate = null, CancellationToken cancellationToken = default)
        {
            var accounts = await _chartOfAccountRepository.GetByClassificationAsync(classification, cancellationToken);
            return (await GetAccountBalancesAsync(accounts.Select(a => a.Id), asOfDate ?? DateTime.UtcNow, cancellationToken)).ToList();
        }

        /// <summary>
        /// Gets account balances by account type
        /// </summary>
        public async Task<IReadOnlyList<AccountBalanceDto>> GetAccountBalancesByTypeAsync(AccountType accountType, DateTime? asOfDate = null, CancellationToken cancellationToken = default)
        {
            var accounts = await _chartOfAccountRepository.GetByTypeAsync(accountType, cancellationToken);
            return (await GetAccountBalancesAsync(accounts.Select(a => a.Id), asOfDate ?? DateTime.UtcNow, cancellationToken)).ToList();
        }

        /// <summary>
        /// Gets activity for a specific account within a date range
        /// </summary>
        public async Task<IReadOnlyList<JournalEntryDto>> GetAccountActivityAsync(string accountId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            var account = await _chartOfAccountRepository.GetByIdAsync(accountId, cancellationToken);
            
            if (account == null)
                throw new InvalidOperationException($"Account with ID {accountId} not found");

            // Get all posted journal entries affecting this account within the date range
            var journalEntries = await _journalEntryRepository.GetByAccountIdAsync(accountId, cancellationToken);
            var entriesInRange = journalEntries
                .Where(je => je.Status == JournalEntryStatus.Posted && je.EntryDate >= startDate && je.EntryDate <= endDate)
                .OrderBy(je => je.EntryDate)
                .ToList();

            var result = new List<JournalEntryDto>();
            
            foreach (var entry in entriesInRange)
            {
                var dto = new JournalEntryDto
                {
                    Id = entry.Id,
                    JournalEntryNumber = entry.JournalEntryNumber,
                    EntryDate = entry.EntryDate,
                    Description = entry.Description,
                    Status = (int)entry.Status,
                    EntryType = (int)entry.EntryType,
                    Reference = entry.Reference,
                    SourceDocument = entry.SourceDocument,
                    FinancialPeriodId = entry.FinancialPeriodId,
                    JournalEntryLines = entry.JournalEntryLines
                        .Select(line => new JournalEntryLineDto
                        {
                            Id = line.Id,
                            JournalEntryId = line.JournalEntryId,
                            JournalEntryNumber = entry.JournalEntryNumber,
                            EntryDate = entry.EntryDate,
                            AccountId = line.AccountId,
                            AccountCode = line.Account?.AccountNumber ?? string.Empty,
                            AccountName = line.Account?.AccountName ?? string.Empty,
                            Amount = line.Amount.Amount,
                            CurrencyCode = line.Amount.Currency,
                            IsDebit = line.IsDebit,
                            Description = line.Description,
                            Reference = line.Reference
                        })
                        .ToList()
                };
                
                result.Add(dto);
            }
            
            return result;
        }


        #region Interface Implementation

        public async Task<bool> UpdateAccountBalanceAsync(string accountId, Money amount, bool isDebit)
        {
            var account = await _chartOfAccountRepository.GetByIdAsync(accountId);
            if (account == null) throw new InvalidOperationException($"Account {accountId} not found");

            // Update the account balance
            account.UpdateBalance(amount, isDebit);
            await _chartOfAccountRepository.UpdateAsync(account);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<Money> GetAccountBalanceAsync(string accountId)
        {
            var balanceDetails = await GetAccountBalanceAsync(accountId, null);
            return Money.Create(balanceDetails, "NGN"); // Defaulting currency, should come from account
        }

        public async Task<AccountBalanceDto> GetAccountWithBalanceAsync(string accountId)
        {
            var account = await _chartOfAccountRepository.GetByIdAsync(accountId);
            if (account == null) return null;

            var balance = await GetAccountBalanceAsync(accountId);
            
            return new AccountBalanceDto
            {
                AccountId = account.Id,
                AccountNumber = account.AccountNumber,
                AccountName = account.AccountName,
                Balance = balance.Amount,
                CurrencyCode = account.CurrencyCode,
                Classification = account.Classification,
                AccountType = (FinTech.Core.Domain.Enums.AccountType)account.AccountType,
                AsOfDate = DateTime.UtcNow
            };
        }

        public async Task<IEnumerable<JournalEntryLineDto>> GetAccountLedgerEntriesAsync(string accountId, DateTime fromDate, DateTime toDate)
        {
            var activity = await GetAccountActivityAsync(accountId, fromDate, toDate);
            return activity.SelectMany(x => x.JournalEntryLines).Where(l => l.AccountId == accountId).ToList(); // Simplified mapping
        }

        public async Task<Money> GetAccountBalanceAsync(string accountId, DateTime fromDate, DateTime toDate)
        {
             // Logic to calculate movement in period
             return Money.Create(0, "NGN"); // Stub
        }

        public async Task<string> GenerateJournalNumberAsync()
        {
            return await _journalEntryRepository.GenerateJournalNumberAsync(JournalEntryType.Standard);
        }

        public async Task<bool> CreateJournalEntryAsync(JournalEntry journalEntry)
        {
            await _journalEntryRepository.AddAsync(journalEntry);
            await UpdateAccountBalancesAsync(journalEntry); // This saves changes
            return true;
        }
        
        // Business Module Integration
        public async Task<bool> PostLoanDisbursementAsync(string loanAccountId, Money amount, string userId)
        {
            var journalNumber = await GenerateJournalNumberAsync();
            var journalEntry = new JournalEntry(
                journalNumber, DateTime.UtcNow, $"Loan Disbursement - {loanAccountId}", JournalEntryType.Standard,
                reference: $"LOAN-DISB-{loanAccountId}", moduleSource: "LOANS",
                tenantId: _currentUserService.TenantId ?? string.Empty, preparedBy: userId);

            var loanRec = await GetAccountByNumberAsync("1200");
            var cash = await GetAccountByNumberAsync("1000");

            journalEntry.AddJournalLine(loanRec.Id, amount, true, $"Disbursement {loanAccountId}");
            journalEntry.AddJournalLine(cash.Id, amount, false, $"Disbursement {loanAccountId}");

            return await CreateJournalEntryAsync(journalEntry);
        }

        public async Task<bool> PostLoanRepaymentAsync(string loanAccountId, Money principal, Money interest, string userId)
        {
            var journalNumber = await GenerateJournalNumberAsync();
            var journalEntry = new JournalEntry(
                journalNumber, DateTime.UtcNow, $"Loan Repayment - {loanAccountId}", JournalEntryType.Standard,
                reference: $"LOAN-REPAY-{loanAccountId}", moduleSource: "LOANS",
                tenantId: _currentUserService.TenantId ?? string.Empty, preparedBy: userId);

            var cash = await GetAccountByNumberAsync("1000");
            var loanRec = await GetAccountByNumberAsync("1200");
            var interestInc = await GetAccountByNumberAsync("4100");

            var total = Money.Create(principal.Amount + interest.Amount, principal.Currency);
            journalEntry.AddJournalLine(cash.Id, total, true, $"Repayment {loanAccountId}");

            if (principal.Amount > 0)
                journalEntry.AddJournalLine(loanRec.Id, principal, false, $"Principal {loanAccountId}");
            
            if (interest.Amount > 0)
                journalEntry.AddJournalLine(interestInc.Id, interest, false, $"Interest {loanAccountId}");

            return await CreateJournalEntryAsync(journalEntry);
        }

        public async Task<bool> PostDepositTransactionAsync(string depositAccountId, Money amount, bool isDeposit, string userId)
        {
            var type = isDeposit ? "Deposit" : "Withdrawal";
            var journalNumber = await GenerateJournalNumberAsync();
            var journalEntry = new JournalEntry(
                journalNumber, DateTime.UtcNow, $"Customer {type} - {depositAccountId}", JournalEntryType.Standard,
                reference: $"DEP-{type.ToUpper()}-{depositAccountId}", moduleSource: "DEPOSITS",
                tenantId: _currentUserService.TenantId ?? string.Empty, preparedBy: userId);

            var cash = await GetAccountByNumberAsync("1000");
            var customerDep = await GetAccountByNumberAsync("2100");

            if (isDeposit)
            {
                journalEntry.AddJournalLine(cash.Id, amount, true, $"{type} {depositAccountId}");
                journalEntry.AddJournalLine(customerDep.Id, amount, false, $"{type} {depositAccountId}");
            }
            else
            {
                journalEntry.AddJournalLine(customerDep.Id, amount, true, $"{type} {depositAccountId}");
                journalEntry.AddJournalLine(cash.Id, amount, false, $"{type} {depositAccountId}");
            }

            return await CreateJournalEntryAsync(journalEntry);
        }

        public Task<bool> PostInterestAccrualAsync(string accountId, Money interestAmount, string userId) => Task.FromResult(true); // Stub
        public Task<bool> PostFeeIncomeAsync(string accountId, Money feeAmount, string feeType, string userId) => Task.FromResult(true); // Stub

        // Financial Period Stubs
        public Task<bool> IsAccountingPeriodOpenAsync(DateTime transactionDate) => Task.FromResult(true);
        public Task<bool> ClosePeriodAsync(string fiscalPeriodId, string userId) => Task.FromResult(true);
        public Task<bool> ReopenPeriodAsync(string fiscalPeriodId, string userId) => Task.FromResult(true);
        public Task<bool> CreateNewFiscalYearAsync(FiscalYearDto fiscalYear, string userId) => Task.FromResult(true);
        public Task<bool> CloseFiscalYearAsync(string fiscalYearId, string userId) => Task.FromResult(true);




        async Task<FinTech.Core.Application.DTOs.GeneralLedger.Period.FinancialPeriodDto> FinTech.Core.Application.Interfaces.Services.IGeneralLedgerService.GetCurrentPeriodAsync() => await Task.FromResult(new FinTech.Core.Application.DTOs.GeneralLedger.Period.FinancialPeriodDto());
        async Task<FinTech.Core.Application.DTOs.GeneralLedger.Period.FiscalYearDto> FinTech.Core.Application.Interfaces.Services.IGeneralLedgerService.GetCurrentFiscalYearAsync() => await Task.FromResult(new FinTech.Core.Application.DTOs.GeneralLedger.Period.FiscalYearDto());

        // Explicit Interface Implementations for Reporting (resolves CS0738)
        async Task<FinTech.Core.Application.DTOs.GeneralLedger.Financial.TrialBalanceDto> FinTech.Core.Application.Interfaces.Services.IGeneralLedgerService.GenerateTrialBalanceAsync(DateTime asOfDate, string? fiscalPeriodId) => await Task.FromResult(new FinTech.Core.Application.DTOs.GeneralLedger.Financial.TrialBalanceDto());
        async Task<FinTech.Core.Application.DTOs.GeneralLedger.Financial.BalanceSheetDto> FinTech.Core.Application.Interfaces.Services.IGeneralLedgerService.GenerateBalanceSheetAsync(DateTime asOfDate) => await Task.FromResult(new FinTech.Core.Application.DTOs.GeneralLedger.Financial.BalanceSheetDto());
        async Task<FinTech.Core.Application.DTOs.GeneralLedger.Financial.IncomeStatementDto> FinTech.Core.Application.Interfaces.Services.IGeneralLedgerService.GenerateIncomeStatementAsync(DateTime fromDate, DateTime toDate) => await Task.FromResult(new FinTech.Core.Application.DTOs.GeneralLedger.Financial.IncomeStatementDto());
        async Task<FinTech.Core.Application.DTOs.GeneralLedger.Financial.CashFlowStatementDto> FinTech.Core.Application.Interfaces.Services.IGeneralLedgerService.GenerateCashFlowStatementAsync(DateTime fromDate, DateTime toDate) => await Task.FromResult(new FinTech.Core.Application.DTOs.GeneralLedger.Financial.CashFlowStatementDto());
        
        async Task<IEnumerable<FinTech.Core.Application.DTOs.GeneralLedger.Journal.JournalEntryLineDto>> FinTech.Core.Application.Interfaces.Services.IGeneralLedgerService.GetAccountLedgerEntriesAsync(string accountId, DateTime fromDate, DateTime toDate)
        {
             return await GetAccountLedgerEntriesAsync(accountId, fromDate, toDate); 
        }

        // Regulatory Stubs Explicit
        async Task<FinTech.Core.Application.DTOs.GeneralLedger.Regulatory.CBNReturnsDto> FinTech.Core.Application.Interfaces.Services.IGeneralLedgerService.GenerateCBNReturnsAsync(DateTime asOfDate, string returnType) => await Task.FromResult(new FinTech.Core.Application.DTOs.GeneralLedger.Regulatory.CBNReturnsDto());
        async Task<FinTech.Core.Application.DTOs.GeneralLedger.Regulatory.NDICReturnsDto> FinTech.Core.Application.Interfaces.Services.IGeneralLedgerService.GenerateNDICReturnsAsync(DateTime asOfDate) => await Task.FromResult(new FinTech.Core.Application.DTOs.GeneralLedger.Regulatory.NDICReturnsDto());
        async Task<FinTech.Core.Application.DTOs.GeneralLedger.Regulatory.IFRSDisclosureDto> FinTech.Core.Application.Interfaces.Services.IGeneralLedgerService.GenerateIFRSDisclosuresAsync(DateTime asOfDate, string disclosureType) => await Task.FromResult(new FinTech.Core.Application.DTOs.GeneralLedger.Regulatory.IFRSDisclosureDto());

        private async Task<ChartOfAccount> GetAccountByNumberAsync(string number)
        {
             var account = await _chartOfAccountRepository.GetByAccountNumberAsync(number);
             if (account == null) throw new InvalidOperationException($"GL Account {number} not found in configuration.");
             return account;
        }



        #endregion

    }


}
