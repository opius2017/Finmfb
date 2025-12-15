using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Enums;

namespace FinTech.Core.Domain.Entities.Inventory;

public class InventoryTransaction : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string TransactionNumber { get; set; } = string.Empty;
    
    [Required]
    public Guid InventoryItemId { get; set; }
    public virtual InventoryItem? InventoryItem { get; set; }
    
    [Required]
    public InventoryTransactionType TransactionType { get; set; }
    
    [Required]
    public DateTime TransactionDate { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,4)")]
    public decimal Quantity { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitCost { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalCost { get; set; }
    
    [Column(TypeName = "decimal(18,4)")]
    public decimal RunningBalance { get; set; } = 0;
    
    [StringLength(100)]
    public string? Reference { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [StringLength(100)]
    public string? Location { get; set; }
    
    public Guid? RelatedDocumentId { get; set; }
    
    [StringLength(50)]
    public string? RelatedDocumentType { get; set; }
    
    [Required]
    public TransactionStatus Status { get; set; } = TransactionStatus.Posted;
    
    [Required]
    public Guid TenantId { get; set; }
}
