using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Entities.Customers;
using FinTech.Core.Domain.Enums;

namespace FinTech.Core.Domain.Entities.Deposits;

public class DepositAccount : BaseEntity
{
    [Required]
    [StringLength(20)]
    public string AccountNumber { get; set; } = string.Empty;
    
    [Required]
    public string CustomerId { get; set; } = string.Empty;
    public virtual Customer Customer { get; set; } = null!;
    
    [Required]
    public string ProductId { get; set; } = string.Empty;
    public virtual DepositProduct Product { get; set; } = null!;
    // public virtual DepositProduct? DepositProduct { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Balance { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal CurrentBalance { get; set; } = 0;
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal AvailableBalance { get; set; } = 0;
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal LienAmount { get; set; } = 0;
    
    [Required]
    public DateTime DateOpened { get; set; }
    
    public DateTime? DateClosed { get; set; }
    
    [Required]
    public AccountStatus Status { get; set; } = AccountStatus.Active;
    
    public DateTime? LastTransactionDate { get; set; }
    
    public DateTime? LastInterestPostDate { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal AccruedInterest { get; set; } = 0;
    
    public DateTime? MaturityDate { get; set; }
    
    [Required]
    public bool IsJointAccount { get; set; } = false;
    
    [StringLength(500)]
    public string? AccountNotes { get; set; }
    
    [Required]
    [StringLength(3)]
    public string CurrencyCode { get; set; } = "NGN";
    
    [Required]
    public string TenantId { get; set; } = string.Empty;
    
    public virtual ICollection<DepositTransaction> Transactions { get; set; } = [];
}
