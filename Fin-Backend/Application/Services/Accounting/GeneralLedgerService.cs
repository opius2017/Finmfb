using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Domain.Entities.Accounting;
using FinTech.Domain.Repositories;
using FinTech.Domain.Repositories.Accounting;

namespace FinTech.Application.Services.Accounting
{
    public interface IGeneralLedgerService
    {
        Task UpdateAccountBalancesAsync(JournalEntry journalEntry, CancellationToken cancellationToken = default);
        Task<decimal> GetAccountBalanceAsync(string accountId, DateTime? asOfDate = null, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AccountBalanceDto>> GetAccountBalancesAsync(IEnumerable<string> accountIds, DateTime? asOfDate = null, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AccountBalanceDto>> GetAccountBalancesByClassificationAsync(AccountClassification classification, DateTime? asOfDate = null, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AccountBalanceDto>> GetAccountBalancesByTypeAsync(AccountType accountType, DateTime? asOfDate = null, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<JournalEntryDto>> GetAccountActivityAsync(string accountId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    }

    public class GeneralLedgerService : IGeneralLedgerService
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
                            new Money(currentAmount.Amount + line.Amount.Amount, currentAmount.CurrencyCode), 
                            currentIsDebit);
                    }
                    else
                    {
                        // Different directions - determine the new direction and amount
                        if (currentAmount.Amount > line.Amount.Amount)
                        {
                            // Current amount is larger, so direction stays the same but amount is reduced
                            accountUpdates[line.AccountId] = (account, 
                                new Money(currentAmount.Amount - line.Amount.Amount, currentAmount.CurrencyCode), 
                                currentIsDebit);
                        }
                        else if (currentAmount.Amount < line.Amount.Amount)
                        {
                            // New amount is larger, so direction flips and amount is the difference
                            accountUpdates[line.AccountId] = (account, 
                                new Money(line.Amount.Amount - currentAmount.Amount, currentAmount.CurrencyCode), 
                                line.IsDebit);
                        }
                        else
                        {
                            // Equal amounts in opposite directions - they cancel out
                            accountUpdates[line.AccountId] = (account, 
                                new Money(0, currentAmount.CurrencyCode), 
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
                            AccountNumber = line.Account?.AccountNumber,
                            AccountName = line.Account?.AccountName,
                            Description = line.Description,
                            DebitAmount = line.IsDebit ? line.Amount.Amount : 0,
                            CreditAmount = !line.IsDebit ? line.Amount.Amount : 0,
                            CurrencyCode = line.Amount.CurrencyCode
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
        public string AccountId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public decimal Balance { get; set; }
        public string CurrencyCode { get; set; }
        public string Classification { get; set; }
        public string AccountType { get; set; }
        public DateTime AsOfDate { get; set; }
    }

    public class JournalEntryDto
    {
        public string Id { get; set; }
        public string JournalEntryNumber { get; set; }
        public DateTime EntryDate { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string EntryType { get; set; }
        public string Reference { get; set; }
        public string SourceDocument { get; set; }
        public string FinancialPeriodId { get; set; }
        public List<JournalEntryLineDto> Lines { get; set; }
    }

    public class JournalEntryLineDto
    {
        public string Id { get; set; }
        public string AccountId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string Description { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public string CurrencyCode { get; set; }
    }
}