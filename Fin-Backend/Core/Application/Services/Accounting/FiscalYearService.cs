using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Accounting;
using FinTech.Core.Domain.Repositories.Accounting;
using FinTech.Core.Domain.Repositories;

namespace FinTech.Core.Application.Services.Accounting
{
    public interface IFiscalYearService
    {
        Task<FiscalYear> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<FiscalYear> GetCurrentFiscalYearAsync(CancellationToken cancellationToken = default);
        Task<FiscalYear> GetByYearAsync(int year, CancellationToken cancellationToken = default);
        Task<FiscalYear> GetByCodeAsync(string fiscalYearCode, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<FiscalYear>> GetActiveFiscalYearsAsync(CancellationToken cancellationToken = default);
        Task<FiscalYear> GetPreviousFiscalYearAsync(string currentFiscalYearId, CancellationToken cancellationToken = default);
        Task<FiscalYear> GetNextFiscalYearAsync(string currentFiscalYearId, CancellationToken cancellationToken = default);
        Task<string> CreateFiscalYearAsync(FiscalYear fiscalYear, CancellationToken cancellationToken = default);
        Task UpdateFiscalYearAsync(FiscalYear fiscalYear, CancellationToken cancellationToken = default);
        Task OpenFiscalYearAsync(string id, string modifiedBy, CancellationToken cancellationToken = default);
        Task CloseFiscalYearAsync(string id, string modifiedBy, CancellationToken cancellationToken = default);
        Task<bool> CanCloseFiscalYearAsync(string fiscalYearId, CancellationToken cancellationToken = default);
        Task<string> GenerateFiscalYearCodeAsync(int year, CancellationToken cancellationToken = default);
        Task<string> CreateStandardFiscalYearAsync(int year, DateTime startDate, DateTime endDate, string createdBy, bool createMonthlyPeriods, CancellationToken cancellationToken = default);
    }

    public class FiscalYearService : IFiscalYearService
    {
        private readonly IFiscalYearRepository _fiscalYearRepository;
        private readonly IFinancialPeriodRepository _financialPeriodRepository;
        private readonly IUnitOfWork _unitOfWork;

        public FiscalYearService(
            IFiscalYearRepository fiscalYearRepository,
            IFinancialPeriodRepository financialPeriodRepository,
            IUnitOfWork unitOfWork)
        {
            _fiscalYearRepository = fiscalYearRepository ?? throw new ArgumentNullException(nameof(fiscalYearRepository));
            _financialPeriodRepository = financialPeriodRepository ?? throw new ArgumentNullException(nameof(financialPeriodRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<FiscalYear> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _fiscalYearRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<FiscalYear> GetCurrentFiscalYearAsync(CancellationToken cancellationToken = default)
        {
            return await _fiscalYearRepository.GetCurrentFiscalYearAsync(cancellationToken);
        }

        public async Task<FiscalYear> GetByYearAsync(int year, CancellationToken cancellationToken = default)
        {
            return await _fiscalYearRepository.GetByYearAsync(year, cancellationToken);
        }

        public async Task<FiscalYear> GetByCodeAsync(string fiscalYearCode, CancellationToken cancellationToken = default)
        {
            return await _fiscalYearRepository.GetByCodeAsync(fiscalYearCode, cancellationToken);
        }

        public async Task<IReadOnlyList<FiscalYear>> GetActiveFiscalYearsAsync(CancellationToken cancellationToken = default)
        {
            return await _fiscalYearRepository.GetActiveFiscalYearsAsync(cancellationToken);
        }

        public async Task<FiscalYear> GetPreviousFiscalYearAsync(string currentFiscalYearId, CancellationToken cancellationToken = default)
        {
            return await _fiscalYearRepository.GetPreviousFiscalYearAsync(currentFiscalYearId, cancellationToken);
        }

        public async Task<FiscalYear> GetNextFiscalYearAsync(string currentFiscalYearId, CancellationToken cancellationToken = default)
        {
            return await _fiscalYearRepository.GetNextFiscalYearAsync(currentFiscalYearId, cancellationToken);
        }

        public async Task<string> CreateFiscalYearAsync(FiscalYear fiscalYear, CancellationToken cancellationToken = default)
        {
            // Validate fiscal year
            if (fiscalYear.Year <= 0)
            {
                throw new ArgumentException("Fiscal year must be a positive number");
            }

            if (fiscalYear.StartDate >= fiscalYear.EndDate)
            {
                throw new ArgumentException("Start date must be before end date");
            }

            // Check if fiscal year already exists
            var existingFiscalYear = await _fiscalYearRepository.GetByYearAsync(fiscalYear.Year, cancellationToken);
            if (existingFiscalYear != null)
            {
                throw new InvalidOperationException($"Fiscal year {fiscalYear.Year} already exists");
            }

            // Set default values if not provided
            if (string.IsNullOrEmpty(fiscalYear.Id))
            {
                fiscalYear.Id = Guid.NewGuid().ToString();
            }

            if (string.IsNullOrEmpty(fiscalYear.Code))
            {
                fiscalYear.Code = await GenerateFiscalYearCodeAsync(fiscalYear.Year, cancellationToken);
            }

            if (string.IsNullOrEmpty(fiscalYear.Name))
            {
                fiscalYear.Name = $"Fiscal Year {fiscalYear.Year}";
            }

            fiscalYear.CreatedAt = DateTime.UtcNow;
            fiscalYear.Status = fiscalYear.Status == FiscalYearStatus.Undefined ? FiscalYearStatus.Planned : fiscalYear.Status;

            // Add the fiscal year
            await _fiscalYearRepository.AddAsync(fiscalYear, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return fiscalYear.Id;
        }

        public async Task UpdateFiscalYearAsync(FiscalYear fiscalYear, CancellationToken cancellationToken = default)
        {
            // Validate fiscal year
            if (fiscalYear.Year <= 0)
            {
                throw new ArgumentException("Fiscal year must be a positive number");
            }

            if (fiscalYear.StartDate >= fiscalYear.EndDate)
            {
                throw new ArgumentException("Start date must be before end date");
            }

            // Get the existing fiscal year
            var existingFiscalYear = await _fiscalYearRepository.GetByIdAsync(fiscalYear.Id, cancellationToken);
            if (existingFiscalYear == null)
            {
                throw new InvalidOperationException($"Fiscal year with ID {fiscalYear.Id} not found");
            }

            // Only allow updates to planned fiscal years
            if (existingFiscalYear.Status != FiscalYearStatus.Planned)
            {
                throw new InvalidOperationException($"Cannot update fiscal year with status {existingFiscalYear.Status}");
            }

            // Update the fiscal year
            fiscalYear.LastModifiedAt = DateTime.UtcNow;
            await _fiscalYearRepository.UpdateAsync(fiscalYear, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task OpenFiscalYearAsync(string id, string modifiedBy, CancellationToken cancellationToken = default)
        {
            var fiscalYear = await _fiscalYearRepository.GetByIdAsync(id, cancellationToken);
            if (fiscalYear == null)
            {
                throw new InvalidOperationException($"Fiscal year with ID {id} not found");
            }

            if (fiscalYear.Status != FiscalYearStatus.Planned)
            {
                throw new InvalidOperationException($"Cannot open fiscal year with status {fiscalYear.Status}");
            }

            // Update status
            fiscalYear.Status = FiscalYearStatus.Open;
            fiscalYear.LastModifiedBy = modifiedBy;
            fiscalYear.LastModifiedAt = DateTime.UtcNow;

            await _fiscalYearRepository.UpdateAsync(fiscalYear, cancellationToken);

            // Also open the first financial period
            var firstPeriod = fiscalYear.FinancialPeriods
                .OrderBy(p => p.StartDate)
                .FirstOrDefault();

            if (firstPeriod != null)
            {
                firstPeriod.Status = FinancialPeriodStatus.Open;
                firstPeriod.LastModifiedBy = modifiedBy;
                firstPeriod.LastModifiedAt = DateTime.UtcNow;

                await _financialPeriodRepository.UpdateAsync(firstPeriod, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task CloseFiscalYearAsync(string id, string modifiedBy, CancellationToken cancellationToken = default)
        {
            var fiscalYear = await _fiscalYearRepository.GetByIdAsync(id, cancellationToken);
            if (fiscalYear == null)
            {
                throw new InvalidOperationException($"Fiscal year with ID {id} not found");
            }

            if (fiscalYear.Status != FiscalYearStatus.Open && fiscalYear.Status != FiscalYearStatus.Active)
            {
                throw new InvalidOperationException($"Cannot close fiscal year with status {fiscalYear.Status}");
            }

            // Check if fiscal year can be closed
            if (!await CanCloseFiscalYearAsync(id, cancellationToken))
            {
                throw new InvalidOperationException($"Fiscal year cannot be closed. All periods must be closed and there must be no pending journal entries.");
            }

            // Update status
            fiscalYear.Status = FiscalYearStatus.Closed;
            fiscalYear.LastModifiedBy = modifiedBy;
            fiscalYear.LastModifiedAt = DateTime.UtcNow;
            fiscalYear.ClosedBy = modifiedBy;
            fiscalYear.ClosedAt = DateTime.UtcNow;

            await _fiscalYearRepository.UpdateAsync(fiscalYear, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> CanCloseFiscalYearAsync(string fiscalYearId, CancellationToken cancellationToken = default)
        {
            return await _fiscalYearRepository.CanCloseFiscalYearAsync(fiscalYearId, cancellationToken);
        }

        public async Task<string> GenerateFiscalYearCodeAsync(int year, CancellationToken cancellationToken = default)
        {
            return await _fiscalYearRepository.GenerateFiscalYearCodeAsync(year, cancellationToken);
        }

        public async Task<string> CreateStandardFiscalYearAsync(
            int year, 
            DateTime startDate, 
            DateTime endDate, 
            string createdBy, 
            bool createMonthlyPeriods = true, 
            CancellationToken cancellationToken = default)
        {
            // Create the fiscal year
            var fiscalYear = new FiscalYear
            {
                Id = Guid.NewGuid().ToString(),
                Year = year,
                Name = $"Fiscal Year {year}",
                StartDate = startDate.Date,
                EndDate = endDate.Date,
                Status = FiscalYearStatus.Planned,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };

            fiscalYear.Code = await GenerateFiscalYearCodeAsync(year, cancellationToken);

            // Create financial periods
            fiscalYear.FinancialPeriods = new List<FinancialPeriod>();

            if (createMonthlyPeriods)
            {
                // Create monthly periods
                var currentDate = startDate.Date;
                int periodNumber = 1;

                while (currentDate <= endDate)
                {
                    var periodEndDate = new DateTime(currentDate.Year, currentDate.Month, DateTime.DaysInMonth(currentDate.Year, currentDate.Month));
                    if (periodEndDate > endDate)
                    {
                        periodEndDate = endDate;
                    }

                    fiscalYear.FinancialPeriods.Add(new FinancialPeriod
                    {
                        Id = Guid.NewGuid().ToString(),
                        FiscalYearId = fiscalYear.Id,
                        Name = $"Period {periodNumber:D2} - {currentDate:MMM yyyy}",
                        StartDate = currentDate,
                        EndDate = periodEndDate,
                        Status = FinancialPeriodStatus.Planned,
                        CreatedBy = createdBy,
                        CreatedAt = DateTime.UtcNow
                    });

                    // Move to next month
                    currentDate = periodEndDate.AddDays(1);
                    periodNumber++;
                }
            }
            else
            {
                // Create a single period for the whole fiscal year
                fiscalYear.FinancialPeriods.Add(new FinancialPeriod
                {
                    Id = Guid.NewGuid().ToString(),
                    FiscalYearId = fiscalYear.Id,
                    Name = $"Annual Period {year}",
                    StartDate = startDate,
                    EndDate = endDate,
                    Status = FinancialPeriodStatus.Planned,
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Save the fiscal year and periods
            await _fiscalYearRepository.AddAsync(fiscalYear, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return fiscalYear.Id;
        }
    }
}
