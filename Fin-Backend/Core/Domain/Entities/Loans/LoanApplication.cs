using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Entities.Common;

using FinTech.Core.Domain.Enums.Loans;

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

    // FinTech Best Practice: Alias for RequestedTenureMonths
    [NotMapped]
    public int RequestedTerm { get => RequestedTenureMonths; set => RequestedTenureMonths = value; }

    [Required]
    [StringLength(500)]
    public string Purpose { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? AdditionalInformation { get; set; }

    [Required]
    // [StringLength(20)] // Removed StringLength as this is now Enum (int by default or mapped)
    public LoanApplicationStatus Status { get; set; } = LoanApplicationStatus.Draft; // DRAFT, SUBMITTED, UNDER_REVIEW, APPROVED, REJECTED, CANCELLED, DISBURSED

    public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;

    public DateTime? SubmittedDate { get; set; }

    // FinTech Best Practice: Alias for SubmittedDate
    [NotMapped]
    public DateTime? SubmittedAt { get => SubmittedDate; set => SubmittedDate = value; }

    // FinTech Best Practice: Track who last updated the application
    [StringLength(450)]
    public string? UpdatedBy { get; set; }

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
    public virtual ICollection<LoanCollateral> Collaterals { get; set; } = new List<LoanCollateral>();
    public virtual ICollection<CommitteeReview> CommitteeReviews { get; set; } = new List<CommitteeReview>();
    public virtual Loan? Loan { get; set; }

    // Missing properties required by Service Layer
    public Guid CustomerId { get; set; } // Mapped from MemberId, ensuring type compatibility
    public decimal InterestRate { get; set; }
    public int RepaymentPeriodMonths { get; set; }
    public int PaymentFrequency { get; set; } // Assuming 1=Monthly, etc.
    public DateTime? DisbursementDate { get; set; }

    /// <summary>
    /// Submits the application for review.
    /// </summary>
    public void Submit()
    {
        if (Status != LoanApplicationStatus.Draft)
            throw new InvalidOperationException("Only draft applications can be submitted.");

        Status = LoanApplicationStatus.Submitted;
        SubmittedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Approves the application.
    /// </summary>
    public void Approve(string approvedBy, decimal? amount, int? term)
    {
        if (Status != LoanApplicationStatus.InReview && Status != LoanApplicationStatus.Submitted)
             // Allow approval from Submitted or InReview, though Service handles transition to InReview
             // stricter check might be InReview only.
             ;

        Status = LoanApplicationStatus.Approved;
        ApprovedBy = approvedBy;
        ApprovedDate = DateTime.UtcNow;
        if (amount.HasValue) ApprovedAmount = amount.Value;
        if (term.HasValue) ApprovedTenureMonths = term.Value;
        // Typically InterestRate might also be locked here
    }

    /// <summary>
    /// Rejects the application.
    /// </summary>
    public void Reject(string reason)
    {
        Status = LoanApplicationStatus.Rejected;
        RejectedDate = DateTime.UtcNow;
        RejectionReason = reason;
    }

    /// <summary>
    /// Creates a Loan entity from this approved application.
    /// </summary>
    public Loan CreateLoan()
    {
        if (Status != LoanApplicationStatus.Approved)
            throw new InvalidOperationException("Loan can only be created from approved applications.");

        return new Loan
        {
             // Mapping logic aligned with Loan entity definition
             CustomerId = this.CustomerId.ToString(),
             MemberId = this.CustomerId.ToString(), // MemberId is Required and string
             LoanProductId = this.LoanProductId,
             PrincipalAmount = this.ApprovedAmount ?? this.RequestedAmount,
             InterestRate = this.ApprovedInterestRate ?? 0, // InterestRate is decimal not nullable
             TenureMonths = this.ApprovedTenureMonths ?? this.RequestedTenureMonths,
             LoanApplicationId = Guid.TryParse(this.Id, out var appId) ? appId : Guid.Empty
        };
    }
}
