using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FinTech.Core.Application.Interfaces.Repositories;
using FinTech.Domain.Entities.Accounting;
using FinTech.Domain.Entities.Common;
using FinTech.Infrastructure.Data;

namespace FinTech.Infrastructure.Repositories
{
    public class GeneralLedgerRepository : IGeneralLedgerRepository
    {
        private readonly ApplicationDbContext _context;

        public GeneralLedgerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Journal Entry Operations
        public async Task<JournalEntry> GetJournalEntryByIdAsync(string journalEntryId)
        {
            return await _context.CoreJournalEntries
                .Include(j => j.JournalEntryLines)
                .FirstOrDefaultAsync(j => j.Id == journalEntryId);
        }

        public async Task<IEnumerable<JournalEntry>> GetJournalEntriesByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.CoreJournalEntries
                .Include(j => j.JournalEntryLines)
                .Where(j => j.EntryDate >= fromDate && j.EntryDate <= toDate)
                .OrderByDescending(j => j.EntryDate)
                .ThenBy(j => j.JournalEntryNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<JournalEntry>> GetPendingJournalEntriesAsync()
        {
            return await _context.CoreJournalEntries
                .Include(j => j.JournalEntryLines)
                .Where(j => j.Status == JournalEntryStatus.Pending)
                .OrderByDescending(j => j.EntryDate)
                .ThenBy(j => j.JournalEntryNumber)
                .ToListAsync();
        }

        public async Task<JournalEntry> CreateJournalEntryAsync(JournalEntry journalEntry)
        {
            _context.CoreJournalEntries.Add(journalEntry);
            await _context.SaveChangesAsync();
            return journalEntry;
        }

        public async Task<bool> UpdateJournalEntryAsync(JournalEntry journalEntry)
        {
            _context.CoreJournalEntries.Update(journalEntry);
            return await _context.SaveChangesAsync() > 0;
        }

        // Chart of Account Operations
        public async Task<ChartOfAccount> GetAccountByIdAsync(string accountId)
        {
            return await _context.CoreChartOfAccounts
                .FirstOrDefaultAsync(a => a.Id == accountId);
        }

        public async Task<ChartOfAccount> GetAccountByCodeAsync(string accountCode)
        {
            return await _context.CoreChartOfAccounts
                .FirstOrDefaultAsync(a => a.AccountCode == accountCode);
        }

        public async Task<IEnumerable<ChartOfAccount>> GetAccountsByIdsAsync(IEnumerable<string> accountIds)
        {
            return await _context.CoreChartOfAccounts
                .Where(a => accountIds.Contains(a.Id))
                .ToListAsync();
        }

        public async Task<IEnumerable<ChartOfAccount>> GetAllActiveAccountsAsync()
        {
            return await _context.CoreChartOfAccounts
                .Where(a => a.IsActive)
                .OrderBy(a => a.AccountCode)
                .ToListAsync();
        }

        public async Task<bool> UpdateAccountBalanceAsync(string accountId, Money amount, bool isDebit)
        {
            var account = await GetAccountByIdAsync(accountId);
            if (account == null)
                return false;

            account.UpdateBalance(amount, isDebit);
            _context.CoreChartOfAccounts.Update(account);
            return await _context.SaveChangesAsync() > 0;
        }

        // Journal Entry Line Operations
        public async Task<IEnumerable<JournalEntryLine>> GetJournalLinesForAccountAsync(string accountId, DateTime fromDate, DateTime toDate)
        {
            return await _context.CoreJournalEntryLines
                .Include(l => l.Account)
                .Where(l => l.AccountId == accountId)
                .Join(_context.CoreJournalEntries,
                    line => line.JournalEntryId,
                    entry => entry.Id,
                    (line, entry) => new { Line = line, Entry = entry })
                .Where(x => x.Entry.EntryDate >= fromDate && x.Entry.EntryDate <= toDate && x.Entry.Status == JournalEntryStatus.Posted)
                .OrderBy(x => x.Entry.EntryDate)
                .ThenBy(x => x.Entry.JournalEntryNumber)
                .Select(x => x.Line)
                .ToListAsync();
        }

        public async Task<IEnumerable<JournalEntryLine>> GetJournalLinesForJournalEntryAsync(string journalEntryId)
        {
            return await _context.CoreJournalEntryLines
                .Include(l => l.Account)
                .Where(l => l.JournalEntryId == journalEntryId)
                .ToListAsync();
        }

        // Financial Period Operations
        public async Task<FinancialPeriod> GetFinancialPeriodByDateAsync(DateTime date)
        {
            return await _context.FinancialPeriods
                .FirstOrDefaultAsync(p => p.StartDate <= date && p.EndDate >= date);
        }

        public async Task<FinancialPeriod> GetCurrentFinancialPeriodAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _context.FinancialPeriods
                .Where(p => p.StartDate <= today && p.EndDate >= today)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> CloseFinancialPeriodAsync(string periodId, string userId)
        {
            var period = await _context.FinancialPeriods
                .FirstOrDefaultAsync(p => p.Id == periodId);

            if (period == null || period.IsClosed)
                return false;

            period.IsClosed = true;
            period.ClosedDate = DateTime.UtcNow;
            period.ClosedBy = userId;
            period.LastModifiedDate = DateTime.UtcNow;
            period.LastModifiedBy = userId;

            _context.FinancialPeriods.Update(period);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ReopenFinancialPeriodAsync(string periodId, string userId)
        {
            var period = await _context.FinancialPeriods
                .FirstOrDefaultAsync(p => p.Id == periodId);

            if (period == null || !period.IsClosed)
                return false;

            period.IsClosed = false;
            period.ClosedDate = null;
            period.ClosedBy = null;
            period.LastModifiedDate = DateTime.UtcNow;
            period.LastModifiedBy = userId;

            _context.FinancialPeriods.Update(period);
            return await _context.SaveChangesAsync() > 0;
        }

        // Fiscal Year Operations
        public async Task<FiscalYear> GetCurrentFiscalYearAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _context.FiscalYears
                .Include(fy => fy.Periods)
                .Where(fy => fy.StartDate <= today && fy.EndDate >= today)
                .FirstOrDefaultAsync();
        }

        public async Task<FiscalYear> GetFiscalYearByIdAsync(string fiscalYearId)
        {
            return await _context.FiscalYears
                .Include(fy => fy.Periods)
                .FirstOrDefaultAsync(fy => fy.Id == fiscalYearId);
        }

        public async Task<FiscalYear> CreateFiscalYearAsync(FiscalYear fiscalYear)
        {
            _context.FiscalYears.Add(fiscalYear);
            await _context.SaveChangesAsync();
            return fiscalYear;
        }

        public async Task<bool> CloseFiscalYearAsync(string fiscalYearId, string userId)
        {
            var fiscalYear = await _context.FiscalYears
                .Include(fy => fy.Periods)
                .FirstOrDefaultAsync(fy => fy.Id == fiscalYearId);

            if (fiscalYear == null || fiscalYear.IsClosed)
                return false;

            // Check if all periods are closed
            if (fiscalYear.Periods.Any(p => !p.IsClosed))
                return false;

            fiscalYear.IsClosed = true;
            fiscalYear.ClosedDate = DateTime.UtcNow;
            fiscalYear.ClosedBy = userId;
            fiscalYear.LastModifiedDate = DateTime.UtcNow;
            fiscalYear.LastModifiedBy = userId;

            _context.FiscalYears.Update(fiscalYear);
            return await _context.SaveChangesAsync() > 0;
        }

        // Generate Next Number
        public async Task<string> GenerateJournalEntryNumberAsync()
        {
            var lastJournal = await _context.CoreJournalEntries
                .OrderByDescending(j => j.JournalEntryNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastJournal != null && lastJournal.JournalEntryNumber.StartsWith("JE"))
            {
                if (int.TryParse(lastJournal.JournalEntryNumber.Substring(2), out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"JE{nextNumber:D6}";
        }
    }
}