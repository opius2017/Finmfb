using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Enums;

namespace FinTech.Core.Domain.Entities.Reporting;

public class RegulatoryReport : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string ReportName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string ReportCode { get; set; } = string.Empty;
    
    [Required]
    public RegulatoryAuthority Authority { get; set; }
    
    [Required]
    public ReportingFrequency Frequency { get; set; }
    
    [Required]
    public DateTime ReportingPeriodStart { get; set; }
    
    [Required]
    public DateTime ReportingPeriodEnd { get; set; }
    
    [Required]
    public DateTime DueDate { get; set; }
    
    [Required]
    public DateTime GeneratedDate { get; set; }
    
    [Required]
    public string ReportData { get; set; } = string.Empty; // JSON or XML data
    
    [Required]
    public RegulatoryReportStatus Status { get; set; } = RegulatoryReportStatus.Draft;
    
    public DateTime? SubmittedDate { get; set; }
    
    [StringLength(100)]
    public string? SubmissionReference { get; set; }
    
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
