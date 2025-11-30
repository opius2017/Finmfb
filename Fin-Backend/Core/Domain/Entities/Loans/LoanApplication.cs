using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Loans;

/// <summary>
/// Represents a loan application submitted by a member
/// </summary>
public class LoanApplication : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string ApplicationNumber { get; set; } = string.Empty;

    [Required]
    [ForeignKey(nameof(Member))]
    public string MemberId { get; set; } = string.Empty;

    public Member? Member { get; set; }

    [Required]
    [ForeignKey(nameof(LoanProduct))]
    public Guid LoanProductId { get; set; }

    public LoanProduct? LoanProduct { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal RequestedAmount { get; set; }

    public int RequestedTenureMonths { get; set; }

    [Required]
    [StringLength(500)]
    public string Purpose { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? AdditionalInformation { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "DRAFT"; // DRAFT, SUBMITTED, UNDER_REVIEW, APPROVED, REJECTED, CANCELLED, DISBURSED

    public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;

    public DateTime? SubmittedDate { get; set; }

    public DateTime? ReviewedDate { get; set; }

    [StringLength(450)]
    public string? ReviewedBy { get; set; }

    public DateTime? ApprovedDate { get; set; }

    [StringLength(450)]
    public string? ApprovedBy { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? ApprovedAmount { get; set; }

    public int? ApprovedTenureMonths { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? ApprovedInterestRate { get; set; }

    public DateTime? RejectedDate { get; set; }

    [StringLength(450)]
    public string? RejectedBy { get; set; }

    [StringLength(1000)]
    public string? RejectionReason { get; set; }

    public DateTime? DisbursedDate { get; set; }

    [StringLength(450)]
    public string? DisbursedBy { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ProcessingFee { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal InsuranceFee { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal OtherFees { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal NetDisbursementAmount { get; set; }

    public int CurrentWorkflowStep { get; set; } = 1;

    [StringLength(1000)]
    public string? InternalNotes { get; set; }

    // Navigation properties
    public virtual ICollection<Guarantor> Guarantors { get; set; } = new List<Guarantor>();
    public virtual ICollection<CommitteeReview> CommitteeReviews { get; set; } = new List<CommitteeReview>();
    public virtual Loan? Loan { get; set; }
}
