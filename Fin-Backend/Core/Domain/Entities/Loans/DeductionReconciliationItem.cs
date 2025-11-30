using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTech.Core.Domain.Entities.Loans
{
    /// <summary>
    /// Individual reconciliation item showing expected vs actual deduction
    /// </summary>
    public class DeductionReconciliationItem
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [ForeignKey(nameof(DeductionReconciliation))]
        public string DeductionReconciliationId { get; set; } = string.Empty;

        public DeductionReconciliation? DeductionReconciliation { get; set; }

        [ForeignKey(nameof(DeductionScheduleItem))]
        public string? DeductionScheduleItemId { get; set; }

        public DeductionScheduleItem? DeductionScheduleItem { get; set; }

        [Required]
        [StringLength(100)]
        public string MemberNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string MemberName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LoanNumber { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal ExpectedAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ActualAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal VarianceAmount { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "PENDING"; // MATCHED, VARIANCE, MISSING, EXTRA, FAILED

        [StringLength(50)]
        public string? PayrollReference { get; set; }

        [StringLength(1000)]
        public string? VarianceReason { get; set; }

        [StringLength(20)]
        public string? ResolutionStatus { get; set; } // PENDING, RESOLVED, ESCALATED

        public DateTime? ResolvedAt { get; set; }

        [StringLength(450)]
        public string? ResolvedBy { get; set; }

        [StringLength(1000)]
        public string? ResolutionNotes { get; set; }

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
