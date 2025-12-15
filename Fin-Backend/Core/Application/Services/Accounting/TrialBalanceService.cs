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
    public interface ITrialBalanceService
    {
        Task<TrialBalanceDto> GenerateTrialBalanceAsync(
            DateTime asOfDate, 
            bool includeZeroBalances = false, 
            string currencyCode = null,
            CancellationToken cancellationToken = default);
            
        Task<TrialBalanceDto> GenerateTrialBalanceByBranchAsync(
            int branchId, 
            DateTime asOfDate, 
            bool includeZeroBalances = false, 
            string currencyCode = null,
            CancellationToken cancellationToken = default);
            
        Task<TrialBalanceDto> GenerateTrialBalanceByFinancialPeriodAsync(
            string financialPeriodId, 
            bool includeZeroBalances = false, 
            string currencyCode = null,
            CancellationToken cancellationToken = default);
            
        Task<UnadjustedTrialBalanceDto> GenerateUnadjustedTrialBalanceAsync(
            string financialPeriodId, 
            bool includeZeroBalances = false, 
            string currencyCode = null,
            CancellationToken cancellationToken = default);
            
        Task<AdjustedTrialBalanceDto> GenerateAdjustedTrialBalanceAsync(
            string financialPeriodId, 
            bool includeZeroBalances = false, 
            string currencyCode = null,
            CancellationToken cancellationToken = default);
    }

    public class TrialBalanceService : ITrialBalanceService
    {
        private readonly IChartOfAccountRepository _chartOfAccountRepository;
        private readonly IFinancialPeriodRepository _financialPeriodRepository;
        private readonly IJournalEntryRepository _journalEntryRepository;
        private readonly IGeneralLedgerService _generalLedgerService;
        
        public TrialBalanceService(
            IChartOfAccountRepository chartOfAccountRepository,
            IFinancialPeriodRepository financialPeriodRepository,
            IJournalEntryRepository journalEntryRepository,
            IGeneralLedgerService generalLedgerService)
        {
            _chartOfAccountRepository = chartOfAccountRepository ?? throw new ArgumentNullException(nameof(chartOfAccountRepository));
            _financialPeriodRepository = financialPeriodRepository ?? throw new ArgumentNullException(nameof(financialPeriodRepository));
            _journalEntryRepository = journalEntryRepository ?? throw new ArgumentNullException(nameof(journalEntryRepository));
            _generalLedgerService = generalLedgerService ?? throw new ArgumentNullException(nameof(generalLedgerService));
        }

        /// <summary>
        /// Generates a trial balance as of a specific date
        /// </summary>
        public async Task<TrialBalanceDto> GenerateTrialBalanceAsync(
            DateTime asOfDate, 
            bool includeZeroBalances = false, 
            string currencyCode = null,
            CancellationToken cancellationToken = default)
        {
            // Get all active accounts
            var accounts = await _chartOfAccountRepository.GetActiveAccountsAsync(cancellationToken);
            
            // Filter by currency if specified
            if (!string.IsNullOrEmpty(currencyCode))
            {
                accounts = accounts.Where(a => a.CurrencyCode == currencyCode).ToList();
            }
            
            // Get account balances as of the specified date
            var accountBalances = await _generalLedgerService.GetAccountBalancesAsync(
                accounts.Select(a => a.Id), 
                asOfDate, 
                cancellationToken);
            
            // Filter out zero balances if requested
            if (!includeZeroBalances)
            {
                accountBalances = accountBalances.Where(ab => ab.Balance != 0).ToList();
            }
            
            // Group accounts by classification
            var result = new TrialBalanceDto
            {
                AsOfDate = asOfDate,
                GeneratedAt = DateTime.UtcNow,
                Accounts = accountBalances
                    .Select(ab => new TrialBalanceAccountDto
                    {
                        AccountId = ab.AccountId,
                        AccountNumber = ab.AccountNumber,
                        AccountName = ab.AccountName,
                        Classification = ab.Classification.ToString(),
                        AccountType = ab.AccountType.ToString(),
                        DebitBalance = ab.Balance > 0 && (ab.Classification == AccountClassification.Asset || ab.Classification == AccountClassification.Expense) ? ab.Balance : 
                                      ab.Balance < 0 && (ab.Classification == AccountClassification.Liability || ab.Classification == AccountClassification.Equity || ab.Classification == AccountClassification.Revenue) ? Math.Abs(ab.Balance) : 0,
                        CreditBalance = ab.Balance < 0 && (ab.Classification == AccountClassification.Asset || ab.Classification == AccountClassification.Expense) ? Math.Abs(ab.Balance) : 
                                       ab.Balance > 0 && (ab.Classification == AccountClassification.Liability || ab.Classification == AccountClassification.Equity || ab.Classification == AccountClassification.Revenue) ? ab.Balance : 0,
                        CurrencyCode = ab.CurrencyCode
                    })
                    .OrderBy(a => a.AccountNumber)
                    .ToList()
            };
            
            // Calculate totals
            result.TotalDebits = result.Accounts.Sum(a => a.DebitBalance);
            result.TotalCredits = result.Accounts.Sum(a => a.CreditBalance);
            result.IsBalanced = Math.Abs(result.TotalDebits - result.TotalCredits) < 0.01m; // Allow small rounding differences
            
            return result;
        }

        /// <summary>
        /// Generates a trial balance for a specific branch as of a specific date
        /// </summary>
        public async Task<TrialBalanceDto> GenerateTrialBalanceByBranchAsync(
            int branchId, 
            DateTime asOfDate, 
            bool includeZeroBalances = false, 
            string currencyCode = null,
            CancellationToken cancellationToken = default)
        {
            // Get all active accounts for the branch
            var accounts = await _chartOfAccountRepository.GetActiveAccountsAsync(cancellationToken);
            accounts = accounts.Where(a => a.BranchId == branchId || a.BranchId == null).ToList();
            
            // Filter by currency if specified
            if (!string.IsNullOrEmpty(currencyCode))
            {
                accounts = accounts.Where(a => a.CurrencyCode == currencyCode).ToList();
            }
            
            // Get account balances as of the specified date
            var accountBalances = await _generalLedgerService.GetAccountBalancesAsync(
                accounts.Select(a => a.Id), 
                asOfDate, 
                cancellationToken);
            
            // Filter out zero balances if requested
            if (!includeZeroBalances)
            {
                accountBalances = accountBalances.Where(ab => ab.Balance != 0).ToList();
            }
            
            // Group accounts by classification
            var result = new TrialBalanceDto
            {
                AsOfDate = asOfDate,
                GeneratedAt = DateTime.UtcNow,
                BranchId = branchId,
                Accounts = accountBalances
                    .Select(ab => new TrialBalanceAccountDto
                    {
                        AccountId = ab.AccountId,
                        AccountNumber = ab.AccountNumber,
                        AccountName = ab.AccountName,
                        Classification = ab.Classification.ToString(),
                        AccountType = ab.AccountType.ToString(),
                        DebitBalance = ab.Balance > 0 && (ab.Classification == AccountClassification.Asset || ab.Classification == AccountClassification.Expense) ? ab.Balance : 
                                      ab.Balance < 0 && (ab.Classification == AccountClassification.Liability || ab.Classification == AccountClassification.Equity || ab.Classification == AccountClassification.Revenue) ? Math.Abs(ab.Balance) : 0,
                        CreditBalance = ab.Balance < 0 && (ab.Classification == AccountClassification.Asset || ab.Classification == AccountClassification.Expense) ? Math.Abs(ab.Balance) : 
                                       ab.Balance > 0 && (ab.Classification == AccountClassification.Liability || ab.Classification == AccountClassification.Equity || ab.Classification == AccountClassification.Revenue) ? ab.Balance : 0,
                        CurrencyCode = ab.CurrencyCode
                    })
                    .OrderBy(a => a.AccountNumber)
                    .ToList()
            };
            
            // Calculate totals
            result.TotalDebits = result.Accounts.Sum(a => a.DebitBalance);
            result.TotalCredits = result.Accounts.Sum(a => a.CreditBalance);
            result.IsBalanced = Math.Abs(result.TotalDebits - result.TotalCredits) < 0.01m; // Allow small rounding differences
            
            return result;
        }

        /// <summary>
        /// Generates a trial balance for a specific financial period
        /// </summary>
        public async Task<TrialBalanceDto> GenerateTrialBalanceByFinancialPeriodAsync(
            string financialPeriodId, 
            bool includeZeroBalances = false, 
            string currencyCode = null,
            CancellationToken cancellationToken = default)
        {
            // Get the financial period
            var financialPeriod = await _financialPeriodRepository.GetByIdAsync(financialPeriodId, cancellationToken);
            if (financialPeriod == null)
            {
                throw new InvalidOperationException($"Financial period with ID {financialPeriodId} not found");
            }
            
            // Generate trial balance as of the end date of the financial period
            return await GenerateTrialBalanceAsync(
                financialPeriod.EndDate, 
                includeZeroBalances, 
                currencyCode,
                cancellationToken);
        }

        /// <summary>
        /// Generates an unadjusted trial balance for a specific financial period (before adjustment entries)
        /// </summary>
        public async Task<UnadjustedTrialBalanceDto> GenerateUnadjustedTrialBalanceAsync(
            string financialPeriodId, 
            bool includeZeroBalances = false, 
            string currencyCode = null,
            CancellationToken cancellationToken = default)
        {
            // Get the financial period
            var financialPeriod = await _financialPeriodRepository.GetByIdAsync(financialPeriodId, cancellationToken);
            if (financialPeriod == null)
            {
                throw new InvalidOperationException($"Financial period with ID {financialPeriodId} not found");
            }
            
            // Get all active accounts
            var accounts = await _chartOfAccountRepository.GetActiveAccountsAsync(cancellationToken);
            
            // Filter by currency if specified
            if (!string.IsNullOrEmpty(currencyCode))
            {
                accounts = accounts.Where(a => a.CurrencyCode == currencyCode).ToList();
            }
            
            // Get account balances as of the financial period end date, excluding adjustment entries
            var accountBalances = new List<FinTech.Core.Application.DTOs.GeneralLedger.Account.AccountBalanceDto>();
            
            foreach (var account in accounts)
            {
                // Get all posted journal entries for this account in this financial period
                var journalEntries = await _journalEntryRepository.GetByAccountIdAsync(account.Id, cancellationToken);
                var periodEntries = journalEntries
                    .Where(je => je.Status == JournalEntryStatus.Posted && 
                                je.FinancialPeriodId == financialPeriodId &&
                                je.EntryType != JournalEntryType.YearEndClosing)
                    .ToList();
                
                // Calculate balance based on these entries
                decimal balance = 0;
                foreach (var entry in periodEntries)
                {
                    foreach (var line in entry.JournalEntryLines.Where(l => l.AccountId == account.Id))
                    {
                        if (account.NormalBalance == NormalBalanceType.Debit)
                        {
                            if (line.IsDebit)
                                balance += line.Amount.Amount;
                            else
                                balance -= line.Amount.Amount;
                        }
                        else
                        {
                            if (line.IsDebit)
                                balance -= line.Amount.Amount;
                            else
                                balance += line.Amount.Amount;
                        }
                    }
                }
                
                // Skip if balance is zero and we're not including zero balances
                if (balance == 0 && !includeZeroBalances)
                    continue;
                
                accountBalances.Add(new FinTech.Core.Application.DTOs.GeneralLedger.Account.AccountBalanceDto
                {
                    AccountId = account.Id,
                    AccountNumber = account.AccountNumber,
                    AccountName = account.AccountName,
                    Balance = balance,
                    CurrencyCode = account.CurrencyCode,
                    Classification = account.Classification,
                    AccountType = (FinTech.Core.Domain.Enums.AccountType)account.AccountType,
                    AsOfDate = financialPeriod.EndDate
                });
            }
            
            // Group accounts by classification
            var result = new UnadjustedTrialBalanceDto
            {
                FinancialPeriodId = financialPeriodId,
                FinancialPeriodName = financialPeriod.Name,
                StartDate = financialPeriod.StartDate,
                EndDate = financialPeriod.EndDate,
                GeneratedAt = DateTime.UtcNow,
                Accounts = accountBalances
                    .Select(ab => new TrialBalanceAccountDto
                    {
                        AccountId = ab.AccountId,
                        AccountNumber = ab.AccountNumber,
                        AccountName = ab.AccountName,
                        Classification = ab.Classification.ToString(),
                        AccountType = ab.AccountType.ToString(),
                        // FinTech Best Practice: ab.Classification is already string, compare directly
                        DebitBalance = ab.Balance > 0 && (ab.Classification == AccountClassification.Asset || ab.Classification == AccountClassification.Expense) ? ab.Balance : 
                                      ab.Balance < 0 && (ab.Classification == AccountClassification.Liability || ab.Classification == AccountClassification.Equity || ab.Classification == AccountClassification.Revenue) ? Math.Abs(ab.Balance) : 0,
                        CreditBalance = ab.Balance < 0 && (ab.Classification == AccountClassification.Asset || ab.Classification == AccountClassification.Expense) ? Math.Abs(ab.Balance) : 
                                       ab.Balance > 0 && (ab.Classification == AccountClassification.Liability || ab.Classification == AccountClassification.Equity || ab.Classification == AccountClassification.Revenue) ? ab.Balance : 0,
                        CurrencyCode = ab.CurrencyCode
                    })
                    .OrderBy(a => a.AccountNumber)
                    .ToList()
            };
            
            // Calculate totals
            result.TotalDebits = result.Accounts.Sum(a => a.DebitBalance);
            result.TotalCredits = result.Accounts.Sum(a => a.CreditBalance);
            result.IsBalanced = Math.Abs(result.TotalDebits - result.TotalCredits) < 0.01m; // Allow small rounding differences
            
            return result;
        }

        /// <summary>
        /// Generates an adjusted trial balance for a specific financial period (after adjustment entries)
        /// </summary>
        public async Task<AdjustedTrialBalanceDto> GenerateAdjustedTrialBalanceAsync(
            string financialPeriodId, 
            bool includeZeroBalances = false, 
            string currencyCode = null,
            CancellationToken cancellationToken = default)
        {
            // Get the financial period
            var financialPeriod = await _financialPeriodRepository.GetByIdAsync(financialPeriodId, cancellationToken);
            if (financialPeriod == null)
            {
                throw new InvalidOperationException($"Financial period with ID {financialPeriodId} not found");
            }
            
            // First, get the unadjusted trial balance
            var unadjustedTB = await GenerateUnadjustedTrialBalanceAsync(
                financialPeriodId, 
                true, // Include zero balances for the calculation
                currencyCode,
                cancellationToken);
            
            // Then, get all adjustment entries for this financial period
            var adjustmentEntries = new List<JournalEntryAdjustmentDto>();
            
            // Get all active accounts
            var accounts = await _chartOfAccountRepository.GetActiveAccountsAsync(cancellationToken);
            
            // Filter by currency if specified
            if (!string.IsNullOrEmpty(currencyCode))
            {
                accounts = accounts.Where(a => a.CurrencyCode == currencyCode).ToList();
            }
            
            // Calculate the adjustments for each account
            var adjustedBalances = new Dictionary<string, decimal>();
            
            foreach (var account in accounts)
            {
                // Get all posted adjustment journal entries for this account in this financial period
                var journalEntries = await _journalEntryRepository.GetByAccountIdAsync(account.Id, cancellationToken);
                var adjustmentEntryList = journalEntries
                    .Where(je => je.Status == JournalEntryStatus.Posted && 
                                je.FinancialPeriodId == financialPeriodId &&
                                je.EntryType == JournalEntryType.YearEndClosing)
                    .ToList();
                
                // Calculate adjustment amount based on these entries
                decimal adjustmentAmount = 0;
                foreach (var entry in adjustmentEntryList)
                {
                    foreach (var line in entry.JournalEntryLines.Where(l => l.AccountId == account.Id))
                    {
                        if (account.NormalBalance == NormalBalanceType.Debit)
                        {
                            if (line.IsDebit)
                                adjustmentAmount += line.Amount.Amount;
                            else
                                adjustmentAmount -= line.Amount.Amount;
                        }
                        else
                        {
                            if (line.IsDebit)
                                adjustmentAmount -= line.Amount.Amount;
                            else
                                adjustmentAmount += line.Amount.Amount;
                        }
                    }
                    
                    // Add to the adjustment entries list if not already there
                    if (!adjustmentEntries.Any(ae => ae.JournalEntryId == entry.Id))
                    {
                        adjustmentEntries.Add(new JournalEntryAdjustmentDto
                        {
                            JournalEntryId = entry.Id,
                            JournalEntryNumber = entry.JournalEntryNumber,
                            Description = entry.Description,
                            EntryDate = entry.EntryDate,
                            Lines = entry.JournalEntryLines
                                .Select(line => new JournalEntryLineAdjustmentDto
                                {
                                    AccountId = line.AccountId,
                                    AccountNumber = line.Account?.AccountNumber,
                                    AccountName = line.Account?.AccountName,
                                    DebitAmount = line.IsDebit ? line.Amount.Amount : 0,
                                    CreditAmount = !line.IsDebit ? line.Amount.Amount : 0,
                                    CurrencyCode = line.Amount.Currency
                                })
                                .ToList()
                        });
                    }
                }
                
                // Store the adjustment amount for this account
                adjustedBalances[account.Id] = adjustmentAmount;
            }
            
            // Now create the adjusted trial balance
            var result = new AdjustedTrialBalanceDto
            {
                FinancialPeriodId = financialPeriodId,
                FinancialPeriodName = financialPeriod.Name,
                StartDate = financialPeriod.StartDate,
                EndDate = financialPeriod.EndDate,
                GeneratedAt = DateTime.UtcNow,
                UnadjustedTrialBalance = unadjustedTB,
                Adjustments = adjustmentEntries,
                Accounts = unadjustedTB.Accounts
                    .Select(unadjusted => 
                    {
                        // Get the adjustment amount for this account
                        decimal adjustmentAmount = adjustedBalances.ContainsKey(unadjusted.AccountId) ? 
                            adjustedBalances[unadjusted.AccountId] : 0;
                        
                        // Calculate the adjusted balance
                        decimal adjustedBalance = 0;
                        if (unadjusted.DebitBalance > 0)
                            adjustedBalance = unadjusted.DebitBalance + adjustmentAmount;
                        else
                            adjustedBalance = unadjusted.CreditBalance + adjustmentAmount;
                        
                        // Create the adjusted account entry
                        return new AdjustedTrialBalanceAccountDto
                        {
                            AccountId = unadjusted.AccountId,
                            AccountNumber = unadjusted.AccountNumber,
                            AccountName = unadjusted.AccountName,
                            Classification = unadjusted.Classification,
                            AccountType = unadjusted.AccountType,
                            UnadjustedDebitBalance = unadjusted.DebitBalance,
                            UnadjustedCreditBalance = unadjusted.CreditBalance,
                            AdjustmentDebit = adjustmentAmount > 0 ? adjustmentAmount : 0,
                            AdjustmentCredit = adjustmentAmount < 0 ? Math.Abs(adjustmentAmount) : 0,
                            AdjustedDebitBalance = adjustedBalance > 0 ? adjustedBalance : 0,
                            AdjustedCreditBalance = adjustedBalance < 0 ? Math.Abs(adjustedBalance) : 0,
                            CurrencyCode = unadjusted.CurrencyCode
                        };
                    })
                    .ToList()
            };
            
            // Filter out zero balances if requested
            if (!includeZeroBalances)
            {
                result.Accounts = result.Accounts
                    .Where(a => a.AdjustedDebitBalance != 0 || a.AdjustedCreditBalance != 0)
                    .ToList();
            }
            
            // Calculate totals
            result.TotalUnadjustedDebits = result.Accounts.Sum(a => a.UnadjustedDebitBalance);
            result.TotalUnadjustedCredits = result.Accounts.Sum(a => a.UnadjustedCreditBalance);
            result.TotalAdjustmentDebits = result.Accounts.Sum(a => a.AdjustmentDebit);
            result.TotalAdjustmentCredits = result.Accounts.Sum(a => a.AdjustmentCredit);
            result.TotalAdjustedDebits = result.Accounts.Sum(a => a.AdjustedDebitBalance);
            result.TotalAdjustedCredits = result.Accounts.Sum(a => a.AdjustedCreditBalance);
            result.IsBalanced = Math.Abs(result.TotalAdjustedDebits - result.TotalAdjustedCredits) < 0.01m; // Allow small rounding differences
            
            return result;
        }
    }

    // DTOs
    public class TrialBalanceDto
    {
        public DateTime AsOfDate { get; set; }
        public DateTime GeneratedAt { get; set; }
        public int? BranchId { get; set; }
        public List<TrialBalanceAccountDto> Accounts { get; set; }
        public decimal TotalDebits { get; set; }
        public decimal TotalCredits { get; set; }
        public bool IsBalanced { get; set; }
    }

    public class TrialBalanceAccountDto
    {
        public string AccountId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string Classification { get; set; }
        public string AccountType { get; set; }
        public decimal DebitBalance { get; set; }
        public decimal CreditBalance { get; set; }
        public string CurrencyCode { get; set; }
    }

    public class UnadjustedTrialBalanceDto : TrialBalanceDto
    {
        public string FinancialPeriodId { get; set; }
        public string FinancialPeriodName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class AdjustedTrialBalanceDto
    {
        public string FinancialPeriodId { get; set; }
        public string FinancialPeriodName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime GeneratedAt { get; set; }
        public UnadjustedTrialBalanceDto UnadjustedTrialBalance { get; set; }
        public List<JournalEntryAdjustmentDto> Adjustments { get; set; }
        public List<AdjustedTrialBalanceAccountDto> Accounts { get; set; }
        public decimal TotalUnadjustedDebits { get; set; }
        public decimal TotalUnadjustedCredits { get; set; }
        public decimal TotalAdjustmentDebits { get; set; }
        public decimal TotalAdjustmentCredits { get; set; }
        public decimal TotalAdjustedDebits { get; set; }
        public decimal TotalAdjustedCredits { get; set; }
        public bool IsBalanced { get; set; }
    }

    public class AdjustedTrialBalanceAccountDto
    {
        public string AccountId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string Classification { get; set; }
        public string AccountType { get; set; }
        public decimal UnadjustedDebitBalance { get; set; }
        public decimal UnadjustedCreditBalance { get; set; }
        public decimal AdjustmentDebit { get; set; }
        public decimal AdjustmentCredit { get; set; }
        public decimal AdjustedDebitBalance { get; set; }
        public decimal AdjustedCreditBalance { get; set; }
        public string CurrencyCode { get; set; }
    }

    public class JournalEntryAdjustmentDto
    {
        public string JournalEntryId { get; set; }
        public string JournalEntryNumber { get; set; }
        public string Description { get; set; }
        public DateTime EntryDate { get; set; }
        public List<JournalEntryLineAdjustmentDto> Lines { get; set; }
    }

    public class JournalEntryLineAdjustmentDto
    {
        public string AccountId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public string CurrencyCode { get; set; }
    }
}
