using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Domain.Entities.Common;
using FinTech.Domain.Enums;

namespace FinTech.Domain.Entities.Inventory;

public class StockAdjustment : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string AdjustmentNumber { get; set; } = string.Empty;
    
    [Required]
    public Guid InventoryItemId { get; set; }
    public virtual InventoryItem InventoryItem { get; set; } = null!;
    
    [Required]
    public DateTime AdjustmentDate { get; set; }
    
    [Required]
    public StockAdjustmentType AdjustmentType { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,4)")]
    public decimal SystemQuantity { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,4)")]
    public decimal ActualQuantity { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,4)")]
    public decimal AdjustmentQuantity { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitCost { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal AdjustmentValue { get; set; }
    
    [Required]
    [StringLength(500)]
    public string Reason { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    
    [Required]
    public AdjustmentStatus Status { get; set; } = AdjustmentStatus.Pending;
    
    [Required]
    public Guid TenantId { get; set; }
}