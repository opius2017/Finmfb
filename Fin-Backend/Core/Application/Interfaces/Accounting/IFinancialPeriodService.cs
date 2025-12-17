using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Accounting; 
using FinTech.Core.Application.DTOs.Accounting;

namespace FinTech.Core.Application.Interfaces.Accounting
{
    public interface IFinancialPeriodService
    {
        // Methods from FinancialPeriodService implementation
        Task<FinancialPeriod> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<FinancialPeriod> GetCurrentPeriodAsync(CancellationToken cancellationToken = default);
        Task<FinancialPeriod> GetByNameAsync(string periodName, string fiscalYearId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<FinancialPeriod>> GetByFiscalYearAsync(string fiscalYearId, CancellationToken cancellationToken = default);
        Task<FinancialPeriod> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<FinancialPeriod>> GetOpenPeriodsAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<FinancialPeriod>> GetAllPeriodsAsync(CancellationToken cancellationToken = default);
        Task<string> CreatePeriodAsync(FinancialPeriod period, CancellationToken cancellationToken = default);
        Task UpdatePeriodAsync(FinancialPeriod period, CancellationToken cancellationToken = default);
        Task OpenPeriodAsync(string id, string modifiedBy, CancellationToken cancellationToken = default);
        Task ClosePeriodAsync(string id, string modifiedBy, CancellationToken cancellationToken = default);
        Task<bool> IsPeriodValidForPostingAsync(string periodId, CancellationToken cancellationToken = default);
        Task<FinancialPeriod> GetNextPeriodAsync(string currentPeriodId, CancellationToken cancellationToken = default);

        // Keeping legacy methods if needed (though they might need implementation in Service)
        // Task<List<FinancialPeriodDto>> GetFinancialPeriodsByYearAsync(int fiscalYear);
        // Task<FinancialPeriodDto> GetFinancialPeriodByIdAsync(string id);
        // Task<BaseResponse<PeriodClosingStatusDto>> ClosePeriodAsync(ClosePeriodRequestDto request);
        // Task<PeriodClosingSummaryDto> GetPeriodClosingSummaryAsync(string id);
        
        // Note: I commented out the legacy methods because FinancialPeriodService does NOT implement them.
        // If the Controller uses them, we will get errors, but we can fix the Controller to use the new methods.
        // E.g. 'GetAllPeriodsAsync' was requested by errors?
        // Wait, 'GetAllPeriodsAsync' was in the error log: "IFinancialPeriodService does not contain definition for GetAllPeriodsAsync"
        // But FinancialPeriodService.cs ALSO DOES NOT HAVE GetAllPeriodsAsync!
        // It has GetByFiscalYearAsync, GetOpenPeriodsAsync.
        // Maybe GetAllPeriodsAsync is missing entirely?
    }
}
