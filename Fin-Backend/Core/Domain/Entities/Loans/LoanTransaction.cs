using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Loans;

/// <summary>
/// Represents a transaction on a loan account
/// </summary>
public class LoanTransaction : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string TransactionNumber { get; set; } = string.Empty;

    [Required]
    [ForeignKey(nameof(Loan))]
    public string LoanId { get; set; } = string.Empty;

    public Loan? Loan { get; set; }

    [ForeignKey(nameof(LoanAccount))]
    public string? LoanAccountId { get; set; }

    public LoanAccount? LoanAccount { get; set; }

    [Required]
    [StringLength(50)]
    public string TransactionType { get; set; } = string.Empty; // DISBURSEMENT, REPAYMENT, INTEREST_ACCRUAL, FEE, PENALTY, REVERSAL

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PrincipalAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal InterestAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal FeeAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal FeesAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [StringLength(100)]
    public string? Reference { get; set; }

    [StringLength(100)]
    public string? TransactionReference { get; set; }

    public string? TenantId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PenaltyAmount { get; set; }

    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

    public DateTime ValueDate { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "PENDING"; // PENDING, COMPLETED, FAILED, REVERSED

    [StringLength(50)]
    public string? PaymentMethod { get; set; }

    [StringLength(100)]
    public string? PaymentReference { get; set; }

    [StringLength(450)]
    public string? ProcessedBy { get; set; }

    public DateTime? ProcessedDate { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    [ForeignKey(nameof(ReversalTransaction))]
    public string? ReversalTransactionId { get; set; }

    public LoanTransaction? ReversalTransaction { get; set; }

    public bool IsReversed { get; set; }

    public DateTime? ReversedDate { get; set; }

    [StringLength(450)]
    public string? ReversedBy { get; set; }

    [StringLength(1000)]
    public string? ReversalReason { get; set; }
    [StringLength(50)]
    public string? SourceAccountNumber { get; set; }

    [NotMapped]
    public string ReferenceNumber { get => Reference; set => Reference = value; }
}
