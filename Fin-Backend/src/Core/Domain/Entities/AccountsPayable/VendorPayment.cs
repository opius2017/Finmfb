using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Domain.Entities.Common;
using FinTech.Domain.Enums;

namespace FinTech.Domain.Entities.AccountsPayable;

public class VendorPayment : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string PaymentNumber { get; set; } = string.Empty;
    
    [Required]
    public Guid VendorId { get; set; }
    public virtual Vendor Vendor { get; set; } = null!;
    
    [Required]
    public Guid VendorBillId { get; set; }
    public virtual VendorBill VendorBill { get; set; } = null!;
    
    [Required]
    public DateTime PaymentDate { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PaymentAmount { get; set; }
    
    [Required]
    public PaymentMethod PaymentMethod { get; set; }
    
    [StringLength(100)]
    public string? PaymentReference { get; set; }
    
    [StringLength(100)]
    public string? ChequeNumber { get; set; }
    
    [StringLength(100)]
    public string? BankName { get; set; }
    
    [Required]
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    public string? ProcessedBy { get; set; }
    public DateTime? ProcessedDate { get; set; }
    
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    
    [Required]
    [StringLength(3)]
    public string CurrencyCode { get; set; } = "NGN";
    
    [Required]
    public Guid TenantId { get; set; }
}