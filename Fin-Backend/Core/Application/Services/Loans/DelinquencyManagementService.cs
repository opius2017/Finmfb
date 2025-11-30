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
    public class DelinquencyManagementService : IDelinquencyManagementService
    {
        private readonly IRepository<Loan> _loanRepository;
        private readonly IRepository<LoanDelinquency> _delinquencyRepository;
        private readonly IRepository<Member> _memberRepository;
        private readonly IRepository<GuarantorConsent> _guarantorConsentRepository;
        private readonly ILoanCalculatorService _calculatorService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DelinquencyManagementService> _logger;

        // CBN Classification thresholds (days)
        private const int SPECIAL_MENTION_DAYS = 31;
        private const int SUBSTANDARD_DAYS = 91;
        private const int DOUBTFUL_DAYS = 181;
        private const int LOSS_DAYS = 361;

        public DelinquencyManagementService(
            IRepository<Loan> loanRepository,
            IRepository<LoanDelinquency> delinquencyRepository,
            IRepository<Member> memberRepository,
            IRepository<GuarantorConsent> guarantorConsentRepository,
            ILoanCalculatorService calculatorService,
            IUnitOfWork unitOfWork,
            ILogger<DelinquencyManagementService> logger)
        {
            _loanRepository = loanRepository;
            _delinquencyRepository = delinquencyRepository;
            _memberRepository = memberRepository;
            _guarantorConsentRepository = guarantorConsentRepository;
            _calculatorService = calculatorService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<DelinquencyCheckResult> CheckLoanDelinquencyAsync(string loanId)
        {
            var loan = await _loanRepository.GetByIdAsync(loanId);
            if (loan == null)
                throw new InvalidOperationException("Loan not found");

            var result = new DelinquencyCheckResult();

            // Check if loan is overdue
            if (loan.NextPaymentDate.HasValue && loan.NextPaymentDate.Value < DateTime.UtcNow)
            {
                result.IsOverdue = true;
                result.DaysOverdue = (DateTime.UtcNow - loan.NextPaymentDate.Value).Days;
                result.OverdueAmount = loan.MonthlyInstallment; // Simplified

                // Calculate penalty
                var penaltyRate = 0.1m; // 0.1% per day
                result.PenaltyAmount = _calculatorService.CalculatePenalty(
                    result.OverdueAmount, result.DaysOverdue, penaltyRate);

                // Determine classification
                var previousClassification = loan.Classification;
                result.Classification = DetermineClassification(result.DaysOverdue);
                result.PreviousClassification = previousClassification;
                result.ClassificationChanged = result.Classification != previousClassification;

                // Determine notification type
                result.NotificationRequired = true;
                result.NotificationType = DetermineNotificationType(result.DaysOverdue);

                result.Message = $"Loan is {result.DaysOverdue} days overdue. Classification: {result.Classification}";
            }
            else
            {
                result.IsOverdue = false;
                result.Classification = "PERFORMING";
                result.Message = "Loan is current";
            }

            return result;
        }

        public async Task<DailyDelinquencyCheckResult> PerformDailyDelinquencyCheckAsync()
        {
            _logger.LogInformation("Starting daily delinquency check");

            var result = new DailyDelinquencyCheckResult
            {
                CheckDate = DateTime.UtcNow
            };

            // Get all active loans
            var activeLoans = await _loanRepository.FindAsync(l => l.LoanStatus == "ACTIVE");
            result.LoansChecked = activeLoans.Count();

            var delinquentLoans = new List<LoanDelinquencyDto>();

            foreach (var loan in activeLoans)
            {
                var checkResult = await CheckLoanDelinquencyAsync(loan.Id);

                if (checkResult.IsOverdue)
                {
                    // Apply penalty
                    await ApplyPenaltyAsync(loan.Id, checkResult.PenaltyAmount, "SYSTEM");

                    // Update classification
                    if (checkResult.ClassificationChanged)
                    {
                        await UpdateLoanClassificationAsync(loan.Id, checkResult.DaysOverdue);
                        result.ClassificationChanges++;
                    }

                    // Send notification
                    if (checkResult.NotificationRequired)
                    {
                        await SendDelinquencyNotificationAsync(loan.Id, checkResult.NotificationType!);
                        result.NotificationsSent++;
                    }

                    // Record delinquency
                    var delinquency = new LoanDelinquency
                    {
                        LoanId = loan.Id,
                        CheckDate = DateTime.UtcNow,
                        DaysOverdue = checkResult.DaysOverdue,
                        OverdueAmount = checkResult.OverdueAmount,
                        PenaltyApplied = checkResult.PenaltyAmount,
                        Classification = checkResult.Classification,
                        PreviousClassification = checkResult.PreviousClassification,
                        ClassificationChanged = checkResult.ClassificationChanged,
                        NotificationSent = checkResult.NotificationRequired,
                        NotificationType = checkResult.NotificationType ?? "NONE",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "SYSTEM"
                    };

                    await _delinquencyRepository.AddAsync(delinquency);

                    result.TotalOverdueAmount += checkResult.OverdueAmount;
                    result.TotalPenaltiesApplied += checkResult.PenaltyAmount;

                    var member = await _memberRepository.GetByIdAsync(loan.MemberId);
                    delinquentLoans.Add(new LoanDelinquencyDto
                    {
                        Id = delinquency.Id,
                        LoanId = loan.Id,
                        LoanNumber = loan.LoanNumber,
                        MemberNumber = member?.MemberNumber ?? "",
                        MemberName = member != null ? $"{member.FirstName} {member.LastName}" : "",
                        CheckDate = delinquency.CheckDate,
                        DaysOverdue = delinquency.DaysOverdue,
                        OverdueAmount = delinquency.OverdueAmount,
                        PenaltyApplied = delinquency.PenaltyApplied,
                        Classification = delinquency.Classification,
                        PreviousClassification = delinquency.PreviousClassification,
                        ClassificationChanged = delinquency.ClassificationChanged,
                        NotificationSent = delinquency.NotificationSent,
                        NotificationType = delinquency.NotificationType
                    });
                }
            }

            result.DelinquentLoans = delinquentLoans.Count;
            result.DelinquentLoansList = delinquentLoans;

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Daily delinquency check completed. {DelinquentCount} delinquent loans found",
                result.DelinquentLoans);

            result.Message = $"Checked {result.LoansChecked} loans, found {result.DelinquentLoans} delinquent";

            return result;
        }

        public async Task<List<LoanDelinquencyDto>> GetDelinquentLoansAsync(DelinquencyReportRequest request)
        {
            var query = await _delinquencyRepository.GetAllAsync();

            if (request.StartDate.HasValue)
                query = query.Where(d => d.CheckDate >= request.StartDate.Value);

            if (request.EndDate.HasValue)
                query = query.Where(d => d.CheckDate <= request.EndDate.Value);

            if (!string.IsNullOrEmpty(request.Classification))
                query = query.Where(d => d.Classification == request.Classification);

            if (request.MinDaysOverdue.HasValue)
                query = query.Where(d => d.DaysOverdue >= request.MinDaysOverdue.Value);

            if (request.MinOverdueAmount.HasValue)
                query = query.Where(d => d.OverdueAmount >= request.MinOverdueAmount.Value);

            var delinquencies = query.OrderByDescending(d => d.CheckDate).ToList();

            var result = new List<LoanDelinquencyDto>();
            foreach (var delinquency in delinquencies)
            {
                var loan = await _loanRepository.GetByIdAsync(delinquency.LoanId);
                var member = loan != null ? await _memberRepository.GetByIdAsync(loan.MemberId) : null;

                result.Add(new LoanDelinquencyDto
                {
                    Id = delinquency.Id,
                    LoanId = delinquency.LoanId,
                    LoanNumber = loan?.LoanNumber ?? "",
                    MemberNumber = member?.MemberNumber ?? "",
                    MemberName = member != null ? $"{member.FirstName} {member.LastName}" : "",
                    CheckDate = delinquency.CheckDate,
                    DaysOverdue = delinquency.DaysOverdue,
                    OverdueAmount = delinquency.OverdueAmount,
                    PenaltyApplied = delinquency.PenaltyApplied,
                    Classification = delinquency.Classification,
                    PreviousClassification = delinquency.PreviousClassification,
                    ClassificationChanged = delinquency.ClassificationChanged,
                    NotificationSent = delinquency.NotificationSent,
                    NotificationType = delinquency.NotificationType
                });
            }

            return result;
        }

        public async Task<DelinquencySummaryDto> GetDelinquencySummaryAsync()
        {
            var allLoans = await _loanRepository.FindAsync(l => l.LoanStatus == "ACTIVE");
            var delinquencies = await _delinquencyRepository.GetAllAsync();
            var latestDelinquencies = delinquencies
                .GroupBy(d => d.LoanId)
                .Select(g => g.OrderByDescending(d => d.CheckDate).First())
                .ToList();

            var summary = new DelinquencySummaryDto
            {
                TotalDelinquentLoans = latestDelinquencies.Count,
                TotalOverdueAmount = latestDelinquencies.Sum(d => d.OverdueAmount),
                TotalPenalties = latestDelinquencies.Sum(d => d.PenaltyApplied),
                PerformingLoans = latestDelinquencies.Count(d => d.Classification == "PERFORMING"),
                SpecialMentionLoans = latestDelinquencies.Count(d => d.Classification == "SPECIAL_MENTION"),
                SubstandardLoans = latestDelinquencies.Count(d => d.Classification == "SUBSTANDARD"),
                DoubtfulLoans = latestDelinquencies.Count(d => d.Classification == "DOUBTFUL"),
                LossLoans = latestDelinquencies.Count(d => d.Classification == "LOSS"),
                AverageOverdueDays = latestDelinquencies.Any() ? 
                    (decimal)latestDelinquencies.Average(d => d.DaysOverdue) : 0,
                DelinquencyRate = allLoans.Any() ? 
                    (decimal)latestDelinquencies.Count / allLoans.Count() * 100 : 0
            };

            return summary;
        }

        public async Task<bool> ApplyPenaltyAsync(string loanId, decimal penaltyAmount, string appliedBy)
        {
            var loan = await _loanRepository.GetByIdAsync(loanId);
            if (loan == null) return false;

            loan.PenaltyAmount += penaltyAmount;
            loan.OutstandingBalance += penaltyAmount;
            loan.UpdatedAt = DateTime.UtcNow;
            loan.UpdatedBy = appliedBy;

            await _loanRepository.UpdateAsync(loan);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Applied penalty of {Amount:C} to loan {LoanNumber}",
                penaltyAmount, loan.LoanNumber);

            return true;
        }

        public async Task<string> UpdateLoanClassificationAsync(string loanId, int daysOverdue)
        {
            var loan = await _loanRepository.GetByIdAsync(loanId);
            if (loan == null)
                throw new InvalidOperationException("Loan not found");

            var newClassification = DetermineClassification(daysOverdue);
            loan.Classification = newClassification;
            loan.DaysInArrears = daysOverdue;
            loan.UpdatedAt = DateTime.UtcNow;
            loan.UpdatedBy = "SYSTEM";

            await _loanRepository.UpdateAsync(loan);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Updated loan {LoanNumber} classification to {Classification}",
                loan.LoanNumber, newClassification);

            return newClassification;
        }

        public async Task<bool> SendDelinquencyNotificationAsync(string loanId, string notificationType)
        {
            // Notification sending will be implemented with notification service
            _logger.LogInformation("Sending {NotificationType} notification for loan {LoanId}",
                notificationType, loanId);

            // Placeholder for actual notification implementation
            await Task.CompletedTask;
            return true;
        }

        public async Task<List<LoanDelinquencyDto>> GetLoanDelinquencyHistoryAsync(string loanId)
        {
            var delinquencies = await _delinquencyRepository.FindAsync(d => d.LoanId == loanId);
            var loan = await _loanRepository.GetByIdAsync(loanId);
            var member = loan != null ? await _memberRepository.GetByIdAsync(loan.MemberId) : null;

            return delinquencies.OrderByDescending(d => d.CheckDate).Select(d => new LoanDelinquencyDto
            {
                Id = d.Id,
                LoanId = d.LoanId,
                LoanNumber = loan?.LoanNumber ?? "",
                MemberNumber = member?.MemberNumber ?? "",
                MemberName = member != null ? $"{member.FirstName} {member.LastName}" : "",
                CheckDate = d.CheckDate,
                DaysOverdue = d.DaysOverdue,
                OverdueAmount = d.OverdueAmount,
                PenaltyApplied = d.PenaltyApplied,
                Classification = d.Classification,
                PreviousClassification = d.PreviousClassification,
                ClassificationChanged = d.ClassificationChanged,
                NotificationSent = d.NotificationSent,
                NotificationType = d.NotificationType
            }).ToList();
        }

        public async Task<List<LoanDelinquencyDto>> IdentifyOverdueLoansAsync(int minDaysOverdue = 1)
        {
            var activeLoans = await _loanRepository.FindAsync(l => 
                l.LoanStatus == "ACTIVE" && 
                l.NextPaymentDate.HasValue &&
                l.NextPaymentDate.Value < DateTime.UtcNow);

            var overdueLoans = new List<LoanDelinquencyDto>();

            foreach (var loan in activeLoans)
            {
                var daysOverdue = (DateTime.UtcNow - loan.NextPaymentDate!.Value).Days;
                if (daysOverdue >= minDaysOverdue)
                {
                    var member = await _memberRepository.GetByIdAsync(loan.MemberId);
                    overdueLoans.Add(new LoanDelinquencyDto
                    {
                        LoanId = loan.Id,
                        LoanNumber = loan.LoanNumber,
                        MemberNumber = member?.MemberNumber ?? "",
                        MemberName = member != null ? $"{member.FirstName} {member.LastName}" : "",
                        DaysOverdue = daysOverdue,
                        OverdueAmount = loan.MonthlyInstallment,
                        Classification = loan.Classification
                    });
                }
            }

            return overdueLoans;
        }

        public async Task<decimal> CalculateDelinquencyRateAsync()
        {
            var allLoans = await _loanRepository.FindAsync(l => l.LoanStatus == "ACTIVE");
            var totalLoans = allLoans.Count();

            if (totalLoans == 0) return 0;

            var overdueLoans = await IdentifyOverdueLoansAsync();
            return (decimal)overdueLoans.Count / totalLoans * 100;
        }

        #region Helper Methods

        private string DetermineClassification(int daysOverdue)
        {
            if (daysOverdue >= LOSS_DAYS) return "LOSS";
            if (daysOverdue >= DOUBTFUL_DAYS) return "DOUBTFUL";
            if (daysOverdue >= SUBSTANDARD_DAYS) return "SUBSTANDARD";
            if (daysOverdue >= SPECIAL_MENTION_DAYS) return "SPECIAL_MENTION";
            return "PERFORMING";
        }

        private string DetermineNotificationType(int daysOverdue)
        {
            if (daysOverdue >= 30) return "FINAL_NOTICE";
            if (daysOverdue >= 7) return "REMINDER_7_DAYS";
            if (daysOverdue >= 3) return "REMINDER_3_DAYS";
            return "NONE";
        }

        #endregion
    }
}
