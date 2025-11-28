using System;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Enums.FixedAssets;

namespace FinTech.Core.Domain.Entities.FixedAssets;

/// <summary>
/// Represents an item in a physical inventory count
/// </summary>
public class AssetInventoryCountItem : BaseEntity
{
    /// <summary>
    /// Reference to the inventory count
    /// </summary>
    public Guid InventoryCountId { get; set; }
    
    /// <summary>
    /// Navigation property for the inventory count
    /// </summary>
    public virtual AssetInventoryCount? InventoryCount { get; set; }
    
    /// <summary>
    /// Reference to the asset
    /// </summary>
    public Guid AssetId { get; set; }
    
    /// <summary>
    /// Navigation property for the asset
    /// </summary>
    public virtual Asset? Asset { get; set; }
    
    /// <summary>
    /// Expected location of the asset
    /// </summary>
    public string ExpectedLocation { get; set; } = string.Empty;
    
    /// <summary>
    /// Actual location where the asset was found
    /// </summary>
    public string ActualLocation { get; set; } = string.Empty;
    
    /// <summary>
    /// Expected condition of the asset
    /// </summary>
    public AssetCondition ExpectedCondition { get; set; }
    
    /// <summary>
    /// Actual condition of the asset
    /// </summary>
    public AssetCondition ActualCondition { get; set; }
    
    /// <summary>
    /// Status of the count for this asset
    /// </summary>
    public CountItemStatus Status { get; set; }
    
    /// <summary>
    /// Whether the asset was found during the count
    /// </summary>
    public bool WasFound { get; set; }
    
    /// <summary>
    /// Date and time when the asset was counted
    /// </summary>
    public DateTime? CountDateTime { get; set; }
    
    /// <summary>
    /// Any discrepancies noted during the count
    /// </summary>
    public string Discrepancies { get; set; } = string.Empty;
    
    /// <summary>
    /// Actions taken or to be taken to resolve discrepancies
    /// </summary>
    public string ActionTaken { get; set; } = string.Empty;
    
    /// <summary>
    /// Any additional notes about this count item
    /// </summary>
    public string Notes { get; set; } = string.Empty;
}
