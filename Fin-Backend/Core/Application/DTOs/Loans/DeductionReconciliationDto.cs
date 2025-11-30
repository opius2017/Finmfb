using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class DeductionReconciliationDto
    {
        public string Id { get; set; } = string.Empty;
        public string ReconciliationNumber { get; set; } = string.Empty;
        public string DeductionScheduleId { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal ExpectedAmount { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal VarianceAmount { get; set; }
        public decimal VariancePercentage { get; set; }
        public int ExpectedCount { get; set; }
        public int ActualCount { get; set; }
        public int MatchedCount { get; set; }
        public int UnmatchedCount { get; set; }
        public int FailedCount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<DeductionReconciliationItemDto> Items { get; set; } = new();
    }

    public class DeductionReconciliationItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string MemberNumber { get; set; } = string.Empty;
        public string MemberName { get; set; } = string.Empty;
        public string LoanNumber { get; set; } = string.Empty;
        public decimal ExpectedAmount { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal VarianceAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? PayrollReference { get; set; }
        public string? VarianceReason { get; set; }
        public string? ResolutionStatus { get; set; }
    }

    public class ImportActualDeductionsRequest
    {
        public string DeductionScheduleId { get; set; } = string.Empty;
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
        public string FileName { get; set; } = string.Empty;
        public string ImportedBy { get; set; } = string.Empty;
    }

    public class ActualDeductionRecord
    {
        public string MemberNumber { get; set; } = string.Empty;
        public string LoanNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? PayrollReference { get; set; }
        public string? Status { get; set; }
    }

    public class ReconciliationResult
    {
        public bool Success { get; set; }
        public string ReconciliationId { get; set; } = string.Empty;
        public int TotalRecords { get; set; }
        public int MatchedRecords { get; set; }
        public int VarianceRecords { get; set; }
        public int MissingRecords { get; set; }
        public int ExtraRecords { get; set; }
        public decimal TotalVariance { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class ResolveVarianceRequest
    {
        public string ReconciliationItemId { get; set; } = string.Empty;
        public string ResolutionNotes { get; set; } = string.Empty;
        public string ResolvedBy { get; set; } = string.Empty;
        public string Action { get; set; } = "RESOLVED"; // RESOLVED, ESCALATED, RETRY
    }
}
