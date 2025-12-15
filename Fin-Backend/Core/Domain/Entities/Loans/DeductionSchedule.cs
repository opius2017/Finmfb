using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Loans;

/// <summary>
/// Represents a monthly deduction schedule for loan repayments
/// </summary>
public class DeductionSchedule : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string ScheduleNumber { get; set; } = string.Empty;

    public int Month { get; set; }

    public int Year { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "DRAFT"; // DRAFT, APPROVED, PROCESSED, CANCELLED

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    public int TotalMembers { get; set; }

    public DateTime? ApprovedDate { get; set; }

    [StringLength(450)]
    public string? ApprovedBy { get; set; }

    public DateTime? ProcessedDate { get; set; }

    [StringLength(450)]
    public string? ProcessedBy { get; set; }

    [StringLength(450)]
    public string? UpdatedBy { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    // Navigation property
    public virtual ICollection<DeductionScheduleItem> Items { get; set; } = new List<DeductionScheduleItem>();
}
