using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Domain.Entities.Common;
using FinTech.Domain.Enums;

namespace FinTech.Domain.Entities.Security;

public class MakerCheckerTransaction : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string TransactionReference { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string EntityName { get; set; } = string.Empty;
    
    [Required]
    public Guid EntityId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Operation { get; set; } = string.Empty;
    
    [Required]
    public string RequestData { get; set; } = string.Empty;
    
    [Required]
    public Guid MakerId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string MakerName { get; set; } = string.Empty;
    
    [Required]
    public DateTime MakerTimestamp { get; set; }
    
    public Guid? CheckerId { get; set; }
    
    [StringLength(100)]
    public string? CheckerName { get; set; }
    
    public DateTime? CheckerTimestamp { get; set; }
    
    [Required]
    public MakerCheckerStatus Status { get; set; } = MakerCheckerStatus.PendingApproval;
    
    [StringLength(500)]
    public string? CheckerComments { get; set; }
    
    [StringLength(500)]
    public string? RejectionReason { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? Amount { get; set; }
    
    [Required]
    public int Priority { get; set; } = 1; // 1=Low, 2=Medium, 3=High, 4=Critical
    
    public DateTime? ExpiryDate { get; set; }
    
    [Required]
    public Guid TenantId { get; set; }
}