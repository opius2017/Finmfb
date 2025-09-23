using System;
using FinTech.Domain.Entities.Common;
using FinTech.Domain.Enums.FixedAssets;

namespace FinTech.Domain.Entities.FixedAssets;

/// <summary>
/// Represents a physical inventory count of assets for verification
/// </summary>
public class AssetInventoryCount : BaseEntity
{
    /// <summary>
    /// Unique inventory count number
    /// </summary>
    public string InventoryCountNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Description or purpose of the inventory count
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Date when the inventory count was started
    /// </summary>
    public DateTime CountDate { get; set; }
    
    /// <summary>
    /// Status of the inventory count
    /// </summary>
    public InventoryCountStatus Status { get; set; }
    
    /// <summary>
    /// Location where the count was performed
    /// </summary>
    public string Location { get; set; } = string.Empty;
    
    /// <summary>
    /// Department for which the count was performed
    /// </summary>
    public string Department { get; set; } = string.Empty;
    
    /// <summary>
    /// Category of assets being counted
    /// </summary>
    public Guid? AssetCategoryId { get; set; }
    
    /// <summary>
    /// Employee who authorized the inventory count
    /// </summary>
    public Guid? AuthorizedById { get; set; }
    
    /// <summary>
    /// Employee who performed the inventory count
    /// </summary>
    public Guid PerformedById { get; set; }
    
    /// <summary>
    /// Date when the count was completed
    /// </summary>
    public DateTime? CompletionDate { get; set; }
    
    /// <summary>
    /// Any additional notes about the inventory count
    /// </summary>
    public string Notes { get; set; } = string.Empty;
    
    /// <summary>
    /// Navigation property for count items
    /// </summary>
    public virtual ICollection<AssetInventoryCountItem> CountItems { get; set; } = new List<AssetInventoryCountItem>();
}