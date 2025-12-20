using System.ComponentModel.DataAnnotations;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Enums;

namespace FinTech.Core.Domain.Entities.GeneralLedger;

public class ChartOfAccounts : BaseEntity
{
    [Required]
    [StringLength(10)]
    public string AccountCode { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    public string AccountName { get; set; } = string.Empty;
    
    [Required]
    public AccountType AccountType { get; set; }
    
    [Required]
    public AccountCategory AccountCategory { get; set; }
    
    public string? ParentAccountId { get; set; }
    public virtual ChartOfAccounts? ParentAccount { get; set; }
    
    public virtual ICollection<ChartOfAccounts> SubAccounts { get; set; } = [];
    
    [Required]
    public int Level { get; set; }
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    [Required]
    public bool IsSystemAccount { get; set; } = false;
    
    public string? Description { get; set; }
    
    [Required]
    public string CurrencyCode { get; set; } = "NGN";
    
    public virtual ICollection<GeneralLedgerEntry> GeneralLedgerEntries { get; set; } = [];
    
    [Required]
    public string TenantId { get; set; } = string.Empty;
}
