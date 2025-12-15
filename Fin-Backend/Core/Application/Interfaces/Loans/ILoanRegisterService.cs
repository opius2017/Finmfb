using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Core.Application.DTOs.Loans;

namespace FinTech.Core.Application.Interfaces.Loans
{
    /// <summary>
    /// Service for managing the official loan register
    /// </summary>
    public interface ILoanRegisterService
    {
        /// <summary>
        /// Registers a loan with unique serial number in format LH/YYYY/NNN
        /// </summary>
        Task<LoanRegistrationResult> RegisterLoanAsync(string loanId, string registeredBy);

        /// <summary>
        /// Generates unique serial number in format LH/YYYY/NNN
        /// </summary>
        Task<string> GenerateSerialNumber();

        /// <summary>
        /// Gets the complete loan register (read-only view)
        /// </summary>
        Task<List<LoanRegisterEntry>> GetLoanRegisterAsync(DateTime? fromDate = null, DateTime? toDate = null, string? status = null);

        /// <summary>
        /// Gets loan register entry by serial number
        /// </summary>
        Task<LoanRegisterEntry> GetBySerialNumberAsync(string serialNumber);

        /// <summary>
        /// Exports loan register to CSV format
        /// </summary>
        Task<string> ExportLoanRegisterAsync(DateTime? fromDate = null, DateTime? toDate = null);

        /// <summary>
        /// Gets register statistics
        /// </summary>
        Task<LoanRegisterStatistics> GetRegisterStatisticsAsync(int? year = null);
    }
}
