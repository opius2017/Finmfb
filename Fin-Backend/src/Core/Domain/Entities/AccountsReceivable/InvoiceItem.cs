using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Domain.Entities.Common;

namespace FinTech.Domain.Entities.AccountsReceivable;

public class InvoiceItem : BaseEntity
{
    [Required]
    public Guid InvoiceId { get; set; }
    public virtual Invoice Invoice { get; set; } = null!;
    
    [Required]
    [StringLength(200)]
    public string ItemDescription { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string? ItemCode { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,4)")]
    public decimal Quantity { get; set; }
    
    [Required]
    [StringLength(20)]
    public string UnitOfMeasure { get; set; } = string.Empty;
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; } = 0;
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal LineTotal { get; set; }
    
    [Required]
    public Guid GLAccountId { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    [Required]
    public Guid TenantId { get; set; }
}