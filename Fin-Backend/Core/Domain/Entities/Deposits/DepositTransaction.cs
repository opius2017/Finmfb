using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Enums;

namespace FinTech.Core.Domain.Entities.Deposits;

public class DepositTransaction : BaseEntity
{
    [Required]
    public Guid AccountId { get; set; }
    public virtual DepositAccount Account { get; set; } = null!;
    
    [Required]
    [StringLength(50)]
    public string TransactionReference { get; set; } = string.Empty;
    
    [Required]
    public TransactionType TransactionType { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal BalanceAfter { get; set; }
    
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
