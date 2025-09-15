using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Domain.Entities.Common;
using FinTech.Domain.Enums;

namespace FinTech.Domain.Entities.Loans;

public class LoanRepaymentSchedule : BaseEntity
{
    [Required]
    public Guid LoanAccountId { get; set; }
    public virtual LoanAccount LoanAccount { get; set; } = null!;
    
    [Required]
    public int InstallmentNumber { get; set; }
    
    [Required]
    public DateTime DueDate { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PrincipalAmount { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal InterestAmount { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal PaidPrincipal { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal PaidInterest { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal PaidTotal { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal OutstandingPrincipal { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal OutstandingInterest { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal OutstandingTotal { get; set; }
    
    [Required]
    public RepaymentStatus Status { get; set; } = RepaymentStatus.Pending;
    
    public DateTime? PaidDate { get; set; }
    
    public int DaysOverdue { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal PenaltyAmount { get; set; } = 0;
    
    [Required]
    public Guid TenantId { get; set; }
}