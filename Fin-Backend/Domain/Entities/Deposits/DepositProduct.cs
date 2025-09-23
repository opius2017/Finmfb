using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Domain.Entities.Common;
using FinTech.Domain.Enums;

namespace FinTech.Domain.Entities.Deposits;

public class DepositProduct : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string ProductName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(20)]
    public string ProductCode { get; set; } = string.Empty;
    
    [Required]
    public DepositProductType ProductType { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(5,2)")]
    public decimal InterestRate { get; set; }
    
    [Required]
    public InterestCalculationMethod InterestCalculationMethod { get; set; }
    
    [Required]
    public InterestPostingFrequency InterestPostingFrequency { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal MinimumBalance { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal MaximumBalance { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal MinimumOpeningBalance { get; set; } = 0;
    
    public int? TenorInDays { get; set; }
    
    public int? MinimumAge { get; set; } = 18;
    
    public int? MaximumAge { get; set; }
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    [Required]
    public bool AllowPartialWithdrawal { get; set; } = true;
    
    [Required]
    public int WithdrawalLimitPerDay { get; set; } = 0; // 0 = unlimited
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal WithdrawalFee { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal MaintenanceFee { get; set; } = 0;
    
    [Required]
    [StringLength(3)]
    public string CurrencyCode { get; set; } = "NGN";
    
    [Required]
    public Guid GLAccountId { get; set; } // Links to Chart of Accounts
    
    [Required]
    public Guid TenantId { get; set; }
    
    public virtual ICollection<DepositAccount> DepositAccounts { get; set; } = [];
}