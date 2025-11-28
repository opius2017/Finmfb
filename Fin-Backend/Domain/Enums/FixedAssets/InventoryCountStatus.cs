namespace FinTech.Core.Domain.Enums.FixedAssets;

/// <summary>
/// Represents the status of an inventory count
/// </summary>
public enum InventoryCountStatus
{
    /// <summary>
    /// Inventory count is planned
    /// </summary>
    Planned = 1,
    
    /// <summary>
    /// Inventory count is in progress
    /// </summary>
    InProgress = 2,
    
    /// <summary>
    /// Inventory count is under review
    /// </summary>
    UnderReview = 3,
    
    /// <summary>
    /// Inventory count has been completed
    /// </summary>
    Completed = 4,
    
    /// <summary>
    /// Inventory count has been approved
    /// </summary>
    Approved = 5,
    
    /// <summary>
    /// Inventory count has been cancelled
    /// </summary>
    Cancelled = 6,
    
    /// <summary>
    /// Inventory count is on hold
    /// </summary>
    OnHold = 7
}
