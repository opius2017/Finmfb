using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Domain.Entities.Common;
using FinTech.Domain.Enums;

namespace FinTech.Domain.Entities.Loans;

public class LoanProduct : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string? ProductName { get; set; }
    
    [Required]
    [StringLength(20)]
    public string? ProductCode { get; set; }
    
    [Required]
    public LoanProductType ProductType { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(5,2)")]
    public decimal InterestRate { get; set; }
    
    [Required]
    public InterestCalculationMethod InterestCalculationMethod { get; set; }
    
    [Required]
    public RepaymentFrequency RepaymentFrequency { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal MinimumAmount { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal MaximumAmount { get; set; } = 0;
    
    public int MinimumTenorDays { get; set; } = 30;
    
    public int MaximumTenorDays { get; set; } = 365;
    
    [Column(TypeName = "decimal(5,2)")]
    public decimal ProcessingFeeRate { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal ProcessingFeeFlat { get; set; } = 0;
    
    [Column(TypeName = "decimal(5,2)")]
    public decimal InsuranceRate { get; set; } = 0;
    
    [Column(TypeName = "decimal(5,2)")]
    public decimal PenaltyRate { get; set; } = 0;
    
    public int GracePeriodDays { get; set; } = 0;
    
    [Required]
    public bool RequiresCollateral { get; set; } = false;
    
    [Required]
    public bool RequiresGuarantor { get; set; } = false;
    
    public int MinimumGuarantors { get; set; } = 0;
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    [Required]
    [StringLength(3)]
    public string? CurrencyCode { get; set; } = "NGN";
    
    [Required]
    public Guid PrincipalGLAccountId { get; set; }
    
    [Required]
    public Guid InterestGLAccountId { get; set; }
    
    [Required]
    public Guid FeesGLAccountId { get; set; }
    
    [Required]
    public Guid ProvisionGLAccountId { get; set; }
    
    [Required]
    public Guid TenantId { get; set; }
    
    public virtual ICollection<LoanAccount> LoanAccounts { get; set; } = [];
}