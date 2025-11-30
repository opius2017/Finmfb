using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Loans;

/// <summary>
/// Represents an active loan
/// </summary>
public class Loan : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string LoanNumber { get; set; } = string.Empty;

    [Required]
    [ForeignKey(nameof(Member))]
    public string MemberId { get; set; } = string.Empty;

    public Member? Member { get; set; }

    [Required]
    [ForeignKey(nameof(LoanProduct))]
    public Guid LoanProductId { get; set; }

    public LoanProduct? LoanProduct { get; set; }

    [ForeignKey(nameof(LoanApplication))]
    public Guid? LoanApplicationId { get; set; }

    public LoanApplication? LoanApplication { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PrincipalAmount { get; set; }

    [Required]
    [Column(TypeName = "decimal(5,2)")]
    public decimal InterestRate { get; set; }

    public int TenureMonths { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal MonthlyInstallment { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalInterest { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalRepayableAmount { get; set; }

    public DateTime DisbursementDate { get; set; }

    public DateTime MaturityDate { get; set; }

    public DateTime FirstInstallmentDate { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "ACTIVE"; // ACTIVE, FULLY_PAID, WRITTEN_OFF, RESTRUCTURED, CLOSED

    [Column(TypeName = "decimal(18,2)")]
    public decimal OutstandingPrincipal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal OutstandingInterest { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalOutstanding { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPaid { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PrincipalPaid { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal InterestPaid { get; set; }

    public int InstallmentsPaid { get; set; }

    public int InstallmentsRemaining { get; set; }

    public DateTime? LastPaymentDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? LastPaymentAmount { get; set; }

    public DateTime? NextPaymentDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? NextPaymentAmount { get; set; }

    public int DaysOverdue { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal OverdueAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PenaltyAmount { get; set; }

    [StringLength(50)]
    public string DelinquencyStatus { get; set; } = "CURRENT"; // CURRENT, EARLY, MODERATE, SEVERE, DEFAULT

    [StringLength(50)]
    public string RepaymentFrequency { get; set; } = "MONTHLY";

    [StringLength(50)]
    public string InterestCalculationMethod { get; set; } = "REDUCING_BALANCE";

    public bool IsRestructured { get; set; }

    public DateTime? RestructuredDate { get; set; }

    [StringLength(1000)]
    public string? RestructureReason { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    // Navigation properties
    public virtual ICollection<Repayment> Repayments { get; set; } = new List<Repayment>();
    public virtual ICollection<LoanTransaction> Transactions { get; set; } = new List<LoanTransaction>();
    public virtual ICollection<Guarantor> Guarantors { get; set; } = new List<Guarantor>();
    public virtual LoanDelinquency? Delinquency { get; set; }
}
