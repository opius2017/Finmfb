using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class LoanDelinquencyDto
    {
        public string Id { get; set; } = string.Empty;
        public string LoanId { get; set; } = string.Empty;
        public string LoanNumber { get; set; } = string.Empty;
        public string MemberNumber { get; set; } = string.Empty;
        public string MemberName { get; set; } = string.Empty;
        public DateTime CheckDate { get; set; }
        public int DaysOverdue { get; set; }
        public decimal OverdueAmount { get; set; }
        public decimal PenaltyApplied { get; set; }
        public string Classification { get; set; } = string.Empty;
        public string? PreviousClassification { get; set; }
        public bool ClassificationChanged { get; set; }
        public bool NotificationSent { get; set; }
        public string NotificationType { get; set; } = string.Empty;
    }

    public class DelinquencyCheckResult
    {
        public bool IsOverdue { get; set; }
        public int DaysOverdue { get; set; }
        public decimal OverdueAmount { get; set; }
        public decimal PenaltyAmount { get; set; }
        public string Classification { get; set; } = string.Empty;
        public bool ClassificationChanged { get; set; }
        public string? PreviousClassification { get; set; }
        public bool NotificationRequired { get; set; }
        public string? NotificationType { get; set; }
        public string? Message { get; set; }
    }

    public class DailyDelinquencyCheckResult
    {
        public DateTime CheckDate { get; set; }
        public int LoansChecked { get; set; }
        public int DelinquentLoans { get; set; }
        public decimal TotalOverdueAmount { get; set; }
        public decimal TotalPenaltiesApplied { get; set; }
        public int NotificationsSent { get; set; }
        public int ClassificationChanges { get; set; }
        public List<LoanDelinquencyDto> DelinquentLoansList { get; set; } = new();
        public string? Message { get; set; }
    }

    public class DelinquencyReportRequest
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Classification { get; set; }
        public int? MinDaysOverdue { get; set; }
        public decimal? MinOverdueAmount { get; set; }
    }

    public class DelinquencySummaryDto
    {
        public int TotalDelinquentLoans { get; set; }
        public decimal TotalOverdueAmount { get; set; }
        public decimal TotalPenalties { get; set; }
        public int PerformingLoans { get; set; }
        public int SpecialMentionLoans { get; set; }
        public int SubstandardLoans { get; set; }
        public int DoubtfulLoans { get; set; }
        public int LossLoans { get; set; }
        public decimal AverageOverdueDays { get; set; }
        public decimal DelinquencyRate { get; set; }
    }
}
