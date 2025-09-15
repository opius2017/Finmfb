using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Domain.Entities.Common;
using FinTech.Domain.Enums;

namespace FinTech.Domain.Entities.Inventory;

public class InventoryItem : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string ItemCode { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    public string ItemName { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public ItemType ItemType { get; set; }
    
    [Required]
    public ItemCategory Category { get; set; }
    
    [StringLength(50)]
    public string? Brand { get; set; }
    
    [StringLength(50)]
    public string? Model { get; set; }
    
    [StringLength(50)]
    public string? SKU { get; set; }
    
    [StringLength(50)]
    public string? Barcode { get; set; }
    
    [Required]
    [StringLength(20)]
    public string UnitOfMeasure { get; set; } = string.Empty;
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitCost { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SellingPrice { get; set; }
    
    [Column(TypeName = "decimal(18,4)")]
    public decimal ReorderLevel { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,4)")]
    public decimal MaximumLevel { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,4)")]
    public decimal CurrentStock { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,4)")]
    public decimal ReservedStock { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,4)")]
    public decimal AvailableStock { get; set; } = 0;
    
    [Required]
    public ValuationMethod ValuationMethod { get; set; } = ValuationMethod.WeightedAverage;
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    [Required]
    public bool TrackStock { get; set; } = true;
    
    [StringLength(100)]
    public string? Supplier { get; set; }
    
    [StringLength(50)]
    public string? Location { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    [Required]
    public Guid InventoryGLAccountId { get; set; }
    
    [Required]
    public Guid COGSGLAccountId { get; set; }
    
    [Required]
    public Guid TenantId { get; set; }
    
    public virtual ICollection<InventoryTransaction> Transactions { get; set; } = [];
    public virtual ICollection<StockAdjustment> StockAdjustments { get; set; } = [];
}