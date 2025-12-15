using System;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Enums.FixedAssets;

namespace FinTech.Core.Domain.Entities.FixedAssets;

/// <summary>
/// Represents a maintenance record for an asset
/// </summary>
public class AssetMaintenance : BaseEntity
{
    /// <summary>
    /// Reference to the asset
    /// </summary>
    /// <summary>
    /// Reference to the asset
    /// </summary>
    public string AssetId { get; set; } = string.Empty;
    
    /// <summary>
    /// Navigation property for the asset
    /// </summary>
    public virtual Asset? Asset { get; set; }
    
    /// <summary>
    /// Maintenance reference number
    /// </summary>
    public string MaintenanceNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of maintenance performed
    /// </summary>
    public MaintenanceType MaintenanceType { get; set; }
    
    /// <summary>
    /// Title/brief description of the maintenance activity
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Detailed description of the maintenance performed
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Date when the maintenance was performed
    /// </summary>
    public DateTime MaintenanceDate { get; set; }
    
    /// <summary>
    /// Cost of the maintenance
    /// </summary>
    public decimal Cost { get; set; }
    
    /// <summary>
    /// Vendor who performed the maintenance
    /// </summary>
    public string? VendorId { get; set; }
    
    /// <summary>
    /// Invoice reference for the maintenance service
    /// </summary>
    public string InvoiceReference { get; set; } = string.Empty;
    
    /// <summary>
    /// Next scheduled maintenance date
    /// </summary>
    public DateTime? NextMaintenanceDate { get; set; }
    
    /// <summary>
    /// Status of the maintenance record
    /// </summary>
    public MaintenanceStatus Status { get; set; }
    
    /// <summary>
    /// Performed by (employee or contractor name)
    /// </summary>
    public string PerformedBy { get; set; } = string.Empty;
    
    /// <summary>
    /// Employee who authorized the maintenance
    /// </summary>
    public string? AuthorizedById { get; set; }

    /// <summary>
    /// Date maintenance was completed
    /// </summary>
    public DateTime? CompletionDate { get; set; }

    /// <summary>
    /// User who marked maintenance as completed
    /// </summary>
    public string? CompletedBy { get; set; }
    
    /// <summary>
    /// Any additional notes about the maintenance
    /// </summary>
    public string Notes { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether this maintenance extended the useful life of the asset
    /// </summary>
    public bool ExtendedAssetLife { get; set; }
    
    /// <summary>
    /// Number of months by which the asset's life was extended
    /// </summary>
    public int? ExtendedLifeMonths { get; set; }
    
    /// <summary>
    /// Whether this maintenance increased the value of the asset
    /// </summary>
    public bool IncreasedAssetValue { get; set; }
    
    /// <summary>
    /// Amount by which the asset's value was increased
    /// </summary>
    public decimal? ValueIncreaseAmount { get; set; }
}
