using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Domain.Entities.Accounting;
using FinTech.Domain.Repositories.Accounting;
using FinTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinTech.Infrastructure.Repositories.Accounting
{
    /// <summary>
    /// Repository implementation for Fiscal Years
    /// </summary>
    public class FiscalYearRepository : Repository<FiscalYear>, IFiscalYearRepository
    {
        public FiscalYearRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<FiscalYear> GetCurrentFiscalYearAsync(CancellationToken cancellationToken = default)
        {
            var currentDate = DateTime.UtcNow.Date;
            
            return await _context.FiscalYears
                .Include(f => f.FinancialPeriods)
                .Where(f => f.StartDate <= currentDate && f.EndDate >= currentDate)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<FiscalYear> GetByYearAsync(int year, CancellationToken cancellationToken = default)
        {
            return await _context.FiscalYears
                .Include(f => f.FinancialPeriods)
                .Where(f => f.Year == year)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<FiscalYear>> GetOpenFiscalYearsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.FiscalYears
                .Where(f => f.Status == FiscalYearStatus.Open)
                .OrderByDescending(f => f.StartDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsAsync(int year, CancellationToken cancellationToken = default)
        {
            return await _context.FiscalYears.AnyAsync(f => f.Year == year, cancellationToken);
        }

        public async Task<FiscalYear> GetFiscalYearByDateAsync(System.DateTime date, CancellationToken cancellationToken = default)
        {
            return await _context.FiscalYears
                .Where(f => f.StartDate <= date && f.EndDate >= date)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<FiscalYear> GetByCodeAsync(string fiscalYearCode, CancellationToken cancellationToken = default)
        {
            return await _context.FiscalYears
                .Include(f => f.FinancialPeriods)
                .Where(f => f.Code == fiscalYearCode)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<FiscalYear>> GetActiveFiscalYearsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.FiscalYears
                .Where(f => f.Status == FiscalYearStatus.Active || f.Status == FiscalYearStatus.Open)
                .OrderByDescending(f => f.StartDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<FiscalYear> GetPreviousFiscalYearAsync(string currentFiscalYearId, CancellationToken cancellationToken = default)
        {
            var currentFiscalYear = await _context.FiscalYears
                .FirstOrDefaultAsync(f => f.Id == currentFiscalYearId, cancellationToken);
                
            if (currentFiscalYear == null)
                return null;
                
            return await _context.FiscalYears
                .Where(f => f.EndDate < currentFiscalYear.StartDate)
                .OrderByDescending(f => f.EndDate)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<FiscalYear> GetNextFiscalYearAsync(string currentFiscalYearId, CancellationToken cancellationToken = default)
        {
            var currentFiscalYear = await _context.FiscalYears
                .FirstOrDefaultAsync(f => f.Id == currentFiscalYearId, cancellationToken);
                
            if (currentFiscalYear == null)
                return null;
                
            return await _context.FiscalYears
                .Where(f => f.StartDate > currentFiscalYear.EndDate)
                .OrderBy(f => f.StartDate)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> CanCloseFiscalYearAsync(string fiscalYearId, CancellationToken cancellationToken = default)
        {
            var fiscalYear = await _context.FiscalYears
                .Include(f => f.FinancialPeriods)
                .FirstOrDefaultAsync(f => f.Id == fiscalYearId, cancellationToken);
                
            if (fiscalYear == null)
                return false;
                
            // Check if all periods are closed
            var allPeriodsClosed = fiscalYear.FinancialPeriods.All(p => p.Status == FinancialPeriodStatus.Closed);
            
            // Check if there are no pending journal entries for this fiscal year
            var hasPendingJournalEntries = await _context.JournalEntries
                .AnyAsync(j => 
                    j.Status == JournalEntryStatus.Pending && 
                    fiscalYear.FinancialPeriods.Any(p => p.Id == j.FinancialPeriodId),
                    cancellationToken);
                    
            return allPeriodsClosed && !hasPendingJournalEntries;
        }

        public async Task<string> GenerateFiscalYearCodeAsync(int year, CancellationToken cancellationToken = default)
        {
            // Format: FY-YYYY (e.g., FY-2023)
            return $"FY-{year}";
        }
    }
}