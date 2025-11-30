using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FinTech.Core.Application.Services;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Core.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace FinTech.Infrastructure.Services
{
    /// <summary>
    /// Implementation of threshold manager service
    /// </summary>
    public class ThresholdManagerService : IThresholdManager
    {
        private readonly IRepository<MonthlyThreshold> _thresholdRepository;
        private readonly IRepository<LoanApplication> _applicationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ThresholdManagerService> _logger;
        
        // Default maximum threshold (₦3,000,000)
        private const decimal DEFAULT_MAX_THRESHOLD = 3000000m;
        
        public ThresholdManagerService(
            IRepository<MonthlyThreshold> thresholdRepository,
            IRepository<LoanApplication> applicationRepository,
            IUnitOfWork unitOfWork,
            ILogger<ThresholdManagerService> logger)
        {
            _thresholdRepository = thresholdRepository ?? throw new ArgumentNullException(nameof(thresholdRepository));
            _applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Get or create threshold for a specific month
        /// </summary>
        public async Task<MonthlyThreshold> GetOrCreateThresholdAsync(int year, int month, decimal? defaultMaxAmount = null)
        {
            var threshold = (await _thresholdRepository.GetAllAsync())
                .FirstOrDefault(t => t.Year == year && t.Month == month);
            
            if (threshold == null)
            {
                var maxAmount = defaultMaxAmount ?? DEFAULT_MAX_THRESHOLD;
                threshold = new MonthlyThreshold(year, month, maxAmount);
                
                await _thresholdRepository.AddAsync(threshold);
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation(
                    "Created new threshold: Year={Year}, Month={Month}, MaxAmount={MaxAmount}",
                    year, month, maxAmount);
            }
            
            return threshold;
        }
        
        /// <summary>
        /// Check if amount can be allocated within threshold
        /// </summary>
        public async Task<ThresholdCheckResult> CheckThresholdAsync(decimal amount, int year, int month)
        {
            var threshold = await GetOrCreateThresholdAsync(year, month);
            
            var result = new ThresholdCheckResult
            {
                RequestedAmount = amount,
                AvailableAmount = threshold.RemainingAmount,
                ThresholdMaximum = threshold.MaximumAmount,
                ThresholdAllocated = threshold.AllocatedAmount,
                ThresholdRemaining = threshold.RemainingAmount,
                Status = threshold.Status,
                CanAllocate = threshold.CanAllocate(amount),
                WillBeQueued = !threshold.CanAllocate(amount)
            };
            
            if (result.CanAllocate)
            {
                result.Message = $"Amount can be allocated. Remaining after allocation: ₦{(threshold.RemainingAmount - amount):N2}";
            }
            else
            {
                if (threshold.Status == ThresholdStatus.Closed)
                {
                    result.Message = "Threshold is closed for this month";
                }
                else if (threshold.Status == ThresholdStatus.Exhausted)
                {
                    result.Message = $"Threshold exhausted. Application will be queued for next month. Shortfall: ₦{(amount - threshold.RemainingAmount):N2}";
                }
                else
                {
                    result.Message = $"Insufficient threshold. Available: ₦{threshold.RemainingAmount:N2}, Requested: ₦{amount:N2}";
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Allocate amount from threshold
        /// </summary>
        public async Task AllocateFromThresholdAsync(decimal amount, int year, int month)
        {
            var threshold = await GetOrCreateThresholdAsync(year, month);
            
            threshold.AllocateAmount(amount);
            
            await _thresholdRepository.UpdateAsync(threshold);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation(
                "Allocated from threshold: Year={Year}, Month={Month}, Amount={Amount}, Remaining={Remaining}",
                year, month, amount, threshold.RemainingAmount);
        }
        
        /// <summary>
        /// Release amount back to threshold
        /// </summary>
        public async Task ReleaseToThresholdAsync(decimal amount, int year, int month)
        {
            var threshold = await GetOrCreateThresholdAsync(year, month);
            
            threshold.ReleaseAmount(amount);
            
            await _thresholdRepository.UpdateAsync(threshold);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation(
                "Released to threshold: Year={Year}, Month={Month}, Amount={Amount}, Remaining={Remaining}",
                year, month, amount, threshold.RemainingAmount);
        }
        
        /// <summary>
        /// Update threshold maximum amount
        /// </summary>
        public async Task<MonthlyThreshold> UpdateThresholdAsync(int year, int month, decimal newMaxAmount)
        {
            var threshold = await GetOrCreateThresholdAsync(year, month);
            
            var oldMaxAmount = threshold.MaximumAmount;
            threshold.UpdateMaximumAmount(newMaxAmount);
            
            await _thresholdRepository.UpdateAsync(threshold);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation(
                "Updated threshold: Year={Year}, Month={Month}, OldMax={OldMax}, NewMax={NewMax}",
                year, month, oldMaxAmount, newMaxAmount);
            
            return threshold;
        }
        
        /// <summary>
        /// Get queued applications for a month
        /// </summary>
        public async Task<List<QueuedLoanApplication>> GetQueuedApplicationsAsync(int year, int month)
        {
            // Get applications that were approved but couldn't be registered due to threshold
            var queuedApps = (await _applicationRepository.GetAllAsync())
                .Where(a => 
                    a.Status == LoanApplicationStatus.Approved &&
                    a.ApprovalDate.HasValue &&
                    a.ApprovalDate.Value.Year == year &&
                    a.ApprovalDate.Value.Month == month)
                .OrderBy(a => a.ApprovalDate)
                .ToList();
            
            var result = new List<QueuedLoanApplication>();
            int position = 1;
            
            foreach (var app in queuedApps)
            {
                result.Add(new QueuedLoanApplication
                {
                    ApplicationId = Guid.Parse(app.Id),
                    ApplicationNumber = app.ApplicationNumber,
                    MemberId = Guid.Parse(app.CustomerId.ToString()),
                    MemberName = $"Member {app.CustomerId}", // TODO: Get actual member name
                    RequestedAmount = app.ApprovedAmount ?? app.RequestedAmount,
                    ApprovedDate = app.ApprovalDate.Value,
                    QueuedDate = app.ApprovalDate.Value,
                    QueuePosition = position++,
                    Status = "QUEUED"
                });
            }
            
            return result;
        }
        
        /// <summary>
        /// Process monthly rollover
        /// </summary>
        public async Task ProcessMonthlyRolloverAsync()
        {
            var now = DateTime.UtcNow;
            var currentYear = now.Year;
            var currentMonth = now.Month;
            
            // Get previous month
            var previousMonth = currentMonth == 1 ? 12 : currentMonth - 1;
            var previousYear = currentMonth == 1 ? currentYear - 1 : currentYear;
            
            // Get queued applications from previous month
            var queuedApps = await GetQueuedApplicationsAsync(previousYear, previousMonth);
            
            if (!queuedApps.Any())
            {
                _logger.LogInformation("No queued applications to rollover");
                return;
            }
            
            // Get or create current month threshold
            var currentThreshold = await GetOrCreateThresholdAsync(currentYear, currentMonth);
            
            int processedCount = 0;
            int queuedCount = 0;
            
            foreach (var queuedApp in queuedApps.OrderBy(a => a.QueuePosition))
            {
                // Check if can allocate in current month
                if (currentThreshold.CanAllocate(queuedApp.RequestedAmount))
                {
                    // Allocate and mark for registration
                    currentThreshold.AllocateAmount(queuedApp.RequestedAmount);
                    processedCount++;
                    
                    _logger.LogInformation(
                        "Rolled over application: {ApplicationNumber}, Amount={Amount}",
                        queuedApp.ApplicationNumber, queuedApp.RequestedAmount);
                }
                else
                {
                    // Still queued for next month
                    currentThreshold.IncrementQueuedCount();
                    queuedCount++;
                }
            }
            
            await _thresholdRepository.UpdateAsync(currentThreshold);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation(
                "Monthly rollover complete: Processed={Processed}, StillQueued={Queued}",
                processedCount, queuedCount);
        }
        
        /// <summary>
        /// Get threshold history for a year
        /// </summary>
        public async Task<List<MonthlyThreshold>> GetThresholdHistoryAsync(int year)
        {
            var thresholds = (await _thresholdRepository.GetAllAsync())
                .Where(t => t.Year == year)
                .OrderBy(t => t.Month)
                .ToList();
            
            return thresholds;
        }
        
        /// <summary>
        /// Get threshold utilization report
        /// </summary>
        public async Task<ThresholdUtilizationReport> GetUtilizationReportAsync(int year, int? month = null)
        {
            var query = (await _thresholdRepository.GetAllAsync())
                .Where(t => t.Year == year);
            
            if (month.HasValue)
            {
                query = query.Where(t => t.Month == month.Value);
            }
            
            var thresholds = query.ToList();
            
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
                    MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(threshold.Month),
                    MaximumAmount = threshold.MaximumAmount,
                    AllocatedAmount = threshold.AllocatedAmount,
                    RemainingAmount = threshold.RemainingAmount,
                    UtilizationPercentage = threshold.GetUtilizationPercentage(),
                    LoansRegistered = threshold.TotalApplicationsRegistered,
                    ApplicationsQueued = threshold.TotalApplicationsQueued,
                    Status = threshold.Status
                });
            }
            
            return report;
        }
    }
}
