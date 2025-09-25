using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Enums;

namespace FinTech.Core.Domain.Entities.GeneralLedger;

public class GeneralLedgerEntry : BaseEntity
{
    [Required]
    public Guid AccountId { get; set; }
    public virtual ChartOfAccounts Account { get; set; } = null!;
    
    [Required]
    public Guid TransactionId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string TransactionReference { get; set; } = string.Empty;
    
    [Required]
    public DateTime TransactionDate { get; set; }
    
    [Required]
    public EntryType EntryType { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    
    [Required]
    [StringLength(3)]
    public string CurrencyCode { get; set; } = "NGN";
    
    [Column(TypeName = "decimal(10,4)")]
    public decimal ExchangeRate { get; set; } = 1.0m;
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal BaseAmount { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [StringLength(100)]
    public string? Reference { get; set; }
    
    public Guid? DocumentId { get; set; }
    
    [Required]
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
    
    public DateTime? PostedDate { get; set; }
    
    public string? PostedBy { get; set; }
    
    [Required]
    public Guid TenantId { get; set; }
}
