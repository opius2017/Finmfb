using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Loans;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Core.Domain.Repositories;
using Microsoft.Extensions.Logging;

using FinTech.Core.Application.Interfaces.Loans;

namespace FinTech.Core.Application.Services.Loans
{
    /// <summary>
    /// Service for managing the official loan register with serial number assignment
    /// </summary>
    public class LoanRegisterService : ILoanRegisterService
    {
        private readonly IRepository<LoanRegister> _loanRegisterRepository;
        private readonly IRepository<Loan> _loanRepository;
        private readonly IRepository<LoanApplication> _loanApplicationRepository;
        private readonly IRepository<Member> _memberRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LoanRegisterService> _logger;
        private static readonly object _serialNumberLock = new object();

        public LoanRegisterService(
            IRepository<LoanRegister> loanRegisterRepository,
            IRepository<Loan> loanRepository,
            IRepository<LoanApplication> loanApplicationRepository,
            IRepository<Member> memberRepository,
            IUnitOfWork unitOfWork,
            ILogger<LoanRegisterService> logger)
        {
            _loanRegisterRepository = loanRegisterRepository;
            _loanRepository = loanRepository;
            _loanApplicationRepository = loanApplicationRepository;
            _memberRepository = memberRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Registers a loan with unique serial number in format LH/YYYY/NNN
        /// </summary>
        public async Task<LoanRegistrationResult> RegisterLoanAsync(string loanId, string registeredBy)
        {
            _logger.LogInformation("Registering loan {LoanId}", loanId);

            var loan = await _loanRepository.GetByIdAsync(loanId);
            if (loan == null)
                throw new InvalidOperationException("Loan not found");

            // Check if loan is already registered
            var existingRegister = (await _loanRegisterRepository.ListAllAsync())
                .FirstOrDefault(r => r.LoanId.ToString() == loanId);

            if (existingRegister != null)
                throw new InvalidOperationException($"Loan is already registered with serial number {existingRegister.SerialNumber}");

            // Generate unique serial number (thread-safe)
            string serialNumber;
            lock (_serialNumberLock)
            {
                serialNumber = GenerateSerialNumber().Result;
            }

            // Extract sequence info
            var parts = serialNumber.Split('/');
            int year = int.Parse(parts[1]);
            int sequenceNumber = int.Parse(parts[2]);

            // Fetch member
            var member = await _memberRepository.GetByIdAsync(loan.MemberId);
            if (member == null)
                 throw new InvalidOperationException($"Member with ID {loan.MemberId} not found");

            // Create register entry
            var registerEntry = new LoanRegister(
                serialNumber,
                loan.Id,
                loan.LoanApplicationId?.ToString() ?? string.Empty,
                loan.MemberId,
                member.MemberNumber ?? "UNKNOWN",
                $"{member.FirstName} {member.LastName}",
                loan.PrincipalAmount,
                loan.InterestRate,
                loan.TenureMonths,
                loan.MonthlyInstallment,
                loan.DisbursementDate,
                loan.MaturityDate,
                "NORMAL",
                registeredBy,
                year,
                DateTime.UtcNow.Month,
                sequenceNumber
            );

            await _loanRegisterRepository.AddAsync(registerEntry);

            // Update loan with serial number
            loan.LoanNumber = serialNumber;
            loan.UpdatedAt = DateTime.UtcNow;
            await _loanRepository.UpdateAsync(loan);

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Loan {LoanId} registered successfully with serial number {SerialNumber}",
                loanId, serialNumber);

            return new LoanRegistrationResult
            {
                LoanId = loanId,
                SerialNumber = serialNumber,
                RegisterDate = registerEntry.RegisterDate,
                RegisterEntryId = registerEntry.Id,
                Message = $"Loan registered successfully with serial number {serialNumber}"
            };
        }

        /// <summary>
        /// Generates unique serial number in format LH/YYYY/NNN
        /// </summary>
        public async Task<string> GenerateSerialNumber()
        {
            int currentYear = DateTime.UtcNow.Year;
            string yearPrefix = $"LH/{currentYear}/";

            // Get the last serial number for this year
            var lastEntry = (await _loanRegisterRepository.ListAllAsync())
                .Where(r => r.SerialNumber != null && 
                           r.SerialNumber.StartsWith(yearPrefix))
                .OrderByDescending(r => r.RegistrationDate)
                .FirstOrDefault();

            int nextNumber = 1;

            if (lastEntry != null)
            {
                // Extract the number from the last serial (e.g., "LH/2024/005" -> 5)
                string lastSerial = lastEntry.SerialNumber;
                string[] parts = lastSerial.Split('/');
                if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            // Format: LH/YYYY/NNN (e.g., LH/2024/001)
            string serialNumber = $"{yearPrefix}{nextNumber:D3}";

            _logger.LogInformation("Generated serial number: {SerialNumber}", serialNumber);

            return serialNumber;
        }

        /// <summary>
        /// Gets the complete loan register (read-only view)
        /// </summary>
        public async Task<List<LoanRegisterEntry>> GetLoanRegisterAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? status = null)
        {
            var query = (await _loanRegisterRepository.ListAllAsync())
                //.Where(r => r.EntryType == "REGISTRATION") // EntryType removed
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(r => r.RegistrationDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(r => r.RegistrationDate <= toDate.Value);

            var entries = query.OrderByDescending(r => r.RegistrationDate).ToList();

            var registerEntries = new List<LoanRegisterEntry>();

            foreach (var entry in entries)
            {
                var loan = await _loanRepository.GetByIdAsync(entry.LoanId.ToString());
                if (loan == null)
                    continue;

                var application = await _loanApplicationRepository.GetByIdAsync(loan.LoanApplicationId.ToString());
                var member = await _memberRepository.GetByIdAsync(loan.MemberId);

                if (status != null && loan.Status != status)
                    continue;

                registerEntries.Add(new LoanRegisterEntry
                {
                    SerialNumber = entry.SerialNumber,
                    RegisterDate = entry.RegistrationDate,
                    LoanId = loan.Id,
                    ApplicationNumber = application?.ApplicationNumber,
                    MemberNumber = member?.MemberNumber,
                    MemberName = member != null ? $"{member.FirstName} {member.LastName}" : "Unknown",
                    PrincipalAmount = loan.PrincipalAmount,
                    InterestRate = loan.InterestRate,
                    TenorMonths = loan.RepaymentPeriodMonths,
                    MonthlyEMI = loan.MonthlyInstallment,
                    DisbursementDate = loan.DisbursementDate,
                    MaturityDate = loan.MaturityDate,
                    Status = loan.Status,
                    OutstandingBalance = loan.OutstandingBalance,
                    RecordedBy = entry.RecordedBy
                });
            }

            return registerEntries;
        }

        /// <summary>
        /// Gets loan register entry by serial number
        /// </summary>
        public async Task<LoanRegisterEntry> GetBySerialNumberAsync(string serialNumber)
        {
            var entry = (await _loanRegisterRepository.ListAllAsync())
                .FirstOrDefault(r => r.SerialNumber == serialNumber);

            if (entry == null)
                return null;

            var loan = await _loanRepository.GetByIdAsync(entry.LoanId.ToString());
            if (loan == null)
                return null;

            var application = await _loanApplicationRepository.GetByIdAsync(loan.LoanApplicationId.ToString());
            var member = await _memberRepository.GetByIdAsync(loan.MemberId);

            return new LoanRegisterEntry
            {
                SerialNumber = entry.SerialNumber,
                RegisterDate = entry.RegistrationDate,
                LoanId = loan.Id,
                ApplicationNumber = application?.ApplicationNumber,
                MemberNumber = member?.MemberNumber,
                MemberName = member != null ? $"{member.FirstName} {member.LastName}" : "Unknown",
                PrincipalAmount = loan.PrincipalAmount,
                InterestRate = loan.InterestRate,
                TenorMonths = loan.RepaymentPeriodMonths,
                MonthlyEMI = loan.MonthlyInstallment,
                DisbursementDate = loan.DisbursementDate,
                MaturityDate = loan.MaturityDate,
                Status = loan.Status,
                OutstandingBalance = loan.OutstandingBalance,
                RecordedBy = entry.RecordedBy
            };
        }

        // Removed AddRegisterEntryAsync and GetLoanRegisterHistoryAsync as they are not supported by the entity model

        /// <summary>
        /// Exports loan register to CSV format
        /// </summary>
        public async Task<string> ExportLoanRegisterAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var entries = await GetLoanRegisterAsync(fromDate, toDate);

            var csv = new System.Text.StringBuilder();
            
            // Header
            csv.AppendLine("Serial Number,Register Date,Member Number,Member Name,Principal Amount,Interest Rate,Tenor (Months),Monthly EMI,Disbursement Date,Maturity Date,Status,Outstanding Balance,Recorded By");

            // Data rows
            foreach (var entry in entries)
            {
                csv.AppendLine($"{entry.SerialNumber}," +
                              $"{entry.RegisterDate:yyyy-MM-dd}," +
                              $"{entry.MemberNumber}," +
                              $"\"{entry.MemberName}\"," +
                              $"{entry.PrincipalAmount}," +
                              $"{entry.InterestRate}," +
                              $"{entry.TenorMonths}," +
                              $"{entry.MonthlyEMI}," +
                              $"{entry.DisbursementDate:yyyy-MM-dd}," +
                              $"{entry.MaturityDate:yyyy-MM-dd}," +
                              $"{entry.Status}," +
                              $"{entry.OutstandingBalance}," +
                              $"{entry.RecordedBy}");
            }

            return csv.ToString();
        }

        /// <summary>
        /// Gets register statistics
        /// </summary>
        public async Task<LoanRegisterStatistics> GetRegisterStatisticsAsync(int? year = null)
        {
            int targetYear = year ?? DateTime.UtcNow.Year;

            var allEntries = (await _loanRegisterRepository.ListAllAsync())
                .Where(r => r.RegistrationDate.Year == targetYear)
                .ToList();

            var statistics = new LoanRegisterStatistics
            {
                Year = targetYear,
                TotalLoansRegistered = allEntries.Count,
                TotalPrincipalAmount = 0,
                TotalOutstandingBalance = 0,
                ActiveLoans = 0,
                ClosedLoans = 0
            };

            foreach (var entry in allEntries)
            {
                var loan = await _loanRepository.GetByIdAsync(entry.LoanId.ToString());
                if (loan != null)
                {
                    statistics.TotalPrincipalAmount += loan.PrincipalAmount;
                    statistics.TotalOutstandingBalance += loan.OutstandingBalance;

                    if (loan.Status == "ACTIVE" || loan.Status == "DISBURSED")
                        statistics.ActiveLoans++;
                    else if (loan.Status == "CLOSED")
                        statistics.ClosedLoans++;
                }
            }

            return statistics;
        }


    }
}
