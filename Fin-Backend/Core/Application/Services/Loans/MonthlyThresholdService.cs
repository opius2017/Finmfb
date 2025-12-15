using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Loans;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Core.Domain.Enums.Loans;
using FinTech.Core.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using FinTech.Core.Application.Interfaces.Loans;

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
            var existing = await _thresholdRepository.GetAll()
                .FirstOrDefaultAsync(t => t.Month == month && t.Year == year);

            if (existing != null)
            {
                // Update existing threshold
                existing.UpdateMaximumAmount(maxLoanAmount);
                existing.LastModifiedBy = setBy;

                await _thresholdRepository.UpdateAsync(existing);
            }
            else
            {
                // Create new threshold
                var threshold = new MonthlyThreshold(year, month, maxLoanAmount);
                threshold.CreatedBy = setBy;
                threshold.CreatedAt = DateTime.UtcNow;

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
                TotalDisbursed = existing.AllocatedAmount,
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

            result.MaxLoanAmount = threshold.MaximumAmount;
            result.TotalDisbursed = threshold.AllocatedAmount;
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
            // Update threshold
            var threshold = await GetOrCreateThresholdAsync(allocatedMonth, allocatedYear);
            threshold.AllocateAmount(loanAmount);

            await _thresholdRepository.UpdateAsync(threshold);

            // Update application with queued month if not current month
            if (allocatedMonth != currentMonth || allocatedYear != currentYear)
            {
                application.InternalNotes = $"Queued for {allocatedMonth}/{allocatedYear}";
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
            threshold.ReleaseAmount(loanAmount);

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
                MaxLoanAmount = threshold.MaximumAmount,
                TotalDisbursed = threshold.AllocatedAmount,
                RemainingAmount = threshold.RemainingAmount,
                UtilizationPercentage = threshold.GetUtilizationPercentage(),
                IsActive = threshold.Status == ThresholdStatus.Open
            };

            // Get queued applications for this month
            info.QueuedApplicationsCount = await GetQueuedApplicationsCountAsync(month, year);

            return info;
        }

        // FinTech Best Practice: Helper method to get queued applications count
        private async Task<int> GetQueuedApplicationsCountAsync(int month, int year)
        {
            var applications = await _loanApplicationRepository.GetAllAsync();
            return applications.Count(a => 
                a.Status == LoanApplicationStatus.Approved && // FinTech Best Practice: Compare enum to enum
                a.ApprovedDate.HasValue &&
                a.ApprovedDate.Value.Month == month && 
                a.ApprovedDate.Value.Year == year);
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
            var queuedApplications = await _loanApplicationRepository.GetAll()
                .Where(a => a.InternalNotes != null && a.InternalNotes.Contains("Queued") &&
                           a.Status == LoanApplicationStatus.Approved)
                .ToListAsync();
            
            queuedApplications = queuedApplications.OrderBy(a => a.ApprovedDate).ToList();

            var threshold = await GetOrCreateThresholdAsync(currentMonth, currentYear);

            foreach (var application in queuedApplications)
            {
                if (application.ApprovedAmount <= threshold.RemainingAmount)
                {
                    // Process application
                    application.InternalNotes = "Released from queue - Ready for disbursement";
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

            decimal utilizationPercentage = threshold.GetUtilizationPercentage();

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

        /// <summary>
        /// Gets queued applications for a specific month
        /// </summary>
        public async Task<List<QueuedLoanApplicationDto>> GetQueuedApplicationsAsync(int month, int year)
        {
            var result = new List<QueuedLoanApplicationDto>();

            // Get queued applications (Approved but not processed or explicitly Queued)
            // Logic adapted to find applications queued for the specific month
            var queuedApps = (await _loanApplicationRepository.GetAllAsync())
                .Where(a => 
                    a.Status == LoanApplicationStatus.Approved &&
                     (a.InternalNotes != null && a.InternalNotes.Contains($"Queued for {month}/{year}"))
                )
                .OrderBy(a => a.ApprovedDate)
                .ToList();
            
            // If checking past/present, might also include general queued items?
            // For now, strict matching based on InternalNotes or ApprovalDate if intended logic was "Approved in that month but blocked"
            // Infrastructure logic filtered by ApprovalDate.Value.Year/Month.
            // Let's support both: explicitly queued OR approved in that month.
            
            if (!queuedApps.Any())
            {
                 queuedApps = (await _loanApplicationRepository.GetAllAsync())
                    .Where(a => 
                        a.Status == LoanApplicationStatus.Approved &&
                        a.ApprovedDate.HasValue &&
                        a.ApprovedDate.Value.Month == month &&
                        a.ApprovedDate.Value.Year == year)
                    .OrderBy(a => a.ApprovedDate)
                    .ToList();
            }

            int position = 1;
            foreach (var app in queuedApps)
            {
                var member = await _unitOfWork.Repository<Domain.Entities.Customers.Customer>().GetByIdAsync(app.MemberId); // FinTech Best Practice: Use Customer entity

                result.Add(new QueuedLoanApplicationDto
                {
                    ApplicationId = Guid.Parse(app.Id),
                    ApplicationNumber = app.ApplicationNumber,
                    MemberId = Guid.Parse(app.CustomerId.ToString()), // Assuming CustomerId maps to MemberId guid
                    MemberName = member != null ? $"{member.FirstName} {member.LastName}" : $"Member {app.CustomerId}",
                    RequestedAmount = app.ApprovedAmount ?? app.RequestedAmount,
                    ApprovedDate = app.ApprovedDate ?? app.SubmittedAt ?? DateTime.UtcNow, // FinTech Best Practice: Handle nullable DateTime
                    QueuedDate = app.ApprovedDate ?? app.SubmittedAt ?? DateTime.UtcNow, // FinTech Best Practice: Handle nullable DateTime
                    QueuePosition = position++,
                    Status = "QUEUED"
                });
            }

            return result;
        }

        #region Helper Methods

        private async Task<MonthlyThreshold> GetOrCreateThresholdAsync(int month, int year)
        {
            var threshold = await _thresholdRepository.GetAll()
                .FirstOrDefaultAsync(t => t.Month == month && t.Year == year);

            if (threshold == null)
            {
                // Create default threshold
                threshold = new MonthlyThreshold(year, month, MAX_THRESHOLD_LIMIT);
                threshold.CreatedBy = "SYSTEM";

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



        public async Task<List<MonthlyThresholdInfo>> GetThresholdHistoryAsync(int year)
        {
            var thresholds = await _thresholdRepository.GetAll()
                .Where(t => t.Year == year)
                .OrderBy(t => t.Month)
                .ToListAsync();

            var result = new List<MonthlyThresholdInfo>();
            foreach (var t in thresholds)
            {
                result.Add(new MonthlyThresholdInfo
                {
                    Month = t.Month,
                    Year = t.Year,
                    MaxLoanAmount = t.MaximumAmount,
                    TotalDisbursed = t.AllocatedAmount,
                    RemainingAmount = t.RemainingAmount,
                    UtilizationPercentage = t.GetUtilizationPercentage(),
                    IsActive = t.Status == ThresholdStatus.Open,
                    // Queued count requires extra query, doing simplified mapping for history list
                    QueuedApplicationsCount = t.TotalApplicationsQueued
                });
            }
            return result;
        }

        public async Task<ThresholdUtilizationReport> GetUtilizationReportAsync(int year, int? month = null)
        {
             var query = _thresholdRepository.GetAll()
                .Where(t => t.Year == year);
            
            if (month.HasValue)
            {
                query = query.Where(t => t.Month == month.Value);
            }
            
            var thresholds = await query.ToListAsync();
            
            var report = new ThresholdUtilizationReport
            {
                Year = year,
                Month = month,
                TotalThresholdAmount = thresholds.Sum(t => t.MaximumAmount),
                TotalAllocatedAmount = thresholds.Sum(t => t.AllocatedAmount),
                TotalRemainingAmount = thresholds.Sum(t => t.RemainingAmount),
                TotalLoansRegistered = thresholds.Sum(t => t.TotalApplicationsRegistered),
                TotalApplicationsQueued = thresholds.Sum(t => t.TotalApplicationsQueued)
            };
            
            if (report.TotalThresholdAmount > 0)
            {
                report.AverageUtilizationPercentage = 
                    (report.TotalAllocatedAmount / report.TotalThresholdAmount) * 100;
            }
            
            // Monthly breakdown
            foreach (var threshold in thresholds.OrderBy(t => t.Month))
            {
                report.MonthlyBreakdown.Add(new MonthlyUtilization
                {
                    Month = threshold.Month,
                    MonthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(threshold.Month),
                    MaximumAmount = threshold.MaximumAmount,
                    AllocatedAmount = threshold.AllocatedAmount,
                    RemainingAmount = threshold.RemainingAmount,
                    UtilizationPercentage = threshold.GetUtilizationPercentage(),
                    LoansRegistered = threshold.TotalApplicationsRegistered,
                    ApplicationsQueued = threshold.TotalApplicationsQueued,
                    Status = threshold.Status.ToString()
                });
            }
            
            return report;
        }

        #endregion
    }
}
