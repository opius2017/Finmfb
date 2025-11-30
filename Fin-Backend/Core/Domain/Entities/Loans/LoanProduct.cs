using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Loans;

/// <summary>
/// Represents a loan product offered by the cooperative
/// </summary>
public class LoanProduct : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    [StringLength(50)]
    public string LoanType { get; set; } = string.Empty; // PERSONAL, EMERGENCY, COMMODITY, SPECIAL

    [Column(TypeName = "decimal(18,2)")]
    public decimal MinimumAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal MaximumAmount { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal InterestRate { get; set; }

    public int MinimumTenureMonths { get; set; }

    public int MaximumTenureMonths { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal ProcessingFeePercentage { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ProcessingFeeFixed { get; set; }

    public bool RequiresGuarantor { get; set; }

    public int MinimumGuarantors { get; set; }

    public bool RequiresCollateral { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal MaxLoanToIncomeRatio { get; set; }

    public int MinimumMembershipMonths { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal MinimumSavingsBalance { get; set; }

    public bool IsActive { get; set; } = true;

    [StringLength(50)]
    public string RepaymentFrequency { get; set; } = "MONTHLY"; // MONTHLY, QUARTERLY, ANNUALLY

    [StringLength(50)]
    public string InterestCalculationMethod { get; set; } = "REDUCING_BALANCE"; // FLAT, REDUCING_BALANCE

    public bool AllowEarlyRepayment { get; set; } = true;

    [Column(TypeName = "decimal(5,2)")]
    public decimal EarlyRepaymentPenaltyPercentage { get; set; }

    [StringLength(1000)]
    public string? EligibilityCriteria { get; set; }

    [StringLength(1000)]
    public string? RequiredDocuments { get; set; }

    public int ApprovalWorkflowSteps { get; set; } = 1;

    // Navigation properties
    public virtual ICollection<LoanApplication> Applications { get; set; } = new List<LoanApplication>();
}
