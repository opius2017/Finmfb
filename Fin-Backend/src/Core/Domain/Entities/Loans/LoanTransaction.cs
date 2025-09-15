using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Domain.Entities.Common;
using FinTech.Domain.Enums;

namespace FinTech.Domain.Entities.Loans;

public class LoanTransaction : BaseEntity
{
    [Required]
    public Guid LoanAccountId { get; set; }
    public virtual LoanAccount LoanAccount { get; set; } = null!;
    
    [Required]
    [StringLength(50)]
    public string TransactionReference { get; set; } = string.Empty;
    
    [Required]
    public LoanTransactionType TransactionType { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal PrincipalAmount { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal InterestAmount { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal FeesAmount { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal PenaltyAmount { get; set; } = 0;
    
    [Required]
    public DateTime TransactionDate { get; set; }
    
    [Required]
    public DateTime ValueDate { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Description { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? ExternalReference { get; set; }
    
    [Required]
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
    
    [StringLength(200)]
    public string? StatusReason { get; set; }
    
    public string? ProcessedBy { get; set; }
    public DateTime? ProcessedDate { get; set; }
    
    public string? AuthorizedBy { get; set; }
    public DateTime? AuthorizedDate { get; set; }
    
    [Required]
    public Guid TenantId { get; set; }
}