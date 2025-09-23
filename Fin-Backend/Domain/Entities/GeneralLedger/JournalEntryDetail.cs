using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Domain.Entities.Common;
using FinTech.Domain.Enums;

namespace FinTech.Domain.Entities.GeneralLedger;

public class JournalEntryDetail : BaseEntity
{
    [Required]
    public Guid JournalEntryId { get; set; }
    public virtual JournalEntry JournalEntry { get; set; } = null!;
    
    [Required]
    public Guid AccountId { get; set; }
    public virtual ChartOfAccounts Account { get; set; } = null!;
    
    [Required]
    public EntryType EntryType { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    
    [StringLength(200)]
    public string? Description { get; set; }
    
    [StringLength(100)]
    public string? Reference { get; set; }
    
    [Required]
    public Guid TenantId { get; set; }
}