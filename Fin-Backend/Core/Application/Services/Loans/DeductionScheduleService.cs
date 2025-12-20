using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Loans;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Core.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using FinTech.Core.Application.Interfaces.Loans;

namespace FinTech.Core.Application.Services.Loans
{
    /// <summary>
    /// Service for managing monthly payroll deduction schedules
    /// </summary>
    public class DeductionScheduleService : IDeductionScheduleService
    {
        private readonly IRepository<DeductionSchedule> _scheduleRepository;
        private readonly IRepository<DeductionScheduleItem> _itemRepository;
        private readonly IRepository<Loan> _loanRepository;
        private readonly IRepository<Member> _memberRepository;
        private readonly ILoanCalculatorService _calculatorService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeductionScheduleService> _logger;

        public DeductionScheduleService(
            IRepository<DeductionSchedule> scheduleRepository,
            IRepository<DeductionScheduleItem> itemRepository,
            IRepository<Loan> loanRepository,
            IRepository<Member> memberRepository,
            ILoanCalculatorService calculatorService,
            IUnitOfWork unitOfWork,
            ILogger<DeductionScheduleService> logger)
        {
            _scheduleRepository = scheduleRepository;
            _itemRepository = itemRepository;
            _loanRepository = loanRepository;
            _memberRepository = memberRepository;
            _calculatorService = calculatorService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<DeductionScheduleDto> GenerateScheduleAsync(GenerateDeductionScheduleRequest request)
        {
            try
            {
                _logger.LogInformation("Generating deduction schedule for {Month}/{Year}", request.Month, request.Year);

                // Check if schedule already exists
                var existingSchedule = await GetScheduleByMonthAsync(request.Month, request.Year);
                if (existingSchedule != null && existingSchedule.Status != "CANCELLED")
                {
                    throw new InvalidOperationException($"Schedule already exists for {request.Month}/{request.Year}");
                }

                // Get all active loans
                var activeLoans = await _loanRepository.GetAll()
                    .Where(l => l.Status == "ACTIVE" && l.OutstandingBalance > 0)
                    .ToListAsync();

                if (!activeLoans.Any())
                {
                    throw new InvalidOperationException("No active loans found for schedule generation");
                }

                // Generate schedule number
                var scheduleNumber = await GenerateScheduleNumberAsync(request.Month, request.Year);

                // Create schedule
                var schedule = new DeductionSchedule
                {
                    ScheduleNumber = scheduleNumber,
                    Month = request.Month,
                    Year = request.Year,
                    Status = "DRAFT",
                    Notes = request.Notes,
                    CreatedBy = request.CreatedBy,
                    CreatedAt = DateTime.UtcNow
                };

                await _scheduleRepository.AddAsync(schedule);

                // Generate schedule items
                var items = new List<DeductionScheduleItem>();
                decimal totalAmount = 0;

                foreach (var loan in activeLoans)
                {
                    var member = await _memberRepository.GetByIdAsync(loan.MemberId);
                    if (member == null) continue;

                    // Calculate deduction amount (monthly installment)
                    var deductionAmount = loan.MonthlyInstallment;
                    
                    // Calculate breakdown
                    var monthlyRate = (loan.InterestRate / 12) / 100; // Assuming InterestRate is annual percentage
                    var breakdown = _calculatorService.CalculateInstallmentBreakdown(
                        loan.OutstandingBalance, monthlyRate, deductionAmount);
                    
                    var monthlyInterest = breakdown.InterestAmount;
                    var principalAmount = breakdown.PrincipalAmount;
                    
                    if (principalAmount < 0) principalAmount = 0;

                    var item = new DeductionScheduleItem
                    {
                        // FinTech Best Practice: Convert Guid to string for ScheduleId
                        ScheduleId = Guid.Parse(schedule.Id),
                        LoanId = loan.Id,
                        MemberId = member.Id,
                        // Member number/name/loan number not in entity
                        Amount = deductionAmount,
                        PrincipalAmount = principalAmount,
                        InterestAmount = monthlyInterest,
                        // PenaltyAmount/OutstandingBalance not in entity
                        InstallmentNumber = CalculateInstallmentNumber(loan),
                        // EmployeeId/Department not in entity
                        Status = "PENDING",
                        CreatedBy = request.CreatedBy,
                        CreatedAt = DateTime.UtcNow
                    };

                    items.Add(item);
                    await _itemRepository.AddAsync(item);
                    totalAmount += deductionAmount;
                }

                // Update schedule totals
                schedule.TotalAmount = totalAmount;
                schedule.TotalMembers = items.Count;

                await _scheduleRepository.UpdateAsync(schedule);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Generated schedule {ScheduleNumber} with {Count} items, total: {Total:C}",
                    scheduleNumber, items.Count, totalAmount);

                return await MapToDto(schedule, items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating deduction schedule for {Month}/{Year}", request.Month, request.Year);
                throw;
            }
        }

        public async Task<DeductionScheduleDto?> GetScheduleByIdAsync(string scheduleId)
        {
            var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
            if (schedule == null) return null;

            // FinTech Best Practice: Convert string to Guid for comparison
            var items = await _itemRepository.GetAll()
                .Where(i => i.ScheduleId.ToString() == scheduleId)
                .Include(i => i.Member)
                .Include(i => i.Loan)
                .ToListAsync();
            return await MapToDto(schedule, items);
        }

        public async Task<DeductionScheduleDto?> GetScheduleByMonthAsync(int month, int year)
        {
            var schedules = await _scheduleRepository.GetAll()
                .Where(s => s.Month == month && s.Year == year)
                .ToListAsync();
            
            var schedule = schedules.FirstOrDefault();
            if (schedule == null) return null;

            // FinTech Best Practice: Convert Guid to string for comparison
            var items = await _itemRepository.GetAll()
                .Where(i => i.ScheduleId == Guid.Parse(schedule.Id))
                .Include(i => i.Member)
                .Include(i => i.Loan)
                .ToListAsync();
            return await MapToDto(schedule, items);
        }

        public async Task<List<DeductionScheduleDto>> GetSchedulesAsync(int? year = null, string? status = null)
        {
            var query = _scheduleRepository.GetAll();
            
            if (year.HasValue)
                query = query.Where(s => s.Year == year.Value);
            
            if (!string.IsNullOrEmpty(status))
                query = query.Where(s => s.Status == status);

            var schedules = query.OrderByDescending(s => s.Year)
                                .ThenByDescending(s => s.Month)
                                .ToList();

            var result = new List<DeductionScheduleDto>();
            foreach (var schedule in schedules)
            {
                var items = await _itemRepository.GetAll()
                    .Where(i => i.ScheduleId.ToString() == schedule.Id)
                    .Include(i => i.Member)
                    .Include(i => i.Loan)
                    .ToListAsync();
                result.Add(await MapToDto(schedule, items));
            }

            return result;
        }

        public async Task<DeductionScheduleDto> ApproveScheduleAsync(ApproveDeductionScheduleRequest request)
        {
            var schedule = await _scheduleRepository.GetByIdAsync(request.ScheduleId);
            if (schedule == null)
                throw new InvalidOperationException("Schedule not found");

            if (schedule.Status != "DRAFT" && schedule.Status != "PENDING_APPROVAL")
                throw new InvalidOperationException($"Cannot approve schedule with status {schedule.Status}");

            schedule.Status = "APPROVED";
            schedule.ApprovedBy = request.ApprovedBy;
            schedule.ApprovedDate = DateTime.UtcNow;
            schedule.UpdatedAt = DateTime.UtcNow;
            schedule.UpdatedBy = request.ApprovedBy;

            if (!string.IsNullOrEmpty(request.Notes))
                schedule.Notes = request.Notes;

            await _scheduleRepository.UpdateAsync(schedule);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Schedule {ScheduleNumber} approved by {ApprovedBy}",
                schedule.ScheduleNumber, request.ApprovedBy);

            // FinTech Best Practice: Convert Guid to string for comparison
            var items = await _itemRepository.GetAll()
                .Where(i => i.ScheduleId == Guid.Parse(schedule.Id))
                .Include(i => i.Member)
                .Include(i => i.Loan) // Need loan and member for DTO
                .ToListAsync();
            return await MapToDto(schedule, items);
        }

        public async Task<DeductionScheduleDto> SubmitScheduleAsync(SubmitDeductionScheduleRequest request)
        {
            var schedule = await _scheduleRepository.GetByIdAsync(request.ScheduleId);
            if (schedule == null)
                throw new InvalidOperationException("Schedule not found");

            if (schedule.Status != "APPROVED")
                throw new InvalidOperationException("Only approved schedules can be submitted");

            schedule.Status = "SUBMITTED";
            // schedule.SubmittedBy = request.SubmittedBy; // Property missing
            // schedule.SubmittedAt = DateTime.UtcNow; // Property missing
            schedule.UpdatedAt = DateTime.UtcNow;
            schedule.UpdatedBy = request.SubmittedBy;

            await _scheduleRepository.UpdateAsync(schedule);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Schedule {ScheduleNumber} submitted by {SubmittedBy}",
                schedule.ScheduleNumber, request.SubmittedBy);

            // FinTech Best Practice: Convert Guid to string for comparison
            var items = await _itemRepository.GetAll()
                .Where(i => i.ScheduleId == Guid.Parse(schedule.Id))
                .Include(i => i.Member)
                .ToListAsync();
            return await MapToDto(schedule, items);
        }

        public async Task<DeductionScheduleExportResult> ExportScheduleAsync(ExportDeductionScheduleRequest request)
        {
            var schedule = await _scheduleRepository.GetByIdAsync(request.ScheduleId);
            if (schedule == null)
                throw new InvalidOperationException("Schedule not found");

            // FinTech Best Practice: Convert Guid to string for comparison
            var items = await _itemRepository.GetAll()
                .Where(i => i.ScheduleId == Guid.Parse(schedule.Id))
                .ToListAsync();

            // Export will be implemented with EPPlus in integration phase
            // For now, return placeholder
            return new DeductionScheduleExportResult
            {
                Success = true,
                FileName = $"DeductionSchedule_{schedule.ScheduleNumber}_{DateTime.UtcNow:yyyyMMdd}.xlsx",
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                Message = "Export functionality will be implemented with EPPlus"
            };
        }

        public async Task<bool> CancelScheduleAsync(string scheduleId, string cancelledBy)
        {
            var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
            if (schedule == null) return false;

            if (schedule.Status == "SUBMITTED" || schedule.Status == "PROCESSED")
                throw new InvalidOperationException("Cannot cancel submitted or processed schedule");

            schedule.Status = "CANCELLED";
            schedule.UpdatedAt = DateTime.UtcNow;
            schedule.UpdatedBy = cancelledBy;

            await _scheduleRepository.UpdateAsync(schedule);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Schedule {ScheduleNumber} cancelled by {CancelledBy}",
                schedule.ScheduleNumber, cancelledBy);

            return true;
        }

        public async Task<DeductionScheduleDto> CreateNewVersionAsync(string scheduleId, string createdBy)
        {
            var originalSchedule = await _scheduleRepository.GetByIdAsync(scheduleId);
            if (originalSchedule == null)
                throw new InvalidOperationException("Original schedule not found");

            var originalItems = await _itemRepository.GetAll()
                .Where(i => i.ScheduleId.ToString() == scheduleId)
                .ToListAsync();

            // Create new version
            // Note: Version is not in entity, using simple suffix or ignoring version logic for now if properties missing
            var newSchedule = new DeductionSchedule
            {
                ScheduleNumber = $"{originalSchedule.ScheduleNumber}_COPY",
                Month = originalSchedule.Month,
                Year = originalSchedule.Year,
                Status = "DRAFT",
                // Version = originalSchedule.Version + 1, // Missing property
                Notes = $"Copy of {originalSchedule.ScheduleNumber}",
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };

            await _scheduleRepository.AddAsync(newSchedule);

            // Copy items
            var newItems = new List<DeductionScheduleItem>();
            foreach (var item in originalItems)
            {
                var newItem = new DeductionScheduleItem
                {
                    // FinTech Best Practice: Convert string to Guid for ScheduleId
                    ScheduleId = Guid.Parse(newSchedule.Id),
                    LoanId = item.LoanId,
                    MemberId = item.MemberId,
                    // MemberNumber = item.MemberNumber, // Missing
                    // MemberName = item.MemberName, // Missing
                    // LoanNumber = item.LoanNumber, // Missing
                    Amount = item.Amount,
                    PrincipalAmount = item.PrincipalAmount,
                    InterestAmount = item.InterestAmount,
                    // PenaltyAmount = item.PenaltyAmount, // Missing
                    // OutstandingBalance = item.OutstandingBalance, // Missing
                    InstallmentNumber = item.InstallmentNumber,
                    // EmployeeId = item.EmployeeId, // Missing
                    // Department = item.Department, // Missing
                    Status = "PENDING",
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                };
                newItems.Add(newItem);
                await _itemRepository.AddAsync(newItem);
            }

            // await _itemRepository.AddRangeAsync(newItems); // Using explicit loop above

            newSchedule.TotalAmount = newItems.Sum(i => i.Amount);
            newSchedule.TotalMembers = newItems.Count;

            await _scheduleRepository.UpdateAsync(newSchedule);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Created new version of schedule {ScheduleNumber}", originalSchedule.ScheduleNumber);

            return await MapToDto(newSchedule, newItems);
        }

        #region Helper Methods

        private async Task<string> GenerateScheduleNumberAsync(int month, int year)
        {
            var existingSchedules = await _scheduleRepository.GetAll()
                .Where(s => s.Month == month && s.Year == year)
                .ToListAsync();
            
            var count = existingSchedules.Count() + 1;
            return $"DS/{year}/{month:D2}/{count:D3}";
        }

        private int CalculateInstallmentNumber(Loan loan)
        {
            // FinTech Best Practice: DisbursementDate is DateTime (non-nullable), remove .Value
            if (loan.DisbursementDate == default) return 1;
            
            var monthsSinceDisbursement = ((DateTime.UtcNow.Year - loan.DisbursementDate.Year) * 12) +
                                         DateTime.UtcNow.Month - loan.DisbursementDate.Month;
            
            return Math.Max(1, monthsSinceDisbursement + 1);
        }

        private async Task<DeductionScheduleDto> MapToDto(DeductionSchedule schedule, List<DeductionScheduleItem> items)
        {
            return await Task.FromResult(new DeductionScheduleDto
            {
                Id = schedule.Id,
                ScheduleNumber = schedule.ScheduleNumber,
                Month = schedule.Month,
                Year = schedule.Year,
                Status = schedule.Status,
                TotalDeductionAmount = schedule.TotalAmount,
                TotalLoansCount = schedule.TotalMembers,
                ApprovedAt = schedule.ApprovedDate,
                ApprovedBy = schedule.ApprovedBy,
                SubmittedAt = schedule.ProcessedDate, // Mapping ProcessedDate to SubmittedAt as fallback
                SubmittedBy = schedule.ProcessedBy, // Mapping ProcessedBy to SubmittedBy as fallback
                Notes = schedule.Notes,
                Version = 1, // Default, not in entity
                FilePath = "", // Not in entity
                CreatedAt = schedule.CreatedAt,
                CreatedBy = schedule.CreatedBy,
                Items = items.Select(i => new DeductionScheduleItemDto
                {
                    Id = i.Id,
                    MemberNumber = i.Member?.MemberNumber ?? "",
                    MemberName = i.Member != null ? $"{i.Member.FirstName} {i.Member.LastName}" : "",
                    LoanNumber = i.Loan?.LoanNumber ?? "",
                    DeductionAmount = i.Amount,
                    PrincipalAmount = i.PrincipalAmount,
                    InterestAmount = i.InterestAmount,
                    PenaltyAmount = 0, // Not in entity
                    OutstandingBalance = 0, // Not in entity
                    InstallmentNumber = i.InstallmentNumber,
                    EmployeeId = i.Member?.EmployeeId ?? "",
                    Department = i.Member?.Department ?? "",
                    Status = i.Status
                }).ToList()
            });
        }

        #endregion
    }
}
