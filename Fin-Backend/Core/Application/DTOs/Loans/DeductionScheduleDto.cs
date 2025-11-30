using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class DeductionScheduleDto
    {
        public string Id { get; set; } = string.Empty;
        public string ScheduleNumber { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalDeductionAmount { get; set; }
        public int TotalLoansCount { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public string? SubmittedBy { get; set; }
        public string? Notes { get; set; }
        public int Version { get; set; }
        public string? FilePath { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public List<DeductionScheduleItemDto> Items { get; set; } = new();
    }

    public class DeductionScheduleItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string MemberNumber { get; set; } = string.Empty;
        public string MemberName { get; set; } = string.Empty;
        public string LoanNumber { get; set; } = string.Empty;
        public decimal DeductionAmount { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal PenaltyAmount { get; set; }
        public decimal OutstandingBalance { get; set; }
        public int InstallmentNumber { get; set; }
        public string? EmployeeId { get; set; }
        public string? Department { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class GenerateDeductionScheduleRequest
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string? Notes { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class ApproveDeductionScheduleRequest
    {
        public string ScheduleId { get; set; } = string.Empty;
        public string ApprovedBy { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class SubmitDeductionScheduleRequest
    {
        public string ScheduleId { get; set; } = string.Empty;
        public string SubmittedBy { get; set; } = string.Empty;
    }

    public class ExportDeductionScheduleRequest
    {
        public string ScheduleId { get; set; } = string.Empty;
        public string Format { get; set; } = "EXCEL"; // EXCEL, CSV, PDF
    }

    public class DeductionScheduleExportResult
    {
        public bool Success { get; set; }
        public string? FilePath { get; set; }
        public byte[]? FileContent { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public string? Message { get; set; }
    }
}
