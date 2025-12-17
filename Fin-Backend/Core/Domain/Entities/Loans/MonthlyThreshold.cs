using System;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Loans
{
    /// <summary>
    /// Represents monthly loan disbursement threshold configuration
    /// </summary>
    public class MonthlyThreshold : AuditableEntity
    {
        public int Year { get; private set; }
        public int Month { get; private set; }
        public decimal MaximumAmount { get; private set; }
        public decimal AllocatedAmount { get; private set; }
        public decimal RemainingAmount { get; private set; }
        public int TotalApplicationsApproved { get; private set; }
        public int TotalApplicationsRegistered { get; private set; }
        public int TotalApplicationsQueued { get; private set; }
        public ThresholdStatus Status { get; private set; }
        public DateTime? ClosedAt { get; private set; }
        public string ClosedBy { get; private set; }
        public string Notes { get; private set; }
        
        private MonthlyThreshold() 
        {
            ClosedBy = string.Empty;
            Notes = string.Empty;
        } // For EF Core
        
        public MonthlyThreshold(int year, int month, decimal maximumAmount)
        {
            if (month < 1 || month > 12)
                throw new ArgumentException("Month must be between 1 and 12", nameof(month));
            
            if (maximumAmount <= 0)
                throw new ArgumentException("Maximum amount must be greater than zero", nameof(maximumAmount));
            
            Year = year;
            Month = month;
            MaximumAmount = maximumAmount;
            AllocatedAmount = 0;
            RemainingAmount = maximumAmount;
            TotalApplicationsApproved = 0;
            TotalApplicationsRegistered = 0;
            TotalApplicationsQueued = 0;
            Status = ThresholdStatus.Open;
            ClosedBy = string.Empty;
            Notes = string.Empty;
        }
        
        /// <summary>
        /// Check if amount can be allocated within threshold
        /// </summary>
        public bool CanAllocate(decimal amount)
        {
            if (Status != ThresholdStatus.Open)
                return false;
            
            return RemainingAmount >= amount;
        }
        
        /// <summary>
        /// Allocate amount from threshold
        /// </summary>
        public void AllocateAmount(decimal amount)
        {
            if (Status != ThresholdStatus.Open)
                throw new InvalidOperationException($"Cannot allocate from {Status} threshold");
            
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero", nameof(amount));
            
            if (amount > RemainingAmount)
                throw new InvalidOperationException(
                    $"Insufficient threshold. Requested: ₦{amount:N2}, Available: ₦{RemainingAmount:N2}");
            
            AllocatedAmount += amount;
            RemainingAmount -= amount;
            TotalApplicationsRegistered++;
            
            // Check if threshold is exhausted
            if (RemainingAmount <= 0)
            {
                Status = ThresholdStatus.Exhausted;
            }
        }
        
        /// <summary>
        /// Release allocated amount (if loan is cancelled before disbursement)
        /// </summary>
        public void ReleaseAmount(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero", nameof(amount));
            
            if (amount > AllocatedAmount)
                throw new InvalidOperationException(
                    $"Cannot release more than allocated. Allocated: ₦{AllocatedAmount:N2}, Requested: ₦{amount:N2}");
            
            AllocatedAmount -= amount;
            RemainingAmount += amount;
            TotalApplicationsRegistered--;
            
            // Reopen if was exhausted
            if (Status == ThresholdStatus.Exhausted && RemainingAmount > 0)
            {
                Status = ThresholdStatus.Open;
            }
        }
        
        /// <summary>
        /// Increment approved applications count
        /// </summary>
        public void IncrementApprovedCount()
        {
            TotalApplicationsApproved++;
        }
        
        /// <summary>
        /// Increment queued applications count
        /// </summary>
        public void IncrementQueuedCount()
        {
            TotalApplicationsQueued++;
        }
        
        /// <summary>
        /// Decrement queued applications count
        /// </summary>
        public void DecrementQueuedCount()
        {
            if (TotalApplicationsQueued > 0)
                TotalApplicationsQueued--;
        }
        
        /// <summary>
        /// Update maximum amount (Super Admin only)
        /// </summary>
        public void UpdateMaximumAmount(decimal newMaximumAmount)
        {
            if (newMaximumAmount <= 0)
                throw new ArgumentException("Maximum amount must be greater than zero", nameof(newMaximumAmount));
            
            if (Status == ThresholdStatus.Closed)
                throw new InvalidOperationException("Cannot update closed threshold");
            
            decimal difference = newMaximumAmount - MaximumAmount;
            MaximumAmount = newMaximumAmount;
            RemainingAmount += difference;
            
            // Reopen if was exhausted and now has remaining amount
            if (Status == ThresholdStatus.Exhausted && RemainingAmount > 0)
            {
                Status = ThresholdStatus.Open;
            }
        }
        
        /// <summary>
        /// Close threshold for the month
        /// </summary>
        public void Close(string closedBy, string? notes = null)
        {
            if (Status == ThresholdStatus.Closed)
                throw new InvalidOperationException("Threshold is already closed");
            
            Status = ThresholdStatus.Closed;
            ClosedAt = DateTime.UtcNow;
            ClosedBy = closedBy ?? throw new ArgumentNullException(nameof(closedBy));
            Notes = notes ?? string.Empty;
        }
        
        /// <summary>
        /// Reopen threshold
        /// </summary>
        public void Reopen()
        {
            if (Status != ThresholdStatus.Closed)
                throw new InvalidOperationException("Can only reopen closed thresholds");
            
            Status = RemainingAmount > 0 ? ThresholdStatus.Open : ThresholdStatus.Exhausted;
            ClosedAt = null;
            ClosedBy = string.Empty;
        }
        
        /// <summary>
        /// Get utilization percentage
        /// </summary>
        public decimal GetUtilizationPercentage()
        {
            if (MaximumAmount == 0)
                return 0;
            
            return (AllocatedAmount / MaximumAmount) * 100;
        }
    }
    
    /// <summary>
    /// Monthly threshold status
    /// </summary>
    public enum ThresholdStatus
    {
        Open = 1,
        Exhausted = 2,
        Closed = 3
    }
}
