using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Entities.Customers;
using FinTech.Core.Domain.Enums;

namespace FinTech.Core.Domain.Entities.Loans;

public class LoanAccount : BaseEntity
{
    [Required]
    [StringLength(20)]
    public string? AccountNumber { get; set; }
    
    [Required]
    public Guid CustomerId { get; set; }
    public virtual Customer Customer { get; set; } = null!;
    
    [Required]
    public Guid ProductId { get; set; }
    public virtual LoanProduct Product { get; set; } = null!;
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PrincipalAmount { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal OutstandingPrincipal { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal OutstandingInterest { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal OutstandingFees { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(5,2)")]
    public decimal InterestRate { get; set; }
    
    [Required]
    public int TenorDays { get; set; }
    
    [Required]
    public DateTime DisbursementDate { get; set; }
    
    [Required]
    public DateTime MaturityDate { get; set; }
    
    public DateTime? FirstRepaymentDate { get; set; }
    
    public DateTime? LastRepaymentDate { get; set; }
    
    [Required]
    public LoanStatus Status { get; set; } = LoanStatus.Applied;
    
    [Required]
    public LoanClassification Classification { get; set; } = LoanClassification.Performing;
    
    public int DaysPastDue { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal ProvisionAmount { get; set; } = 0;
    
    [Column(TypeName = "decimal(5,2)")]
    public decimal ProvisionRate { get; set; } = 0;
    
    [StringLength(500)]
    public string? Purpose { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    
    public string? DisbursedBy { get; set; }
    public DateTime? DisbursedDate { get; set; }
    
    [Required]
    [StringLength(3)]
    public string? CurrencyCode { get; set; } = "NGN";
    
    [Required]
    public Guid TenantId { get; set; }
    
    public virtual ICollection<LoanTransaction> Transactions { get; set; } = [];
    public virtual ICollection<LoanRepaymentSchedule> RepaymentSchedule { get; set; } = [];
    public virtual ICollection<LoanCollateral> Collaterals { get; set; } = [];
    public virtual ICollection<LoanGuarantor> Guarantors { get; set; } = [];
}
