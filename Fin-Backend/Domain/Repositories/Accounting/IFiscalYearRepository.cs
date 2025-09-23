using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Domain.Entities.Accounting;

namespace FinTech.Domain.Repositories.Accounting
{
    /// <summary>
    /// Repository interface for Fiscal Years
    /// </summary>
    public interface IFiscalYearRepository : IRepository<FiscalYear>
    {
        Task<FiscalYear> GetByYearAsync(
            int year, 
            CancellationToken cancellationToken = default);
            
        Task<FiscalYear> GetCurrentFiscalYearAsync(
            CancellationToken cancellationToken = default);
            
        Task<IReadOnlyList<FiscalYear>> GetOpenFiscalYearsAsync(
            CancellationToken cancellationToken = default);
            
        Task<bool> ExistsAsync(
            int year, 
            CancellationToken cancellationToken = default);
            
        Task<FiscalYear> GetFiscalYearByDateAsync(
            System.DateTime date, 
            CancellationToken cancellationToken = default);

        Task<FiscalYear> GetByCodeAsync(string fiscalYearCode, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<FiscalYear>> GetActiveFiscalYearsAsync(CancellationToken cancellationToken = default);
        Task<FiscalYear> GetPreviousFiscalYearAsync(string currentFiscalYearId, CancellationToken cancellationToken = default);
        Task<FiscalYear> GetNextFiscalYearAsync(string currentFiscalYearId, CancellationToken cancellationToken = default);
        Task<bool> CanCloseFiscalYearAsync(string fiscalYearId, CancellationToken cancellationToken = default);
        Task<string> GenerateFiscalYearCodeAsync(int year, CancellationToken cancellationToken = default);
    }
}