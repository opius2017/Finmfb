using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Loans;

/// <summary>
/// Represents a committee review for a loan application
/// </summary>
public class CommitteeReview : BaseEntity
{
    [Required]
    [ForeignKey(nameof(LoanApplication))]
    public Guid LoanApplicationId { get; set; }

    public LoanApplication? LoanApplication { get; set; }

    [Required]
    [StringLength(450)]
    public string ReviewerId { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string ReviewerName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? ReviewerRole { get; set; }

    [Required]
    [StringLength(20)]
    public string Decision { get; set; } = "PENDING"; // PENDING, APPROVED, REJECTED, DEFERRED

    public DateTime ReviewDate { get; set; } = DateTime.UtcNow;

    [StringLength(2000)]
    public string? Comments { get; set; }

    [StringLength(1000)]
    public string? Recommendations { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? RecommendedAmount { get; set; }

    public int? RecommendedTenureMonths { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? RecommendedInterestRate { get; set; }

    public int VotingRound { get; set; } = 1;

    public int ReviewSequence { get; set; }

    [Required]
    [StringLength(20)]
    public string ReviewLevel { get; set; } = "COMMITTEE"; // COMMITTEE, MANAGEMENT, BOARD

    [StringLength(1000)]
    public string? RejectionReason { get; set; }

    [StringLength(1000)]
    public string? DeferralReason { get; set; }

    public DateTime? NextReviewDate { get; set; }

    public bool IsFinalDecision { get; set; }

    [StringLength(1000)]
    public string? InternalNotes { get; set; }
}
