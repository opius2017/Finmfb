using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Domain.Entities.Common;
using FinTech.Domain.Enums;

namespace FinTech.Domain.Entities.Reporting;

public class FinancialStatement : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string StatementName { get; set; } = string.Empty;
    
    [Required]
    public FinancialStatementType StatementType { get; set; }
    
    [Required]
    public DateTime PeriodStart { get; set; }
    
    [Required]
    public DateTime PeriodEnd { get; set; }
    
    [Required]
    public DateTime GeneratedDate { get; set; }
    
    [Required]
    public string StatementData { get; set; } = string.Empty; // JSON data
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAssets { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalLiabilities { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalEquity { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalRevenue { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalExpenses { get; set; } = 0;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal NetIncome { get; set; } = 0;
    
    [Required]
    public FinancialStatementStatus Status { get; set; } = FinancialStatementStatus.Draft;
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    public string? PreparedBy { get; set; }
    public DateTime? PreparedDate { get; set; }
    
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedDate { get; set; }
    
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    
    [Required]
    public Guid TenantId { get; set; }
}