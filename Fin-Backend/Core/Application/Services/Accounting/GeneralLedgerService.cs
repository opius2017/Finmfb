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

namespace FinTech.Core.Application.Services.Accounting
{
    public class GeneralLedgerService
    {
        private readonly IChartOfAccountRepository _chartOfAccountRepository;
        private readonly IJournalEntryRepository _journalEntryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public GeneralLedgerService(
            IChartOfAccountRepository chartOfAccountRepository,
            IJournalEntryRepository journalEntryRepository,
            IUnitOfWork unitOfWork)
        {
            _chartOfAccountRepository = chartOfAccountRepository ?? throw new ArgumentNullException(nameof(chartOfAccountRepository));
            _journalEntryRepository = journalEntryRepository ?? throw new ArgumentNullException(nameof(journalEntryRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
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
        public async Task<IReadOnlyList<AccountBalanceDto>> GetAccountBalancesAsync(IEnumerable<string> accountIds, DateTime? asOfDate = null, CancellationToken cancellationToken = default)
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
                    Classification = account.Classification.ToString(),
                    AccountType = account.AccountType.ToString(),
                    AsOfDate = asOfDate ?? DateTime.UtcNow
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
            return await GetAccountBalancesAsync(accounts.Select(a => a.Id), asOfDate, cancellationToken);
        }

        /// <summary>
        /// Gets account balances by account type
        /// </summary>
        public async Task<IReadOnlyList<AccountBalanceDto>> GetAccountBalancesByTypeAsync(AccountType accountType, DateTime? asOfDate = null, CancellationToken cancellationToken = default)
        {
            var accounts = await _chartOfAccountRepository.GetByTypeAsync(accountType, cancellationToken);
            return await GetAccountBalancesAsync(accounts.Select(a => a.Id), asOfDate, cancellationToken);
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
                    Status = entry.Status.ToString(),
                    EntryType = entry.EntryType.ToString(),
                    Reference = entry.Reference,
                    SourceDocument = entry.SourceDocument,
                    FinancialPeriodId = entry.FinancialPeriodId,
                    Lines = entry.JournalEntryLines
                        .Select(line => new JournalEntryLineDto
                        {
                            Id = line.Id,
                            AccountId = line.AccountId,
                            AccountNumber = line.Account?.AccountNumber ?? string.Empty,
                            AccountName = line.Account?.AccountName ?? string.Empty,
                            Description = line.Description,
                            DebitAmount = line.IsDebit ? line.Amount.Amount : 0,
                            CreditAmount = !line.IsDebit ? line.Amount.Amount : 0,
                            CurrencyCode = line.Amount.Currency
                        })
                        .ToList()
                };
                
                result.Add(dto);
            }
            
            return result;
        }
    }

    // DTOs
    public class AccountBalanceDto
    {
        public string AccountId { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string Classification { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public DateTime AsOfDate { get; set; }
    }

    public class JournalEntryDto
    {
        public string Id { get; set; } = string.Empty;
        public string JournalEntryNumber { get; set; } = string.Empty;
        public DateTime EntryDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string EntryType { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string SourceDocument { get; set; } = string.Empty;
        public string FinancialPeriodId { get; set; } = string.Empty;
        public List<JournalEntryLineDto> Lines { get; set; } = new List<JournalEntryLineDto>();
    }

    public class JournalEntryLineDto
    {
        public string Id { get; set; } = string.Empty;
        public string AccountId { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
    }
}
