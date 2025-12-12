using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Loans;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Core.Domain.Repositories;
using Microsoft.Extensions.Logging;

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
            var existingRegister = (await _loanRegisterRepository.GetAllAsync())
                .FirstOrDefault(r => r.LoanId == loanId);

            if (existingRegister != null)
                throw new InvalidOperationException($"Loan is already registered with serial number {existingRegister.TransactionReference}");

            // Generate unique serial number (thread-safe)
            string serialNumber;
            lock (_serialNumberLock)
            {
                serialNumber = GenerateSerialNumber().Result;
            }

            // Create register entry
            var registerEntry = new LoanRegister
            {
                Id = Guid.NewGuid().ToString(),
                LoanId = loanId,
                RegisterDate = DateTime.UtcNow,
                EntryType = "REGISTRATION",
                Description = $"Loan registered with serial number {serialNumber}",
                Amount = loan.PrincipalAmount,
                Balance = loan.OutstandingBalance,
                TransactionReference = serialNumber,
                RecordedBy = registeredBy,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = registeredBy
            };

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
            var lastEntry = (await _loanRegisterRepository.GetAllAsync())
                .Where(r => r.TransactionReference != null && 
                           r.TransactionReference.StartsWith(yearPrefix))
                .OrderByDescending(r => r.RegisterDate)
                .FirstOrDefault();

            int nextNumber = 1;

            if (lastEntry != null)
            {
                // Extract the number from the last serial (e.g., "LH/2024/005" -> 5)
                string lastSerial = lastEntry.TransactionReference;
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
            var query = (await _loanRegisterRepository.GetAllAsync())
                .Where(r => r.EntryType == "REGISTRATION")
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(r => r.RegisterDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(r => r.RegisterDate <= toDate.Value);

            var entries = query.OrderByDescending(r => r.RegisterDate).ToList();

            var registerEntries = new List<LoanRegisterEntry>();

            foreach (var entry in entries)
            {
                var loan = await _loanRepository.GetByIdAsync(entry.LoanId);
                if (loan == null)
                    continue;

                var application = await _loanApplicationRepository.GetByIdAsync(loan.LoanApplicationId);
                var member = await _memberRepository.GetByIdAsync(loan.MemberId);

                if (status != null && loan.LoanStatus != status)
                    continue;

                registerEntries.Add(new LoanRegisterEntry
                {
                    SerialNumber = entry.TransactionReference,
                    RegisterDate = entry.RegisterDate,
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
                    Status = loan.LoanStatus,
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
            var entry = (await _loanRegisterRepository.GetAllAsync())
                .FirstOrDefault(r => r.TransactionReference == serialNumber);

            if (entry == null)
                return null;

            var loan = await _loanRepository.GetByIdAsync(entry.LoanId);
            if (loan == null)
                return null;

            var application = await _loanApplicationRepository.GetByIdAsync(loan.LoanApplicationId);
            var member = await _memberRepository.GetByIdAsync(loan.MemberId);

            return new LoanRegisterEntry
            {
                SerialNumber = entry.TransactionReference,
                RegisterDate = entry.RegisterDate,
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
                Status = loan.LoanStatus,
                OutstandingBalance = loan.OutstandingBalance,
                RecordedBy = entry.RecordedBy
            };
        }

        /// <summary>
        /// Adds a transaction entry to the loan register
        /// </summary>
        public async Task<string> AddRegisterEntryAsync(
            string loanId,
            string entryType,
            string description,
            decimal amount,
            string transactionReference,
            string recordedBy)
        {
            _logger.LogInformation(
                "Adding register entry for loan {LoanId}, type {EntryType}",
                loanId, entryType);

            var loan = await _loanRepository.GetByIdAsync(loanId);
            if (loan == null)
                throw new InvalidOperationException("Loan not found");

            var entry = new LoanRegister
            {
                Id = Guid.NewGuid().ToString(),
                LoanId = loanId,
                RegisterDate = DateTime.UtcNow,
                EntryType = entryType,
                Description = description,
                Amount = amount,
                Balance = loan.OutstandingBalance,
                TransactionReference = transactionReference,
                RecordedBy = recordedBy,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = recordedBy
            };

            await _loanRegisterRepository.AddAsync(entry);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Register entry added with ID {EntryId}", entry.Id);

            return entry.Id;
        }

        /// <summary>
        /// Gets all register entries for a specific loan
        /// </summary>
        public async Task<List<LoanRegisterTransactionEntry>> GetLoanRegisterHistoryAsync(string loanId)
        {
            var entries = (await _loanRegisterRepository.GetAllAsync())
                .Where(r => r.LoanId == loanId)
                .OrderBy(r => r.RegisterDate)
                .ToList();

            return entries.Select(e => new LoanRegisterTransactionEntry
            {
                EntryId = e.Id,
                RegisterDate = e.RegisterDate,
                EntryType = e.EntryType,
                Description = e.Description,
                Amount = e.Amount,
                Balance = e.Balance,
                TransactionReference = e.TransactionReference,
                RecordedBy = e.RecordedBy
            }).ToList();
        }

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

            var allEntries = (await _loanRegisterRepository.GetAllAsync())
                .Where(r => r.EntryType == "REGISTRATION" && 
                           r.RegisterDate.Year == targetYear)
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
                var loan = await _loanRepository.GetByIdAsync(entry.LoanId);
                if (loan != null)
                {
                    statistics.TotalPrincipalAmount += loan.PrincipalAmount;
                    statistics.TotalOutstandingBalance += loan.OutstandingBalance;

                    if (loan.LoanStatus == "ACTIVE" || loan.LoanStatus == "DISBURSED")
                        statistics.ActiveLoans++;
                    else if (loan.LoanStatus == "CLOSED")
                        statistics.ClosedLoans++;
                }
            }

            return statistics;
        }

        // Interface methods matching ILoanRegisterService
        async Task<LoanRegisterEntryDto> ILoanRegisterService.RegisterLoanAsync(string loanId, string registeredBy)
        {
            var result = await RegisterLoanAsync(loanId, registeredBy);
            return new LoanRegisterEntryDto
            {
                LoanId = loanId,
                SerialNumber = result.SerialNumber,
                RegisterDate = result.RegisterDate
            };
        }

        async Task<string> ILoanRegisterService.GenerateSerialNumberAsync(int year)
        {
            return await GenerateSerialNumber();
        }

        async Task<LoanRegisterEntryDto> ILoanRegisterService.GetRegisterEntryAsync(string loanId)
        {
            var entries = await GetLoanRegisterAsync(null, null, loanId);
            var entry = entries.FirstOrDefault();
            if (entry == null) return null;
            
            return new LoanRegisterEntryDto
            {
                LoanId = entry.LoanId,
                SerialNumber = entry.SerialNumber,
                RegisterDate = entry.RegisterDate
            };
        }

        async Task<List<LoanRegisterEntryDto>> ILoanRegisterService.GetRegisterEntriesAsync(int? year, int? month)
        {
            var entries = await GetLoanRegisterAsync(year, month, null);
            return entries.Select(e => new LoanRegisterEntryDto
            {
                LoanId = e.LoanId,
                SerialNumber = e.SerialNumber,
                RegisterDate = e.RegisterDate
            }).ToList();
        }

        async Task<byte[]> ILoanRegisterService.ExportRegisterAsync(int year)
        {
            var csvContent = await ExportLoanRegisterAsync(year, null);
            return System.Text.Encoding.UTF8.GetBytes(csvContent);
        }
    }
}
