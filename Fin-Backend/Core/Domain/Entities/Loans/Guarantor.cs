using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Loans;

/// <summary>
/// Represents a guarantor for a loan application or loan
/// </summary>
public class Guarantor : BaseEntity
{
    [ForeignKey(nameof(LoanApplication))]
    public Guid? LoanApplicationId { get; set; }

    public LoanApplication? LoanApplication { get; set; }

    [ForeignKey(nameof(Loan))]
    public string? LoanId { get; set; }

    public Loan? Loan { get; set; }

    [Required]
    [ForeignKey(nameof(GuarantorMember))]
    public string GuarantorMemberId { get; set; } = string.Empty;

    public Member? GuarantorMember { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal GuaranteeAmount { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "PENDING"; // PENDING, APPROVED, REJECTED, RELEASED

    public DateTime RequestDate { get; set; } = DateTime.UtcNow;

    public DateTime? ResponseDate { get; set; }

    [StringLength(1000)]
    public string? Comments { get; set; }

    [StringLength(450)]
    public string? RequestedBy { get; set; }

    [StringLength(450)]
    public string? ProcessedBy { get; set; }

    public bool ConsentGiven { get; set; }

    public DateTime? ConsentDate { get; set; }

    [StringLength(500)]
    public string? ConsentDocumentPath { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CurrentExposure { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal MaximumExposure { get; set; }

    public bool IsReleased { get; set; }

    public DateTime? ReleasedDate { get; set; }

    [StringLength(450)]
    public string? ReleasedBy { get; set; }

    [StringLength(1000)]
    public string? ReleaseNotes { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}
