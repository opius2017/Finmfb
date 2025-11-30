using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Loans;

namespace FinTech.Core.Application.Services.Loans
{
    /// <summary>
    /// Interface for monthly threshold management services
    /// </summary>
    public interface IMonthlyThresholdService
    {
        /// <summary>
        /// Sets or updates monthly threshold
        /// </summary>
        Task<MonthlyThresholdResult> SetMonthlyThresholdAsync(
            int month,
            int year,
            decimal maxLoanAmount,
            string setBy);

        /// <summary>
        /// Checks if a loan amount can be accommodated
        /// </summary>
        Task<ThresholdCheckResult> CheckThresholdAsync(
            decimal loanAmount,
            int? targetMonth = null,
            int? targetYear = null);

        /// <summary>
        /// Allocates a loan amount to the monthly threshold
        /// </summary>
        Task<ThresholdAllocationResult> AllocateLoanToThresholdAsync(
            string loanApplicationId,
            decimal loanAmount);

        /// <summary>
        /// Releases allocated amount back to threshold
        /// </summary>
        Task ReleaseThresholdAllocationAsync(
            decimal loanAmount,
            int month,
            int year);

        /// <summary>
        /// Gets threshold information for a specific month
        /// </summary>
        Task<MonthlyThresholdInfo> GetMonthlyThresholdInfoAsync(int month, int year);

        /// <summary>
        /// Performs monthly rollover
        /// </summary>
        Task<MonthlyRolloverResult> PerformMonthlyRolloverAsync();

        /// <summary>
        /// Gets threshold breach alerts
        /// </summary>
        Task<List<ThresholdAlert>> GetThresholdAlertsAsync();
    }
}
