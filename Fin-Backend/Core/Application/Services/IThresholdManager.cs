using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Loans;

namespace FinTech.Core.Application.Services
{
    /// <summary>
    /// Service for managing monthly loan thresholds
    /// </summary>
    public interface IThresholdManager
    {
        /// <summary>
        /// Get or create threshold for a specific month
        /// </summary>
        Task<MonthlyThreshold> GetOrCreateThresholdAsync(int year, int month, decimal? defaultMaxAmount = null);
        
        /// <summary>
        /// Check if amount can be allocated within threshold
        /// </summary>
        Task<ThresholdCheckResult> CheckThresholdAsync(decimal amount, int year, int month);
        
        /// <summary>
        /// Allocate amount from threshold (when registering loan)
        /// </summary>
        Task AllocateFromThresholdAsync(decimal amount, int year, int month);
        
        /// <summary>
        /// Release amount back to threshold (if loan cancelled)
        /// </summary>
        Task ReleaseToThresholdAsync(decimal amount, int year, int month);
        
        /// <summary>
        /// Update threshold maximum amount
        /// </summary>
        Task<MonthlyThreshold> UpdateThresholdAsync(int year, int month, decimal newMaxAmount);
        
        /// <summary>
        /// Get queued applications for a month
        /// </summary>
        Task<List<QueuedLoanApplication>> GetQueuedApplicationsAsync(int year, int month);
        
        /// <summary>
        /// Process monthly rollover (move queued applications to next month)
        /// </summary>
        Task ProcessMonthlyRolloverAsync();
        
        /// <summary>
        /// Get threshold history
        /// </summary>
        Task<List<MonthlyThreshold>> GetThresholdHistoryAsync(int year);
        
        /// <summary>
        /// Get threshold utilization report
        /// </summary>
        Task<ThresholdUtilizationReport> GetUtilizationReportAsync(int year, int? month = null);
    }
    
    /// <summary>
    /// Result of threshold check
    /// </summary>
    public class ThresholdCheckResult
    {
        public bool CanAllocate { get; set; }
        public decimal RequestedAmount { get; set; }
        public decimal AvailableAmount { get; set; }
        public decimal ThresholdMaximum { get; set; }
        public decimal ThresholdAllocated { get; set; }
        public decimal ThresholdRemaining { get; set; }
        public ThresholdStatus Status { get; set; }
        public string Message { get; set; }
        public bool WillBeQueued { get; set; }
    }
    
    /// <summary>
    /// Queued loan application
    /// </summary>
    public class QueuedLoanApplication
    {
        public Guid ApplicationId { get; set; }
        public string ApplicationNumber { get; set; }
        public Guid MemberId { get; set; }
        public string MemberName { get; set; }
        public decimal RequestedAmount { get; set; }
        public DateTime ApprovedDate { get; set; }
        public DateTime QueuedDate { get; set; }
        public int QueuePosition { get; set; }
        public string Status { get; set; }
    }
    
    /// <summary>
    /// Threshold utilization report
    /// </summary>
    public class ThresholdUtilizationReport
    {
        public int Year { get; set; }
        public int? Month { get; set; }
        public decimal TotalThresholdAmount { get; set; }
        public decimal TotalAllocatedAmount { get; set; }
        public decimal TotalRemainingAmount { get; set; }
        public decimal AverageUtilizationPercentage { get; set; }
        public int TotalLoansRegistered { get; set; }
        public int TotalApplicationsQueued { get; set; }
        public List<MonthlyUtilization> MonthlyBreakdown { get; set; } = new List<MonthlyUtilization>();
    }
    
    /// <summary>
    /// Monthly utilization details
    /// </summary>
    public class MonthlyUtilization
    {
        public int Month { get; set; }
        public string MonthName { get; set; }
        public decimal MaximumAmount { get; set; }
        public decimal AllocatedAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal UtilizationPercentage { get; set; }
        public int LoansRegistered { get; set; }
        public int ApplicationsQueued { get; set; }
        public ThresholdStatus Status { get; set; }
    }
}
