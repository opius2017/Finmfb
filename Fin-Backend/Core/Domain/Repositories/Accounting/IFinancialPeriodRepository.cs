using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Accounting;

namespace FinTech.Core.Domain.Repositories.Accounting
{
    /// <summary>
    /// Repository interface for Financial Periods
    /// </summary>
    public interface IFinancialPeriodRepository : IRepository<FinancialPeriod>
    {
        Task<FinancialPeriod> GetByPeriodCodeAsync(
            string periodCode, 
            CancellationToken cancellationToken = default);

        Task<FinancialPeriod> GetByNameAsync(
            string periodName, 
            string fiscalYearId,
            CancellationToken cancellationToken = default);
            
        Task<FinancialPeriod> GetCurrentPeriodAsync(
            CancellationToken cancellationToken = default);
            
        Task<IReadOnlyList<FinancialPeriod>> GetByFiscalYearAsync(
            string fiscalYearId, 
            CancellationToken cancellationToken = default);
            
        Task<IReadOnlyList<FinancialPeriod>> GetOpenPeriodsAsync(
            CancellationToken cancellationToken = default);
            
        Task<bool> IsPeriodOpenAsync(
            string periodId, 
            CancellationToken cancellationToken = default);
            
        Task<FinancialPeriod> GetPeriodByDateAsync(
            System.DateTime date, 
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Gets the previous financial period based on the current period ID
        /// </summary>
        Task<FinancialPeriod> GetPreviousPeriodAsync(
            string currentPeriodId, 
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Gets the next financial period based on the current period ID
        /// </summary>
        Task<FinancialPeriod> GetNextPeriodAsync(
            string currentPeriodId, 
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Gets the current active financial period
        /// </summary>
        Task<FinancialPeriod> GetCurrentActivePeriodAsync(
            CancellationToken cancellationToken = default);

        Task<bool> IsPeriodValidForPostingAsync(
            string periodId, 
            CancellationToken cancellationToken = default);
    }
}
