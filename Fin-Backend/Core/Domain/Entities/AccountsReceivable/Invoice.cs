using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Entities.Customers;
using FinTech.Core.Domain.Enums;

namespace FinTech.Core.Domain.Entities.AccountsReceivable;

public class Invoice : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string InvoiceNumber { get; set; } = string.Empty;
    
    [Required]
    public Guid CustomerId { get; set; }
    public virtual Customer? Customer { get; set; }
    
    [Required]
    public DateTime InvoiceDate { get; set; }
    
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
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    [StringLength(500)]
    public string? Terms { get; set; }
    
    public DateTime? SentDate { get; set; }
    
    public DateTime? LastReminderDate { get; set; }
    
    public int ReminderCount { get; set; } = 0;
    
    [Required]
    [StringLength(3)]
    public string CurrencyCode { get; set; } = "NGN";
    
    [Required]
    public Guid TenantId { get; set; }
    
    public virtual ICollection<InvoiceItem> Items { get; set; } = [];
    public virtual ICollection<CustomerPayment> Payments { get; set; } = [];
}
