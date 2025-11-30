using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Loans;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Core.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace FinTech.Core.Application.Services.Loans
{
    /// <summary>
    /// Service for managing monthly loan disbursement thresholds for liquidity control
    /// </summary>
    public class MonthlyThresholdService : IMonthlyThresholdService
    {
        private readonly IRepository<MonthlyThreshold> _thresholdRepository;
        private readonly IRepository<LoanApplication> _loanApplicationRepository;
        private readonly IRepository<Loan> _loanRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MonthlyThresholdService> _logger;
        private const decimal MAX_THRESHOLD_LIMIT = 3000000m; // ₦3,000,000

        public MonthlyThresholdService(
            IRepository<MonthlyThreshold> thresholdRepository,
            IRepository<LoanApplication> loanApplicationRepository,
            IRepository<Loan> loanRepository,
            IUnitOfWork unitOfWork,
            ILogger<MonthlyThresholdService> logger)
        {
            _thresholdRepository = thresholdRepository;
            _loanApplicationRepository = loanApplicationRepository;
            _loanRepository = loanRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Sets or updates monthly threshold for a specific month
        /// </summary>
        public async Task<MonthlyThresholdResult> SetMonthlyThresholdAsync(
            int month,
            int year,
            decimal maxLoanAmount,
            string setBy)
        {
            _logger.LogInformation(
                "Setting monthly threshold for {Month}/{Year}: ₦{Amount:N2}",
                month, year, maxLoanAmount);

            // Validate inputs
            if (month < 1 || month > 12)
                throw new ArgumentException("Month must be between 1 and 12", nameof(month));

            if (year < 2020 || year > 2100)
                throw new ArgumentException("Invalid year", nameof(year));

            if (maxLoanAmount <= 0)
                throw new ArgumentException("Threshold amount must be greater than zero", nameof(maxLoanAmount));

            if (maxLoanAmount > MAX_THRESHOLD_LIMIT)
                throw new ArgumentException(
                    $"Threshold cannot exceed ₦{MAX_THRESHOLD_LIMIT:N2}", nameof(maxLoanAmount));

            // Check if threshold already exists
            var existing = (await _thresholdRepository.GetAllAsync())
                .FirstOrDefault(t => t.Month == month && t.Year == year);

            if (existing != null)
            {
                // Update existing threshold
                existing.MaxLoanAmount = maxLoanAmount;
                existing.RemainingAmount = maxLoanAmount - existing.TotalDisbursed;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = setBy;

                await _thresholdRepository.UpdateAsync(existing);
            }
            else
            {
                // Create new threshold
                var threshold = new MonthlyThreshold
                {
                    Id = Guid.NewGuid().ToString(),
                    Month = month,
                    Year = year,
                    MaxLoanAmount = maxLoanAmount,
                    TotalDisbursed = 0,
                    RemainingAmount = maxLoanAmount,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = setBy
                };

                await _thresholdRepository.AddAsync(threshold);
                existing = threshold;
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Monthly threshold set successfully for {Month}/{Year}",
                month, year);

            return new MonthlyThresholdResult
            {
                ThresholdId = existing.Id,
                Month = month,
                Year = year,
                MaxLoanAmount = maxLoanAmount,
                TotalDisbursed = existing.TotalDisbursed,
                RemainingAmount = existing.RemainingAmount,
                Message = "Monthly threshold set successfully"
            };
        }

        /// <summary>
        /// Checks if a loan amount can be accommodated within the monthly threshold
        /// </summary>
        public async Task<ThresholdCheckResult> CheckThresholdAsync(
            decimal loanAmount,
            int? targetMonth = null,
            int? targetYear = null)
        {
            // Default to current month if not specified
            int month = targetMonth ?? DateTime.UtcNow.Month;
            int year = targetYear ?? DateTime.UtcNow.Year;

            _logger.LogInformation(
                "Checking threshold for ₦{Amount:N2} in {Month}/{Year}",
                loanAmount, month, year);

            var result = new ThresholdCheckResult
            {
                Month = month,
                Year = year,
                RequestedAmount = loanAmount
            };

            // Get threshold for the month
            var threshold = await GetOrCreateThresholdAsync(month, year);

            result.MaxLoanAmount = threshold.MaxLoanAmount;
            result.TotalDisbursed = threshold.TotalDisbursed;
            result.RemainingAmount = threshold.RemainingAmount;

            // Check if amount fits within remaining threshold
            if (loanAmount <= threshold.RemainingAmount)
            {
                result.CanAccommodate = true;
                result.Message = $"Loan amount can be accommodated in {month}/{year}";
            }
            else
            {
                result.CanAccommodate = false;
                result.Shortfall = loanAmount - threshold.RemainingAmount;
                result.Message = $"Insufficient threshold. Shortfall: ₦{result.Shortfall:N2}";

                // Suggest next available month
                result.SuggestedMonth = await FindNextAvailableMonthAsync(loanAmount, month, year);
            }

            return result;
        }

        /// <summary>
        /// Allocates a loan amount to the monthly threshold
        /// </summary>
        public async Task<ThresholdAllocationResult> AllocateLoanToThresholdAsync(
            string loanApplicationId,
            decimal loanAmount)
        {
            _logger.LogInformation(
                "Allocating loan {ApplicationId} amount ₦{Amount:N2} to threshold",
                loanApplicationId, loanAmount);

            var application = await _loanApplicationRepository.GetByIdAsync(loanApplicationId);
            if (application == null)
                throw new InvalidOperationException("Loan application not found");

            // Try current month first
            int currentMonth = DateTime.UtcNow.Month;
            int currentYear = DateTime.UtcNow.Year;

            var checkResult = await CheckThresholdAsync(loanAmount, currentMonth, currentYear);

            int allocatedMonth = currentMonth;
            int allocatedYear = currentYear;

            if (!checkResult.CanAccommodate)
            {
                // Find next available month
                var nextAvailable = await FindNextAvailableMonthAsync(loanAmount, currentMonth, currentYear);
                if (nextAvailable == null)
                {
                    return new ThresholdAllocationResult
                    {
                        Success = false,
                        Message = "No available threshold capacity in the next 12 months",
                        QueuedForMonth = null,
                        QueuedForYear = null
                    };
                }

                allocatedMonth = nextAvailable.Value.Month;
                allocatedYear = nextAvailable.Value.Year;
            }

            // Update threshold
            var threshold = await GetOrCreateThresholdAsync(allocatedMonth, allocatedYear);
            threshold.TotalDisbursed += loanAmount;
            threshold.RemainingAmount -= loanAmount;
            threshold.UpdatedAt = DateTime.UtcNow;

            await _thresholdRepository.UpdateAsync(threshold);

            // Update application with queued month if not current month
            if (allocatedMonth != currentMonth || allocatedYear != currentYear)
            {
                application.CommitteeReviewStatus = "QUEUED";
                application.UpdatedAt = DateTime.UtcNow;
                await _loanApplicationRepository.UpdateAsync(application);
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Loan allocated to {Month}/{Year} threshold",
                allocatedMonth, allocatedYear);

            return new ThresholdAllocationResult
            {
                Success = true,
                AllocatedAmount = loanAmount,
                AllocatedMonth = allocatedMonth,
                AllocatedYear = allocatedYear,
                QueuedForMonth = allocatedMonth != currentMonth ? allocatedMonth : null,
                QueuedForYear = allocatedMonth != currentMonth ? allocatedYear : null,
                Message = allocatedMonth == currentMonth
                    ? "Loan allocated to current month"
                    : $"Loan queued for {allocatedMonth}/{allocatedYear}"
            };
        }

        /// <summary>
        /// Releases allocated amount back to threshold (e.g., when loan is rejected)
        /// </summary>
        public async Task ReleaseThresholdAllocationAsync(
            decimal loanAmount,
            int month,
            int year)
        {
            _logger.LogInformation(
                "Releasing ₦{Amount:N2} back to {Month}/{Year} threshold",
                loanAmount, month, year);

            var threshold = await GetOrCreateThresholdAsync(month, year);

            threshold.TotalDisbursed -= loanAmount;
            threshold.RemainingAmount += loanAmount;
            threshold.UpdatedAt = DateTime.UtcNow;

            await _thresholdRepository.UpdateAsync(threshold);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Threshold allocation released successfully");
        }

        /// <summary>
        /// Gets threshold information for a specific month
        /// </summary>
        public async Task<MonthlyThresholdInfo> GetMonthlyThresholdInfoAsync(int month, int year)
        {
            var threshold = await GetOrCreateThresholdAsync(month, year);

            var info = new MonthlyThresholdInfo
            {
                Month = month,
                Year = year,
                MaxLoanAmount = threshold.MaxLoanAmount,
                TotalDisbursed = threshold.TotalDisbursed,
                RemainingAmount = threshold.RemainingAmount,
                UtilizationPercentage = threshold.MaxLoanAmount > 0
                    ? Math.Round((threshold.TotalDisbursed / threshold.MaxLoanAmount) * 100, 2)
                    : 0,
                IsActive = threshold.IsActive
            };

            // Get queued applications for this month
            info.QueuedApplicationsCount = await GetQueuedApplicationsCountAsync(month, year);

            return info;
        }

        /// <summary>
        /// Performs monthly rollover - processes queued applications for the new month
        /// </summary>
        public async Task<MonthlyRolloverResult> PerformMonthlyRolloverAsync()
        {
            int currentMonth = DateTime.UtcNow.Month;
            int currentYear = DateTime.UtcNow.Year;

            _logger.LogInformation(
                "Performing monthly rollover for {Month}/{Year}",
                currentMonth, currentYear);

            var result = new MonthlyRolloverResult
            {
                Month = currentMonth,
                Year = currentYear,
                ProcessedApplications = new List<string>()
            };

            // Get queued applications for this month
            var queuedApplications = (await _loanApplicationRepository.GetAllAsync())
                .Where(a => a.CommitteeReviewStatus == "QUEUED" &&
                           a.ApplicationStatus == "APPROVED")
                .OrderBy(a => a.ApprovedAt)
                .ToList();

            var threshold = await GetOrCreateThresholdAsync(currentMonth, currentYear);

            foreach (var application in queuedApplications)
            {
                if (application.ApprovedAmount <= threshold.RemainingAmount)
                {
                    // Process application
                    application.CommitteeReviewStatus = "READY_FOR_DISBURSEMENT";
                    application.UpdatedAt = DateTime.UtcNow;
                    await _loanApplicationRepository.UpdateAsync(application);

                    result.ProcessedApplications.Add(application.Id);
                    result.ProcessedCount++;
                }
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Monthly rollover completed. Processed {Count} applications",
                result.ProcessedCount);

            result.Message = $"Processed {result.ProcessedCount} queued applications";
            return result;
        }

        /// <summary>
        /// Gets threshold breach alerts
        /// </summary>
        public async Task<List<ThresholdAlert>> GetThresholdAlertsAsync()
        {
            var alerts = new List<ThresholdAlert>();
            int currentMonth = DateTime.UtcNow.Month;
            int currentYear = DateTime.UtcNow.Year;

            // Check current month threshold
            var threshold = await GetOrCreateThresholdAsync(currentMonth, currentYear);

            decimal utilizationPercentage = threshold.MaxLoanAmount > 0
                ? (threshold.TotalDisbursed / threshold.MaxLoanAmount) * 100
                : 0;

            if (utilizationPercentage >= 90)
            {
                alerts.Add(new ThresholdAlert
                {
                    Month = currentMonth,
                    Year = currentYear,
                    Severity = "CRITICAL",
                    Message = $"Threshold utilization at {utilizationPercentage:F2}% for {currentMonth}/{currentYear}",
                    RemainingAmount = threshold.RemainingAmount
                });
            }
            else if (utilizationPercentage >= 75)
            {
                alerts.Add(new ThresholdAlert
                {
                    Month = currentMonth,
                    Year = currentYear,
                    Severity = "WARNING",
                    Message = $"Threshold utilization at {utilizationPercentage:F2}% for {currentMonth}/{currentYear}",
                    RemainingAmount = threshold.RemainingAmount
                });
            }

            return alerts;
        }

        #region Helper Methods

        private async Task<MonthlyThreshold> GetOrCreateThresholdAsync(int month, int year)
        {
            var threshold = (await _thresholdRepository.GetAllAsync())
                .FirstOrDefault(t => t.Month == month && t.Year == year);

            if (threshold == null)
            {
                // Create default threshold
                threshold = new MonthlyThreshold
                {
                    Id = Guid.NewGuid().ToString(),
                    Month = month,
                    Year = year,
                    MaxLoanAmount = MAX_THRESHOLD_LIMIT,
                    TotalDisbursed = 0,
                    RemainingAmount = MAX_THRESHOLD_LIMIT,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "SYSTEM"
                };

                await _thresholdRepository.AddAsync(threshold);
                await _unitOfWork.SaveChangesAsync();
            }

            return threshold;
        }

        private async Task<(int Month, int Year)?> FindNextAvailableMonthAsync(
            decimal requiredAmount,
            int startMonth,
            int startYear)
        {
            for (int i = 1; i <= 12; i++)
            {
                int checkMonth = startMonth + i;
                int checkYear = startYear;

                if (checkMonth > 12)
                {
                    checkMonth -= 12;
                    checkYear++;
                }

                var threshold = await GetOrCreateThresholdAsync(checkMonth, checkYear);

                if (threshold.RemainingAmount >= requiredAmount)
                {
                    return (checkMonth, checkYear);
                }
            }

            return null;
        }

        private async Task<int> GetQueuedApplicationsCountAsync(int month, int year)
        {
            // TODO: Implement proper queued applications tracking
            return await Task.FromResult(0);
        }

        #endregion
    }
}
