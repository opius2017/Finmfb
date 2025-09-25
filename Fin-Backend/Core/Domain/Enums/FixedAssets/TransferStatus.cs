namespace FinTech.Core.Domain.Enums.FixedAssets;

/// <summary>
/// Represents the status of an asset transfer
/// </summary>
public enum TransferStatus
{
    /// <summary>
    /// Transfer has been requested
    /// </summary>
    Requested = 1,
    
    /// <summary>
    /// Transfer request is pending approval
    /// </summary>
    PendingApproval = 2,
    
    /// <summary>
    /// Transfer has been approved
    /// </summary>
    Approved = 3,
    
    /// <summary>
    /// Transfer is in progress
    /// </summary>
    InProgress = 4,
    
    /// <summary>
    /// Transfer has been completed
    /// </summary>
    Completed = 5,
    
    /// <summary>
    /// Transfer has been rejected
    /// </summary>
    Rejected = 6,
    
    /// <summary>
    /// Transfer has been cancelled
    /// </summary>
    Cancelled = 7,
    
    /// <summary>
    /// Transfer has been delayed
    /// </summary>
    Delayed = 8
}
