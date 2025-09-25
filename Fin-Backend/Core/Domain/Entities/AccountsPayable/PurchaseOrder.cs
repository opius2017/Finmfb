using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Enums;

namespace FinTech.Core.Domain.Entities.AccountsPayable;

public class PurchaseOrder : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string PONumber { get; set; } = string.Empty;
    
    [Required]
    public Guid VendorId { get; set; }
    public virtual Vendor Vendor { get; set; } = null!;
    
    [Required]
    public DateTime OrderDate { get; set; }
    
    public DateTime? ExpectedDeliveryDate { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SubTotal { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal VATAmount { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; } = 0;
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }
    
    [Required]
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [StringLength(500)]
    public string? DeliveryAddress { get; set; }
    
    [StringLength(100)]
    public string? DeliveryContact { get; set; }
    
    [StringLength(20)]
    public string? DeliveryPhone { get; set; }
    
    public string? RequestedBy { get; set; }
    public DateTime? RequestedDate { get; set; }
    
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    
    [StringLength(500)]
    public string? ApprovalNotes { get; set; }
    
    [Required]
    [StringLength(3)]
    public string CurrencyCode { get; set; } = "NGN";
    
    [Required]
    public Guid TenantId { get; set; }
    
    public virtual ICollection<PurchaseOrderItem> Items { get; set; } = [];
}
