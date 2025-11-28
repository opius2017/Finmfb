using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Enums;

namespace FinTech.Core.Domain.Entities.MultiCurrency;

public class CurrencyRevaluation : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string RevaluationNumber { get; set; } = string.Empty;
    
    [Required]
    public DateTime RevaluationDate { get; set; }
    
    [Required]
    [StringLength(3)]
    public string CurrencyCode { get; set; } = string.Empty;
    
    [Required]
    [Column(TypeName = "decimal(18,6)")]
    public decimal OldRate { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,6)")]
    public decimal NewRate { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal ForeignCurrencyAmount { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal OldBaseCurrencyAmount { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal NewBaseCurrencyAmount { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal RevaluationGainLoss { get; set; }
    
    [Required]
    public RevaluationType RevaluationType { get; set; }
    
    [Required]
    public RevaluationStatus Status { get; set; } = RevaluationStatus.Pending;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public string? ProcessedBy { get; set; }
    public DateTime? ProcessedDate { get; set; }
    
    [Required]
    public Guid TenantId { get; set; }
}
