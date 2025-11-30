using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Loans;

/// <summary>
/// Represents an individual item in a deduction schedule
/// </summary>
public class DeductionScheduleItem : BaseEntity
{
    [Required]
    [ForeignKey(nameof(Schedule))]
    public Guid ScheduleId { get; set; }

    public DeductionSchedule? Schedule { get; set; }

    [Required]
    [ForeignKey(nameof(Loan))]
    public string LoanId { get; set; } = string.Empty;

    public Loan? Loan { get; set; }

    [Required]
    [ForeignKey(nameof(Member))]
    public string MemberId { get; set; } = string.Empty;

    public Member? Member { get; set; }

    public int InstallmentNumber { get; set; }

    public DateTime DueDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PrincipalAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal InterestAmount { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "PENDING"; // PENDING, PROCESSED, FAILED, REVERSED

    public DateTime? ProcessedDate { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}
