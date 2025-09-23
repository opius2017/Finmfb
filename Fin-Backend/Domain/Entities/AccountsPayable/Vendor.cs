using System.ComponentModel.DataAnnotations;
using FinTech.Domain.Entities.Common;
using FinTech.Domain.Enums;

namespace FinTech.Domain.Entities.AccountsPayable;

public class Vendor : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string VendorNumber { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    public string VendorName { get; set; } = string.Empty;
    
    [Required]
    public VendorType VendorType { get; set; }
    
    [StringLength(20)]
    public string? RCNumber { get; set; }
    
    [StringLength(20)]
    public string? TINNumber { get; set; }
    
    [Required]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;
    
    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }
    
    [StringLength(500)]
    public string? Address { get; set; }
    
    [StringLength(100)]
    public string? City { get; set; }
    
    [StringLength(50)]
    public string? State { get; set; }
    
    [StringLength(100)]
    public string? ContactPersonName { get; set; }
    
    [StringLength(20)]
    public string? ContactPersonPhone { get; set; }
    
    [EmailAddress]
    [StringLength(100)]
    public string? ContactPersonEmail { get; set; }
    
    [StringLength(50)]
    public string? PaymentTerms { get; set; }
    
    [StringLength(100)]
    public string? BankName { get; set; }
    
    [StringLength(20)]
    public string? AccountNumber { get; set; }
    
    [StringLength(100)]
    public string? AccountName { get; set; }
    
    [Required]
    public VendorStatus Status { get; set; } = VendorStatus.Active;
    
    [Required]
    public bool IsWHTApplicable { get; set; } = false;
    
    [Required]
    public bool IsVATRegistered { get; set; } = false;
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    [Required]
    public Guid TenantId { get; set; }
    
    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = [];
    public virtual ICollection<VendorBill> VendorBills { get; set; } = [];
}