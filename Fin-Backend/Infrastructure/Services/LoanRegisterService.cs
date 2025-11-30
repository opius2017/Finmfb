using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FinTech.Core.Application.Services;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Core.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace FinTech.Infrastructure.Services
{
    /// <summary>
    /// Implementation of loan register service
    /// </summary>
    public class LoanRegisterService : ILoanRegisterService
    {
        private readonly IRepository<LoanRegister> _registerRepository;
        private readonly IRepository<Loan> _loanRepository;
        private readonly IRepository<LoanApplication> _applicationRepository;
        private readonly IRepository<Member> _memberRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LoanRegisterService> _logger;
        
        // Lock object for thread-safe serial number generation
        private static readonly object _serialNumberLock = new object();
        
        public LoanRegisterService(
            IRepository<LoanRegister> registerRepository,
            IRepository<Loan> loanRepository,
            IRepository<LoanApplication> applicationRepository,
            IRepository<Member> memberRepository,
            IUnitOfWork unitOfWork,
            ILogger<LoanRegisterService> logger)
        {
            _registerRepository = registerRepository ?? throw new ArgumentNullException(nameof(registerRepository));
            _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
            _applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            _memberRepository = memberRepository ?? throw new ArgumentNullException(nameof(memberRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Register an approved loan in the official register
        /// </summary>
        public async Task<LoanRegister> RegisterLoanAsync(RegisterLoanCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            
            // Get loan details
            var loan = await _loanRepository.GetByIdAsync(command.LoanId.ToString());
            if (loan == null)
                throw new ArgumentException($"Loan not found: {command.LoanId}", nameof(command));
            
            // Get application details
            var application = await _applicationRepository.GetByIdAsync(command.ApplicationId.ToString());
            if (application == null)
                throw new ArgumentException($"Application not found: {command.ApplicationId}", nameof(command));
            
            // Get member details
            var member = await _memberRepository.GetByIdAsync(command.MemberId.ToString());
            if (member == null)
                throw new ArgumentException($"Member not found: {command.MemberId}", nameof(command));
            
            // Check if loan is already registered
            var existingEntry = (await _registerRepository.GetAllAsync())
                .FirstOrDefault(r => r.LoanId == command.LoanId);
            
            if (existingEntry != null)
            {
                _logger.LogWarning("Loan already registered: {LoanId}, Serial: {SerialNumber}",
                    command.LoanId, existingEntry.SerialNumber);
                return existingEntry;
            }
            
            // Generate serial number
            var now = DateTime.UtcNow;
            var serialNumber = await GenerateSerialNumberAsync(now.Year, now.Month);
            
            // Get sequence number from serial
            var parts = serialNumber.Split('/');
            var sequenceNumber = int.Parse(parts[2]);
            
            // Create register entry
            var registerEntry = new LoanRegister(
                serialNumber: serialNumber,
                loanId: command.LoanId,
                applicationId: command.ApplicationId,
                memberId: command.MemberId,
                memberNumber: member.MemberNumber,
                memberName: member.GetFullName(),
                principalAmount: loan.PrincipalAmount,
                interestRate: loan.InterestRate,
                tenorMonths: loan.LoanTermMonths,
                monthlyEMI: loan.MonthlyPayment,
                disbursementDate: loan.DisbursementDate ?? DateTime.UtcNow,
                maturityDate: loan.MaturityDate,
                loanType: loan.LoanType,
                registeredBy: command.RegisteredBy,
                year: now.Year,
                month: now.Month,
                sequenceNumber: sequenceNumber);
            
            if (!string.IsNullOrWhiteSpace(command.Notes))
            {
                registerEntry.UpdateStatus("REGISTERED", command.Notes);
            }
            
            await _registerRepository.AddAsync(registerEntry);
            await _unitOfWork.SaveChangesAsync();
            
            _logger.LogInformation(
                "Loan registered: Serial={SerialNumber}, Loan={LoanId}, Member={MemberNumber}",
                serialNumber, command.LoanId, member.MemberNumber);
            
            return registerEntry;
        }
        
        /// <summary>
        /// Generate next serial number atomically
        /// Format: LH/YYYY/NNN
        /// </summary>
        public async Task<string> GenerateSerialNumberAsync(int year, int month)
        {
            lock (_serialNumberLock)
            {
                // Get all entries for the year
                var yearEntries = _registerRepository.GetAllAsync().Result
                    .Where(r => r.RegistrationYear == year)
                    .OrderByDescending(r => r.SequenceNumber)
                    .ToList();
                
                // Get next sequence number
                int nextSequence = yearEntries.Any() 
                    ? yearEntries.First().SequenceNumber + 1 
                    : 1;
                
                // Format: LH/YYYY/NNN
                string serialNumber = $"LH/{year}/{nextSequence:D3}";
                
                _logger.LogInformation(
                    "Generated serial number: {SerialNumber} for {Year}/{Month}",
                    serialNumber, year, month);
                
                return serialNumber;
            }
        }
        
        /// <summary>
        /// Get monthly register report
        /// </summary>
        public async Task<MonthlyRegisterReport> GetMonthlyRegisterAsync(int year, int month)
        {
            var entries = (await _registerRepository.GetAllAsync())
                .Where(r => r.RegistrationYear == year && r.RegistrationMonth == month)
                .OrderBy(r => r.SequenceNumber)
                .ToList();
            
            var report = new MonthlyRegisterReport
            {
                Year = year,
                Month = month,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
                TotalLoansRegistered = entries.Count,
                TotalPrincipalAmount = entries.Sum(e => e.PrincipalAmount),
                AverageInterestRate = entries.Any() ? entries.Average(e => e.InterestRate) : 0,
                AverageTenor = entries.Any() ? (int)entries.Average(e => e.TenorMonths) : 0,
                Entries = entries
            };
            
            return report;
        }
        
        /// <summary>
        /// Get register entry by serial number
        /// </summary>
        public async Task<LoanRegister> GetBySerialNumberAsync(string serialNumber)
        {
            var entry = (await _registerRepository.GetAllAsync())
                .FirstOrDefault(r => r.SerialNumber == serialNumber);
            
            return entry;
        }
        
        /// <summary>
        /// Get register entry by loan ID
        /// </summary>
        public async Task<LoanRegister> GetByLoanIdAsync(Guid loanId)
        {
            var entry = (await _registerRepository.GetAllAsync())
                .FirstOrDefault(r => r.LoanId == loanId);
            
            return entry;
        }
        
        /// <summary>
        /// Get all register entries for a member
        /// </summary>
        public async Task<List<LoanRegister>> GetMemberRegisterEntriesAsync(Guid memberId)
        {
            var entries = (await _registerRepository.GetAllAsync())
                .Where(r => r.MemberId == memberId)
                .OrderByDescending(r => r.RegistrationDate)
                .ToList();
            
            return entries;
        }
        
        /// <summary>
        /// Export monthly register to Excel
        /// </summary>
        public async Task<byte[]> ExportMonthlyRegisterAsync(int year, int month)
        {
            var report = await GetMonthlyRegisterAsync(year, month);
            
            // TODO: Implement Excel export using EPPlus
            // For now, return empty byte array
            _logger.LogWarning("Excel export not yet implemented");
            return Array.Empty<byte>();
        }
        
        /// <summary>
        /// Get register statistics
        /// </summary>
        public async Task<RegisterStatistics> GetRegisterStatisticsAsync(int year, int? month = null)
        {
            var query = (await _registerRepository.GetAllAsync())
                .Where(r => r.RegistrationYear == year);
            
            if (month.HasValue)
            {
                query = query.Where(r => r.RegistrationMonth == month.Value);
            }
            
            var entries = query.ToList();
            
            var stats = new RegisterStatistics
            {
                Year = year,
                Month = month,
                TotalLoansRegistered = entries.Count,
                TotalPrincipalDisbursed = entries.Sum(e => e.PrincipalAmount),
                AverageLoanSize = entries.Any() ? entries.Average(e => e.PrincipalAmount) : 0,
                ShortestTenor = entries.Any() ? entries.Min(e => e.TenorMonths) : 0,
                LongestTenor = entries.Any() ? entries.Max(e => e.TenorMonths) : 0,
                LowestInterestRate = entries.Any() ? entries.Min(e => e.InterestRate) : 0,
                HighestInterestRate = entries.Any() ? entries.Max(e => e.InterestRate) : 0
            };
            
            // Loan type distribution
            stats.LoanTypeDistribution = entries
                .GroupBy(e => e.LoanType)
                .ToDictionary(g => g.Key, g => g.Count());
            
            // Monthly distribution (if year-level stats)
            if (!month.HasValue)
            {
                stats.MonthlyDistribution = entries
                    .GroupBy(e => e.RegistrationMonth)
                    .ToDictionary(g => g.Key, g => g.Count());
            }
            
            return stats;
        }
    }
}
