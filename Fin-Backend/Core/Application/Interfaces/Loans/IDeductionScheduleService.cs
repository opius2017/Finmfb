using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Loans;

namespace FinTech.Core.Application.Interfaces.Loans
{
    /// <summary>
    /// Service for managing monthly payroll deduction schedules
    /// </summary>
    public interface IDeductionScheduleService
    {
        /// <summary>
        /// Generate deduction schedule for a specific month
        /// </summary>
        Task<DeductionScheduleDto> GenerateScheduleAsync(GenerateDeductionScheduleRequest request);

        /// <summary>
        /// Get deduction schedule by ID
        /// </summary>
        Task<DeductionScheduleDto?> GetScheduleByIdAsync(string scheduleId);

        /// <summary>
        /// Get deduction schedule by month and year
        /// </summary>
        Task<DeductionScheduleDto?> GetScheduleByMonthAsync(int month, int year);

        /// <summary>
        /// Get all deduction schedules with optional filtering
        /// </summary>
        Task<List<DeductionScheduleDto>> GetSchedulesAsync(int? year = null, string? status = null);

        /// <summary>
        /// Approve a deduction schedule
        /// </summary>
        Task<DeductionScheduleDto> ApproveScheduleAsync(ApproveDeductionScheduleRequest request);

        /// <summary>
        /// Submit schedule to payroll system
        /// </summary>
        Task<DeductionScheduleDto> SubmitScheduleAsync(SubmitDeductionScheduleRequest request);

        /// <summary>
        /// Export schedule to Excel/CSV/PDF
        /// </summary>
        Task<DeductionScheduleExportResult> ExportScheduleAsync(ExportDeductionScheduleRequest request);

        /// <summary>
        /// Cancel a deduction schedule
        /// </summary>
        Task<bool> CancelScheduleAsync(string scheduleId, string cancelledBy);

        /// <summary>
        /// Create new version of schedule
        /// </summary>
        Task<DeductionScheduleDto> CreateNewVersionAsync(string scheduleId, string createdBy);
    }
}
