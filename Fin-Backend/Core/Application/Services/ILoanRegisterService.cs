using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Loans;

namespace FinTech.Core.Application.Services
{
    /// <summary>
    /// Service for managing the official loan register
    /// </summary>
    public interface ILoanRegisterService
    {
        /// <summary>
        /// Register an approved loan in the official register
        /// </summary>
        Task<LoanRegister> RegisterLoanAsync(RegisterLoanCommand command);
        
        /// <summary>
        /// Generate next serial number for a loan
        /// Format: LH/YYYY/NNN (e.g., LH/2024/001)
        /// </summary>
        Task<string> GenerateSerialNumberAsync(int year, int month);
        
        /// <summary>
        /// Get monthly register (all loans registered in a specific month)
        /// </summary>
        Task<MonthlyRegisterReport> GetMonthlyRegisterAsync(int year, int month);
        
        /// <summary>
        /// Get register entry by serial number
        /// </summary>
        Task<LoanRegister> GetBySerialNumberAsync(string serialNumber);
        
        /// <summary>
        /// Get register entry by loan ID
        /// </summary>
        Task<LoanRegister> GetByLoanIdAsync(Guid loanId);
        
        /// <summary>
        /// Get all register entries for a member
        /// </summary>
        Task<List<LoanRegister>> GetMemberRegisterEntriesAsync(Guid memberId);
        
        /// <summary>
        /// Export monthly register to Excel
        /// </summary>
        Task<byte[]> ExportMonthlyRegisterAsync(int year, int month);
        
        /// <summary>
        /// Get register statistics
        /// </summary>
        Task<RegisterStatistics> GetRegisterStatisticsAsync(int year, int? month = null);
    }
    
    /// <summary>
    /// Command to register a loan
    /// </summary>
    public class RegisterLoanCommand
    {
        public Guid LoanId { get; set; }
        public Guid ApplicationId { get; set; }
        public Guid MemberId { get; set; }
        public string RegisteredBy { get; set; }
        public string Notes { get; set; }
    }
    
    /// <summary>
    /// Monthly register report
    /// </summary>
    public class MonthlyRegisterReport
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; }
        public int TotalLoansRegistered { get; set; }
        public decimal TotalPrincipalAmount { get; set; }
        public decimal AverageInterestRate { get; set; }
        public int AverageTenor { get; set; }
        public List<LoanRegister> Entries { get; set; } = new List<LoanRegister>();
    }
    
    /// <summary>
    /// Register statistics
    /// </summary>
    public class RegisterStatistics
    {
        public int Year { get; set; }
        public int? Month { get; set; }
        public int TotalLoansRegistered { get; set; }
        public decimal TotalPrincipalDisbursed { get; set; }
        public decimal AverageLoanSize { get; set; }
        public int ShortestTenor { get; set; }
        public int LongestTenor { get; set; }
        public decimal LowestInterestRate { get; set; }
        public decimal HighestInterestRate { get; set; }
        public Dictionary<string, int> LoanTypeDistribution { get; set; } = new Dictionary<string, int>();
        public Dictionary<int, int> MonthlyDistribution { get; set; } = new Dictionary<int, int>();
    }
}
