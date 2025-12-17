using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Accounting;
using FinTech.Core.Domain.Repositories.Accounting;
using FinTech.Core.Domain.Repositories;
using FinTech.Core.Application.Interfaces.Accounting;

namespace FinTech.Core.Application.Services.Accounting
{


    public class FinancialPeriodService : IFinancialPeriodService
    {
        private readonly IFinancialPeriodRepository _financialPeriodRepository;
        private readonly IFiscalYearRepository _fiscalYearRepository;
        private readonly IUnitOfWork _unitOfWork;

        public FinancialPeriodService(
            IFinancialPeriodRepository financialPeriodRepository,
            IFiscalYearRepository fiscalYearRepository,
            IUnitOfWork unitOfWork)
        {
            _financialPeriodRepository = financialPeriodRepository ?? throw new ArgumentNullException(nameof(financialPeriodRepository));
            _fiscalYearRepository = fiscalYearRepository ?? throw new ArgumentNullException(nameof(fiscalYearRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<FinancialPeriod> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _financialPeriodRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<FinancialPeriod> GetCurrentPeriodAsync(CancellationToken cancellationToken = default)
        {
            return await _financialPeriodRepository.GetCurrentPeriodAsync(cancellationToken);
        }

        public async Task<FinancialPeriod> GetByNameAsync(string periodName, string fiscalYearId, CancellationToken cancellationToken = default)
        {
            return await _financialPeriodRepository.GetByNameAsync(periodName, fiscalYearId, cancellationToken);
        }

        public async Task<IReadOnlyList<FinancialPeriod>> GetByFiscalYearAsync(string fiscalYearId, CancellationToken cancellationToken = default)
        {
            return await _financialPeriodRepository.GetByFiscalYearAsync(fiscalYearId, cancellationToken);
        }

        public async Task<FinancialPeriod> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default)
        {
            return await _financialPeriodRepository.GetPeriodByDateAsync(date, cancellationToken);
        }

        public async Task<IReadOnlyList<FinancialPeriod>> GetOpenPeriodsAsync(CancellationToken cancellationToken = default)
        {
            return await _financialPeriodRepository.GetOpenPeriodsAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<FinancialPeriod>> GetAllPeriodsAsync(CancellationToken cancellationToken = default)
        {
            return await _financialPeriodRepository.GetAllAsync(cancellationToken);
        }

        public async Task<string> CreatePeriodAsync(FinancialPeriod period, CancellationToken cancellationToken = default)
        {
            // Validate period
            if (period.StartDate >= period.EndDate)
            {
                throw new ArgumentException("Start date must be before end date");
            }

            // Check if fiscal year exists
            var fiscalYear = await _fiscalYearRepository.GetByIdAsync(period.FiscalYearId, cancellationToken);
            if (fiscalYear == null)
            {
                throw new InvalidOperationException($"Fiscal year with ID {period.FiscalYearId} not found");
            }

            // Check if period dates are within fiscal year
            if (period.StartDate < fiscalYear.StartDate || period.EndDate > fiscalYear.EndDate)
            {
                throw new InvalidOperationException($"Period dates must be within fiscal year dates ({fiscalYear.StartDate:yyyy-MM-dd} to {fiscalYear.EndDate:yyyy-MM-dd})");
            }

            // Check if period name already exists in this fiscal year
            var existingPeriod = await _financialPeriodRepository.GetByNameAsync(period.Name, period.FiscalYearId, cancellationToken);
            if (existingPeriod != null)
            {
                throw new InvalidOperationException($"Period with name {period.Name} already exists in this fiscal year");
            }

            // Set default values if not provided
            if (string.IsNullOrEmpty(period.Id))
            {
                period.Id = Guid.NewGuid().ToString();
            }

            period.CreatedAt = DateTime.UtcNow;
            period.Status = period.Status == FinancialPeriodStatus.Undefined ? FinancialPeriodStatus.Planned : period.Status;

            // Add the period
            await _financialPeriodRepository.AddAsync(period, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return period.Id;
        }

        public async Task UpdatePeriodAsync(FinancialPeriod period, CancellationToken cancellationToken = default)
        {
            // Validate period
            if (period.StartDate >= period.EndDate)
            {
                throw new ArgumentException("Start date must be before end date");
            }

            // Get the existing period
            var existingPeriod = await _financialPeriodRepository.GetByIdAsync(period.Id, cancellationToken);
            if (existingPeriod == null)
            {
                throw new InvalidOperationException($"Period with ID {period.Id} not found");
            }

            // Only allow updates to planned periods
            if (existingPeriod.Status != FinancialPeriodStatus.Planned)
            {
                throw new InvalidOperationException($"Cannot update period with status {existingPeriod.Status}");
            }

            // Check if fiscal year exists
            var fiscalYear = await _fiscalYearRepository.GetByIdAsync(period.FiscalYearId, cancellationToken);
            if (fiscalYear == null)
            {
                throw new InvalidOperationException($"Fiscal year with ID {period.FiscalYearId} not found");
            }

            // Check if period dates are within fiscal year
            if (period.StartDate < fiscalYear.StartDate || period.EndDate > fiscalYear.EndDate)
            {
                throw new InvalidOperationException($"Period dates must be within fiscal year dates ({fiscalYear.StartDate:yyyy-MM-dd} to {fiscalYear.EndDate:yyyy-MM-dd})");
            }

            // Check if period name already exists in this fiscal year (excluding this period)
            var existingPeriodWithName = await _financialPeriodRepository.GetByNameAsync(period.Name, period.FiscalYearId, cancellationToken);
            if (existingPeriodWithName != null && existingPeriodWithName.Id != period.Id)
            {
                throw new InvalidOperationException($"Period with name {period.Name} already exists in this fiscal year");
            }

            // Update the period
            period.LastModifiedAt = DateTime.UtcNow;
            await _financialPeriodRepository.UpdateAsync(period, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task OpenPeriodAsync(string id, string modifiedBy, CancellationToken cancellationToken = default)
        {
            var period = await _financialPeriodRepository.GetByIdAsync(id, cancellationToken);
            if (period == null)
            {
                throw new InvalidOperationException($"Period with ID {id} not found");
            }

            if (period.Status != FinancialPeriodStatus.Planned && period.Status != FinancialPeriodStatus.Closed)
            {
                throw new InvalidOperationException($"Cannot open period with status {period.Status}");
            }

            // Check if fiscal year is open
            var fiscalYear = await _fiscalYearRepository.GetByIdAsync(period.FiscalYearId, cancellationToken);
            if (fiscalYear == null)
            {
                throw new InvalidOperationException($"Fiscal year with ID {period.FiscalYearId} not found");
            }

            if (fiscalYear.Status != FiscalYearStatus.Open && fiscalYear.Status != FiscalYearStatus.Active)
            {
                throw new InvalidOperationException($"Cannot open period in a fiscal year with status {fiscalYear.Status}");
            }

            // Update status
            period.Status = FinancialPeriodStatus.Open;
            period.LastModifiedBy = modifiedBy;
            period.LastModifiedAt = DateTime.UtcNow;

            await _financialPeriodRepository.UpdateAsync(period, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task ClosePeriodAsync(string id, string modifiedBy, CancellationToken cancellationToken = default)
        {
            var period = await _financialPeriodRepository.GetByIdAsync(id, cancellationToken);
            if (period == null)
            {
                throw new InvalidOperationException($"Period with ID {id} not found");
            }

            if (period.Status != FinancialPeriodStatus.Open)
            {
                throw new InvalidOperationException($"Cannot close period with status {period.Status}");
            }

            // TODO: Check if there are any unposted transactions for this period

            // Update status
            period.Status = FinancialPeriodStatus.Closed;
            period.LastModifiedBy = modifiedBy;
            period.LastModifiedAt = DateTime.UtcNow;
            period.ClosedBy = modifiedBy;
            period.ClosedDate = DateTime.UtcNow;

            await _financialPeriodRepository.UpdateAsync(period, cancellationToken);

            // Check if this is the last open period in the fiscal year
            var openPeriods = await _financialPeriodRepository.GetByFiscalYearAsync(period.FiscalYearId, cancellationToken);
            bool hasOpenPeriods = openPeriods.Any(p => p.Status == FinancialPeriodStatus.Open);

            if (!hasOpenPeriods)
            {
                // Update fiscal year status to indicate all periods are closed
                var fiscalYear = await _fiscalYearRepository.GetByIdAsync(period.FiscalYearId, cancellationToken);
                if (fiscalYear != null && fiscalYear.Status == FiscalYearStatus.Open)
                {
                    fiscalYear.Status = FiscalYearStatus.Active; // All periods closed, but fiscal year not yet closed
                    fiscalYear.LastModifiedBy = modifiedBy;
                    fiscalYear.LastModifiedAt = DateTime.UtcNow;

                    await _fiscalYearRepository.UpdateAsync(fiscalYear, cancellationToken);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Automatically open the next period if available
            var nextPeriod = await GetNextPeriodAsync(id, cancellationToken);
            if (nextPeriod != null && nextPeriod.Status == FinancialPeriodStatus.Planned)
            {
                await OpenPeriodAsync(nextPeriod.Id, modifiedBy, cancellationToken);
            }
        }

        public async Task<bool> IsPeriodValidForPostingAsync(string periodId, CancellationToken cancellationToken = default)
        {
            return await _financialPeriodRepository.IsPeriodValidForPostingAsync(periodId, cancellationToken);
        }

        public async Task<FinancialPeriod> GetNextPeriodAsync(string currentPeriodId, CancellationToken cancellationToken = default)
        {
            return await _financialPeriodRepository.GetNextPeriodAsync(currentPeriodId, cancellationToken);
        }
    }
}
