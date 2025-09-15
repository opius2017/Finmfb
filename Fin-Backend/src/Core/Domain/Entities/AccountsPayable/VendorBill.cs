using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Domain.Entities.Common;
using FinTech.Domain.Enums;

namespace FinTech.Domain.Entities.AccountsPayable;

public class VendorBill : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string BillNumber { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string? VendorInvoiceNumber { get; set; }
    
    [Required]
    public Guid VendorId { get; set; }
    public virtual Vendor Vendor { get; set; } = null!;
    
    public Guid? PurchaseOrderId { get; set; }
    public virtual PurchaseOrder? PurchaseOrder { get; set; }
    
    [Required]
    public DateTime BillDate { get; set; }
    
    [Required]
    public DateTime DueDate { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SubTotal { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal VATAmount { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal WHTAmount { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; } = 0;
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal PaidAmount { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal OutstandingAmount { get; set; }
    
    [Required]
    public BillStatus Status { get; set; } = BillStatus.Draft;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    
    [Required]
    [StringLength(3)]
    public string CurrencyCode { get; set; } = "NGN";
    
    [Required]
    public Guid TenantId { get; set; }
    
    public virtual ICollection<VendorBillItem> Items { get; set; } = [];
    public virtual ICollection<VendorPayment> Payments { get; set; } = [];
}