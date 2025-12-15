using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Loans;

namespace FinTech.Core.Application.Interfaces.Loans
{
    /// <summary>
    /// Service for managing loan delinquency detection and tracking
    /// CBN-compliant classification system
    /// </summary>
    public interface IDelinquencyManagementService
    {
        /// <summary>
        /// Check delinquency status for a specific loan
        /// </summary>
        Task<DelinquencyCheckResult> CheckLoanDelinquencyAsync(string loanId);

        /// <summary>
        /// Perform daily delinquency check for all active loans
        /// </summary>
        Task<DailyDelinquencyCheckResult> PerformDailyDelinquencyCheckAsync();

        /// <summary>
        /// Get delinquent loans with optional filtering
        /// </summary>
        Task<List<LoanDelinquencyDto>> GetDelinquentLoansAsync(DelinquencyReportRequest request);

        /// <summary>
        /// Get delinquency summary statistics
        /// </summary>
        Task<DelinquencySummaryDto> GetDelinquencySummaryAsync();

        /// <summary>
        /// Apply penalty to overdue loan
        /// </summary>
        Task<bool> ApplyPenaltyAsync(string loanId, decimal penaltyAmount, string appliedBy);

        /// <summary>
        /// Update loan classification based on days overdue
        /// </summary>
        Task<string> UpdateLoanClassificationAsync(string loanId, int daysOverdue);

        /// <summary>
        /// Send delinquency notification to member
        /// </summary>
        Task<bool> SendDelinquencyNotificationAsync(string loanId, string notificationType);

        /// <summary>
        /// Get delinquency history for a loan
        /// </summary>
        Task<List<LoanDelinquencyDto>> GetLoanDelinquencyHistoryAsync(string loanId);

        /// <summary>
        /// Identify loans overdue by minimum days
        /// </summary>
        Task<List<LoanDelinquencyDto>> IdentifyOverdueLoansAsync(int minDaysOverdue = 1);

        /// <summary>
        /// Calculate delinquency rate
        /// </summary>
        Task<decimal> CalculateDelinquencyRateAsync();
    }
}
