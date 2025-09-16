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
    /// Repository implementation for Financial Periods
    /// </summary>
    public class FinancialPeriodRepository : Repository<FinancialPeriod>, IFinancialPeriodRepository
    {
        public FinancialPeriodRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<FinancialPeriod> GetCurrentPeriodAsync(CancellationToken cancellationToken = default)
        {
            var currentDate = DateTime.UtcNow.Date;
            
            return await _context.FinancialPeriods
                .Where(p => p.StartDate <= currentDate && p.EndDate >= currentDate)
                .Include(p => p.FiscalYear)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<FinancialPeriod> GetByNameAsync(string periodName, string fiscalYearId, CancellationToken cancellationToken = default)
        {
            return await _context.FinancialPeriods
                .Where(p => p.Name == periodName && p.FiscalYearId == fiscalYearId)
                .Include(p => p.FiscalYear)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<FinancialPeriod>> GetByFiscalYearAsync(string fiscalYearId, CancellationToken cancellationToken = default)
        {
            return await _context.FinancialPeriods
                .Where(p => p.FiscalYearId == fiscalYearId)
                .OrderBy(p => p.StartDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<FinancialPeriod> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default)
        {
            return await _context.FinancialPeriods
                .Where(p => p.StartDate <= date && p.EndDate >= date)
                .Include(p => p.FiscalYear)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<FinancialPeriod>> GetOpenPeriodsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.FinancialPeriods
                .Where(p => p.Status == FinancialPeriodStatus.Open)
                .OrderBy(p => p.StartDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsPeriodValidForPostingAsync(string periodId, CancellationToken cancellationToken = default)
        {
            var period = await _context.FinancialPeriods
                .FirstOrDefaultAsync(p => p.Id == periodId, cancellationToken);
            
            return period != null && period.Status == FinancialPeriodStatus.Open;
        }

        public async Task<FinancialPeriod> GetNextPeriodAsync(string currentPeriodId, CancellationToken cancellationToken = default)
        {
            var currentPeriod = await _context.FinancialPeriods
                .FirstOrDefaultAsync(p => p.Id == currentPeriodId, cancellationToken);
                
            if (currentPeriod == null)
                return null;
                
            return await _context.FinancialPeriods
                .Where(p => p.StartDate > currentPeriod.EndDate)
                .OrderBy(p => p.StartDate)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}