using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Domain.Entities.Accounting;

namespace FinTech.Domain.Repositories.Accounting
{
    /// <summary>
    /// Repository interface for Financial Periods
    /// </summary>
    public interface IFinancialPeriodRepository : IRepository<FinancialPeriod>
    {
        Task<FinancialPeriod> GetByPeriodCodeAsync(
            string periodCode, 
            CancellationToken cancellationToken = default);
            
        Task<FinancialPeriod> GetCurrentPeriodAsync(
            CancellationToken cancellationToken = default);
            
        Task<IReadOnlyList<FinancialPeriod>> GetByFiscalYearAsync(
            int fiscalYear, 
            CancellationToken cancellationToken = default);
            
        Task<IReadOnlyList<FinancialPeriod>> GetOpenPeriodsAsync(
            CancellationToken cancellationToken = default);
            
        Task<bool> IsPeriodOpenAsync(
            string periodId, 
            CancellationToken cancellationToken = default);
            
        Task<FinancialPeriod> GetPeriodByDateAsync(
            System.DateTime date, 
            CancellationToken cancellationToken = default);
    }
}