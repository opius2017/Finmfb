using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Domain.Entities.Common;
using FinTech.Domain.Enums;

namespace FinTech.Domain.Entities.MultiCurrency;

public class ExchangeRate : BaseEntity
{
    [Required]
    [StringLength(3)]
    public string FromCurrency { get; set; } = string.Empty;
    
    [Required]
    [StringLength(3)]
    public string ToCurrency { get; set; } = string.Empty;
    
    [Required]
    public DateTime EffectiveDate { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,6)")]
    public decimal BuyRate { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,6)")]
    public decimal SellRate { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,6)")]
    public decimal MidRate { get; set; }
    
    [Required]
    public ExchangeRateSource Source { get; set; }
    
    [StringLength(100)]
    public string? SourceReference { get; set; }
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    [Required]
    public Guid TenantId { get; set; }
}