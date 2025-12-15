using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Accounting;
using FinTech.Core.Domain.ValueObjects;

namespace FinTech.Core.Application.Interfaces.Repositories
{
    public interface IGeneralLedgerRepository
    {
        // Journal Entry Operations
        Task<JournalEntry> GetJournalEntryByIdAsync(string journalEntryId);
        Task<IEnumerable<JournalEntry>> GetJournalEntriesByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<JournalEntry>> GetPendingJournalEntriesAsync();
        Task<JournalEntry> CreateJournalEntryAsync(JournalEntry journalEntry);
        Task<bool> UpdateJournalEntryAsync(JournalEntry journalEntry);
        
        // Chart of Account Operations
        Task<ChartOfAccount> GetAccountByIdAsync(string accountId);
        Task<ChartOfAccount> GetAccountByCodeAsync(string accountCode);
        Task<IEnumerable<ChartOfAccount>> GetAccountsByIdsAsync(IEnumerable<string> accountIds);
        Task<IEnumerable<ChartOfAccount>> GetAllActiveAccountsAsync();
        Task<bool> UpdateAccountBalanceAsync(string accountId, FinTech.Core.Domain.ValueObjects.Money amount, bool isDebit);
        
        // Journal Entry Line Operations
        Task<IEnumerable<JournalEntryLine>> GetJournalLinesForAccountAsync(string accountId, DateTime fromDate, DateTime toDate);
        Task<IEnumerable<JournalEntryLine>> GetJournalLinesForJournalEntryAsync(string journalEntryId);
        
        // Financial Period Operations
        Task<FinancialPeriod> GetFinancialPeriodByDateAsync(DateTime date);
        Task<FinancialPeriod> GetCurrentFinancialPeriodAsync();
        Task<bool> CloseFinancialPeriodAsync(string periodId, string userId);
        Task<bool> ReopenFinancialPeriodAsync(string periodId, string userId);
        
        // Fiscal Year Operations
        Task<FiscalYear> GetCurrentFiscalYearAsync();
        Task<FiscalYear> GetFiscalYearByIdAsync(string fiscalYearId);
        Task<FiscalYear> CreateFiscalYearAsync(FiscalYear fiscalYear);
        Task<bool> CloseFiscalYearAsync(string fiscalYearId, string userId);
        
        // Generate Next Number
        Task<string> GenerateJournalEntryNumberAsync();
    }
}
