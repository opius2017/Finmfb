using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Loans;

/// <summary>
/// Represents a loan repayment/installment
/// </summary>
public class Repayment : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string RepaymentNumber { get; set; } = string.Empty;

    [Required]
    [ForeignKey(nameof(Loan))]
    public string LoanId { get; set; } = string.Empty;

    public Loan? Loan { get; set; }

    public int InstallmentNumber { get; set; }

    public DateTime DueDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ScheduledAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ScheduledPrincipal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ScheduledInterest { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PaidAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PaidPrincipal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PaidInterest { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal OutstandingAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PenaltyAmount { get; set; }

    public DateTime? PaymentDate { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "PENDING"; // PENDING, PARTIAL, PAID, OVERDUE, WAIVED

    public int DaysOverdue { get; set; }

    [StringLength(50)]
    public string? PaymentMethod { get; set; }

    [StringLength(100)]
    public string? PaymentReference { get; set; }

    [StringLength(450)]
    public string? ProcessedBy { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal OpeningBalance { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ClosingBalance { get; set; }

    public bool IsWaived { get; set; }

    public DateTime? WaivedDate { get; set; }

    [StringLength(450)]
    public string? WaivedBy { get; set; }

    [StringLength(1000)]
    public string? WaiverReason { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}
