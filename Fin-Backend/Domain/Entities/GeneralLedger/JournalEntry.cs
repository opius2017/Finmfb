using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Enums;

namespace FinTech.Core.Domain.Entities.GeneralLedger;

public class JournalEntry : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string JournalNumber { get; set; } = string.Empty;
    
    [Required]
    public DateTime TransactionDate { get; set; }
    
    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalDebit { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCredit { get; set; }
    
    [Required]
    public JournalStatus Status { get; set; } = JournalStatus.Draft;
    
    public string? PreparedBy { get; set; }
    public DateTime? PreparedDate { get; set; }
    
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedDate { get; set; }
    
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    
    public string? PostedBy { get; set; }
    public DateTime? PostedDate { get; set; }
    
    public string? RejectionReason { get; set; }
    
    public virtual ICollection<JournalEntryDetail> Details { get; set; } = [];
    
    [Required]
    public Guid TenantId { get; set; }
}
