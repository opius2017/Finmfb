using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Loans;

namespace FinTech.Core.Application.Interfaces.Loans
{
    /// <summary>
    /// Service for reconciling expected vs actual payroll deductions
    /// </summary>
    public interface IDeductionReconciliationService
    {
        /// <summary>
        /// Import actual deductions from payroll system
        /// </summary>
        Task<ReconciliationResult> ImportActualDeductionsAsync(ImportActualDeductionsRequest request);

        /// <summary>
        /// Perform reconciliation between expected and actual deductions
        /// </summary>
        Task<ReconciliationResult> PerformReconciliationAsync(string deductionScheduleId, string performedBy);

        /// <summary>
        /// Get reconciliation by ID
        /// </summary>
        Task<DeductionReconciliationDto?> GetReconciliationByIdAsync(string reconciliationId);

        /// <summary>
        /// Get reconciliation for a specific schedule
        /// </summary>
        Task<DeductionReconciliationDto?> GetReconciliationByScheduleAsync(string deductionScheduleId);

        /// <summary>
        /// Get all reconciliations with optional filtering
        /// </summary>
        Task<List<DeductionReconciliationDto>> GetReconciliationsAsync(int? year = null, int? month = null, string? status = null);

        /// <summary>
        /// Get variance items that need resolution
        /// </summary>
        Task<List<DeductionReconciliationItemDto>> GetVarianceItemsAsync(string reconciliationId);

        /// <summary>
        /// Resolve a variance item
        /// </summary>
        Task<bool> ResolveVarianceAsync(ResolveVarianceRequest request);

        /// <summary>
        /// Retry failed deductions
        /// </summary>
        Task<ReconciliationResult> RetryFailedDeductionsAsync(string reconciliationId, string retriedBy);

        /// <summary>
        /// Generate reconciliation report
        /// </summary>
        Task<DeductionScheduleExportResult> GenerateReconciliationReportAsync(string reconciliationId, string format = "EXCEL");

        /// <summary>
        /// Get reconciliation summary statistics
        /// </summary>
        Task<ReconciliationSummaryDto> GetReconciliationSummaryAsync(int year, int? month = null);
    }

    public class ReconciliationSummaryDto
    {
        public int TotalReconciliations { get; set; }
        public int CompletedReconciliations { get; set; }
        public int PendingReconciliations { get; set; }
        public decimal TotalExpectedAmount { get; set; }
        public decimal TotalActualAmount { get; set; }
        public decimal TotalVarianceAmount { get; set; }
        public decimal VariancePercentage { get; set; }
        public int TotalVarianceItems { get; set; }
        public int ResolvedVarianceItems { get; set; }
        public int PendingVarianceItems { get; set; }
    }
}
