using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTech.Core.Domain.Entities.Loans
{
    /// <summary>
    /// Represents reconciliation of expected vs actual deductions
    /// Tracks variances between scheduled and actual payroll deductions
    /// </summary>
    public class DeductionReconciliation
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(50)]
        public string ReconciliationNumber { get; set; } = string.Empty; // Format: REC/YYYY/MM/NNN

        [Required]
        [ForeignKey(nameof(DeductionSchedule))]
        public string DeductionScheduleId { get; set; } = string.Empty;

        public DeductionSchedule? DeductionSchedule { get; set; }

        [Required]
        public int Month { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ExpectedAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ActualAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal VarianceAmount { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal VariancePercentage { get; set; }

        public int ExpectedCount { get; set; }

        public int ActualCount { get; set; }

        public int MatchedCount { get; set; }

        public int UnmatchedCount { get; set; }

        public int FailedCount { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "IN_PROGRESS"; // IN_PROGRESS, COMPLETED, FAILED

        [StringLength(500)]
        public string? ImportFilePath { get; set; }

        [StringLength(500)]
        public string? ReportFilePath { get; set; }

        public DateTime? CompletedAt { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime? UpdatedAt { get; set; }

        [StringLength(450)]
        public string? UpdatedBy { get; set; }
    }
}
