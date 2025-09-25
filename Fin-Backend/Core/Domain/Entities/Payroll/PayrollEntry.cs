using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Enums;

namespace FinTech.Core.Domain.Entities.Payroll;

public class PayrollEntry : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string PayrollNumber { get; set; } = string.Empty;
    
    [Required]
    public Guid EmployeeId { get; set; }
    public virtual Employee Employee { get; set; } = null!;
    
    [Required]
    public DateTime PayrollDate { get; set; }
    
    [Required]
    public DateTime PayPeriodStart { get; set; }
    
    [Required]
    public DateTime PayPeriodEnd { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal BasicSalary { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal HousingAllowance { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal TransportAllowance { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal MedicalAllowance { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal OtherAllowances { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal OvertimeAmount { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal BonusAmount { get; set; } = 0;
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal GrossEarnings { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal PAYEDeduction { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal PensionDeduction { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal NHFDeduction { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal CooperativeDeduction { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal LoanDeduction { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal OtherDeductions { get; set; } = 0;
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalDeductions { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal NetPay { get; set; }
    
    [Required]
    public PayrollStatus Status { get; set; } = PayrollStatus.Draft;
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    public string? ProcessedBy { get; set; }
    public DateTime? ProcessedDate { get; set; }
    
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    
    public DateTime? PaidDate { get; set; }
    
    [Required]
    public Guid TenantId { get; set; }
}
