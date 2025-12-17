using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Accounting
{
    public class FinancialPeriodDto
    {
        public string Id { get; set; } = string.Empty;
        public string PeriodCode { get; set; } = string.Empty;
        public string PeriodName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsClosed { get; set; }
        public DateTime? ClosedDate { get; set; }
        public string ClosedBy { get; set; } = string.Empty;
        public int FiscalYear { get; set; }
        public int FiscalMonth { get; set; }
        public bool IsAdjustmentPeriod { get; set; }
    }

    public class ClosePeriodRequestDto
    {
        public string PeriodId { get; set; } = string.Empty;
        public bool ForceClose { get; set; }
    }

    public class PeriodClosingStatusDto
    {
        public string FinancialPeriodId { get; set; } = string.Empty;
        public string FinancialPeriodName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ClosingStatus { get; set; } = string.Empty;
        public bool IsClosed { get; set; }
        public DateTime? ClosedAt { get; set; }
        public string ClosedBy { get; set; } = string.Empty;
        public List<string> ValidationErrors { get; set; } = new List<string>();
        public bool CanRollBack { get; set; }
    }

    public class PeriodClosingSummaryDto
    {
        public string PeriodId { get; set; } = string.Empty;
        public string PeriodCode { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalDebits { get; set; }
        public decimal TotalCredits { get; set; }
        public bool IsBalanced { get; set; }
    }

    public class PeriodClosingResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new List<string>();
        public DateTime ClosingDate { get; set; }
    }

    public class PeriodClosingPrerequisiteCheckDto
    {
        public bool ArePrerequisitesMet { get; set; }
        public List<string> PendingItems { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }
}
