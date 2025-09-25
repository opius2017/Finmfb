namespace FinTech.Core.Domain.Enums.FixedAssets;

/// <summary>
/// Represents the status of an item in an inventory count
/// </summary>
public enum CountItemStatus
{
    /// <summary>
    /// Item is pending count
    /// </summary>
    Pending = 1,
    
    /// <summary>
    /// Item has been counted
    /// </summary>
    Counted = 2,
    
    /// <summary>
    /// Item has discrepancies
    /// </summary>
    Discrepancy = 3,
    
    /// <summary>
    /// Item was not found during count
    /// </summary>
    NotFound = 4,
    
    /// <summary>
    /// Discrepancy has been resolved
    /// </summary>
    Resolved = 5,
    
    /// <summary>
    /// Item was skipped during count
    /// </summary>
    Skipped = 6,
    
    /// <summary>
    /// Item requires investigation
    /// </summary>
    InvestigationRequired = 7
}
