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
                var activeLoans = await _loanRepository.FindAsync(l => 
                    l.LoanStatus == "ACTIVE" && 
                    l.OutstandingBalance > 0);

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
                    var monthlyInterest = _calculatorService.CalculateMonthlyInterest(
                        loan.OutstandingBalance, loan.InterestRate);
                    var principalAmount = deductionAmount - monthlyInterest;
                    
                    if (principalAmount < 0) principalAmount = 0;

                    var item = new DeductionScheduleItem
                    {
                        DeductionScheduleId = schedule.Id,
                        LoanId = loan.Id,
                        MemberId = member.Id,
                        MemberNumber = member.MemberNumber,
                        MemberName = $"{member.FirstName} {member.LastName}",
                        LoanNumber = loan.LoanNumber,
                        DeductionAmount = deductionAmount,
                        PrincipalAmount = principalAmount,
                        InterestAmount = monthlyInterest,
                        PenaltyAmount = loan.PenaltyAmount,
                        OutstandingBalance = loan.OutstandingBalance,
                        InstallmentNumber = CalculateInstallmentNumber(loan),
                        EmployeeId = member.EmployeeId,
                        Department = member.Department,
                        Status = "PENDING",
                        CreatedBy = request.CreatedBy,
                        CreatedAt = DateTime.UtcNow
                    };

                    items.Add(item);
                    totalAmount += deductionAmount;
                }

                await _itemRepository.AddRangeAsync(items);

                // Update schedule totals
                schedule.TotalDeductionAmount = totalAmount;
                schedule.TotalLoansCount = items.Count;

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

            var items = await _itemRepository.FindAsync(i => i.DeductionScheduleId == scheduleId);
            return await MapToDto(schedule, items.ToList());
        }

        public async Task<DeductionScheduleDto?> GetScheduleByMonthAsync(int month, int year)
        {
            var schedules = await _scheduleRepository.FindAsync(s => 
                s.Month == month && s.Year == year);
            
            var schedule = schedules.FirstOrDefault();
            if (schedule == null) return null;

            var items = await _itemRepository.FindAsync(i => i.DeductionScheduleId == schedule.Id);
            return await MapToDto(schedule, items.ToList());
        }

        public async Task<List<DeductionScheduleDto>> GetSchedulesAsync(int? year = null, string? status = null)
        {
            var query = await _scheduleRepository.GetAllAsync();
            
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
                var items = await _itemRepository.FindAsync(i => i.DeductionScheduleId == schedule.Id);
                result.Add(await MapToDto(schedule, items.ToList()));
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
            schedule.ApprovedAt = DateTime.UtcNow;
            schedule.UpdatedAt = DateTime.UtcNow;
            schedule.UpdatedBy = request.ApprovedBy;

            if (!string.IsNullOrEmpty(request.Notes))
                schedule.Notes = request.Notes;

            await _scheduleRepository.UpdateAsync(schedule);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Schedule {ScheduleNumber} approved by {ApprovedBy}",
                schedule.ScheduleNumber, request.ApprovedBy);

            var items = await _itemRepository.FindAsync(i => i.DeductionScheduleId == schedule.Id);
            return await MapToDto(schedule, items.ToList());
        }

        public async Task<DeductionScheduleDto> SubmitScheduleAsync(SubmitDeductionScheduleRequest request)
        {
            var schedule = await _scheduleRepository.GetByIdAsync(request.ScheduleId);
            if (schedule == null)
                throw new InvalidOperationException("Schedule not found");

            if (schedule.Status != "APPROVED")
                throw new InvalidOperationException("Only approved schedules can be submitted");

            schedule.Status = "SUBMITTED";
            schedule.SubmittedBy = request.SubmittedBy;
            schedule.SubmittedAt = DateTime.UtcNow;
            schedule.UpdatedAt = DateTime.UtcNow;
            schedule.UpdatedBy = request.SubmittedBy;

            await _scheduleRepository.UpdateAsync(schedule);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Schedule {ScheduleNumber} submitted by {SubmittedBy}",
                schedule.ScheduleNumber, request.SubmittedBy);

            var items = await _itemRepository.FindAsync(i => i.DeductionScheduleId == schedule.Id);
            return await MapToDto(schedule, items.ToList());
        }

        public async Task<DeductionScheduleExportResult> ExportScheduleAsync(ExportDeductionScheduleRequest request)
        {
            var schedule = await _scheduleRepository.GetByIdAsync(request.ScheduleId);
            if (schedule == null)
                throw new InvalidOperationException("Schedule not found");

            var items = await _itemRepository.FindAsync(i => i.DeductionScheduleId == schedule.Id);

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

            var originalItems = await _itemRepository.FindAsync(i => i.DeductionScheduleId == scheduleId);

            // Create new version
            var newSchedule = new DeductionSchedule
            {
                ScheduleNumber = $"{originalSchedule.ScheduleNumber}_V{originalSchedule.Version + 1}",
                Month = originalSchedule.Month,
                Year = originalSchedule.Year,
                Status = "DRAFT",
                Version = originalSchedule.Version + 1,
                Notes = $"Version {originalSchedule.Version + 1} of {originalSchedule.ScheduleNumber}",
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
                    DeductionScheduleId = newSchedule.Id,
                    LoanId = item.LoanId,
                    MemberId = item.MemberId,
                    MemberNumber = item.MemberNumber,
                    MemberName = item.MemberName,
                    LoanNumber = item.LoanNumber,
                    DeductionAmount = item.DeductionAmount,
                    PrincipalAmount = item.PrincipalAmount,
                    InterestAmount = item.InterestAmount,
                    PenaltyAmount = item.PenaltyAmount,
                    OutstandingBalance = item.OutstandingBalance,
                    InstallmentNumber = item.InstallmentNumber,
                    EmployeeId = item.EmployeeId,
                    Department = item.Department,
                    Status = "PENDING",
                    CreatedBy = createdBy,
                    CreatedAt = DateTime.UtcNow
                };
                newItems.Add(newItem);
            }

            await _itemRepository.AddRangeAsync(newItems);

            newSchedule.TotalDeductionAmount = newItems.Sum(i => i.DeductionAmount);
            newSchedule.TotalLoansCount = newItems.Count;

            await _scheduleRepository.UpdateAsync(newSchedule);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Created new version {Version} of schedule {ScheduleNumber}",
                newSchedule.Version, originalSchedule.ScheduleNumber);

            return await MapToDto(newSchedule, newItems);
        }

        #region Helper Methods

        private async Task<string> GenerateScheduleNumberAsync(int month, int year)
        {
            var existingSchedules = await _scheduleRepository.FindAsync(s => 
                s.Month == month && s.Year == year);
            
            var count = existingSchedules.Count() + 1;
            return $"DS/{year}/{month:D2}/{count:D3}";
        }

        private int CalculateInstallmentNumber(Loan loan)
        {
            if (loan.DisbursementDate == null) return 1;
            
            var monthsSinceDisbursement = ((DateTime.UtcNow.Year - loan.DisbursementDate.Value.Year) * 12) +
                                         DateTime.UtcNow.Month - loan.DisbursementDate.Value.Month;
            
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
                TotalDeductionAmount = schedule.TotalDeductionAmount,
                TotalLoansCount = schedule.TotalLoansCount,
                ApprovedAt = schedule.ApprovedAt,
                ApprovedBy = schedule.ApprovedBy,
                SubmittedAt = schedule.SubmittedAt,
                SubmittedBy = schedule.SubmittedBy,
                Notes = schedule.Notes,
                Version = schedule.Version,
                FilePath = schedule.FilePath,
                CreatedAt = schedule.CreatedAt,
                CreatedBy = schedule.CreatedBy,
                Items = items.Select(i => new DeductionScheduleItemDto
                {
                    Id = i.Id,
                    MemberNumber = i.MemberNumber,
                    MemberName = i.MemberName,
                    LoanNumber = i.LoanNumber,
                    DeductionAmount = i.DeductionAmount,
                    PrincipalAmount = i.PrincipalAmount,
                    InterestAmount = i.InterestAmount,
                    PenaltyAmount = i.PenaltyAmount,
                    OutstandingBalance = i.OutstandingBalance,
                    InstallmentNumber = i.InstallmentNumber,
                    EmployeeId = i.EmployeeId,
                    Department = i.Department,
                    Status = i.Status
                }).ToList()
            });
        }

        #endregion
    }
}
