using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Domain.Entities.Common;
using FinTech.Domain.Enums;

namespace FinTech.Domain.Entities.Loans;

public class LoanCollateral : BaseEntity
{
    [Required]
    public Guid LoanAccountId { get; set; }
    public virtual LoanAccount LoanAccount { get; set; } = null!;
    
    [Required]
    [StringLength(100)]
    public string CollateralType { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal EstimatedValue { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal ForcedSaleValue { get; set; } = 0;
    
    [Column(TypeName = "decimal(5,2)")]
    public decimal HaircutPercentage { get; set; } = 0;
    
    [StringLength(100)]
    public string? Location { get; set; }
    
    [StringLength(100)]
    public string? DocumentReference { get; set; }
    
    public DateTime? ValuationDate { get; set; }
    
    [StringLength(100)]
    public string? ValuedBy { get; set; }
    
    public DateTime? NextValuationDate { get; set; }
    
    [Required]
    public CollateralStatus Status { get; set; } = CollateralStatus.Active;
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    [Required]
    public Guid TenantId { get; set; }
}