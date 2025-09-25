using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Entities.Customers;
using FinTech.Core.Domain.Enums;

namespace FinTech.Core.Domain.Entities.AccountsReceivable;

public class CustomerPayment : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string PaymentNumber { get; set; } = string.Empty;
    
    [Required]
    public Guid CustomerId { get; set; }
    public virtual Customer Customer { get; set; } = null!;
    
    [Required]
    public Guid InvoiceId { get; set; }
    public virtual Invoice Invoice { get; set; } = null!;
    
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
    
    [Required]
    [StringLength(3)]
    public string CurrencyCode { get; set; } = "NGN";
    
    [Required]
    public Guid TenantId { get; set; }
}
