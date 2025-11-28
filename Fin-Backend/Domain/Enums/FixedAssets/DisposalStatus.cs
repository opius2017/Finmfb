namespace FinTech.Core.Domain.Enums.FixedAssets;

/// <summary>
/// Represents the status of an asset disposal
/// </summary>
public enum DisposalStatus
{
    /// <summary>
    /// Disposal has been proposed
    /// </summary>
    Proposed = 1,
    
    /// <summary>
    /// Disposal is pending approval
    /// </summary>
    PendingApproval = 2,
    
    /// <summary>
    /// Disposal has been approved
    /// </summary>
    Approved = 3,
    
    /// <summary>
    /// Disposal is in progress
    /// </summary>
    InProgress = 4,
    
    /// <summary>
    /// Disposal has been completed
    /// </summary>
    Completed = 5,
    
    /// <summary>
    /// Disposal has been rejected
    /// </summary>
    Rejected = 6,
    
    /// <summary>
    /// Disposal has been cancelled
    /// </summary>
    Cancelled = 7,
    
    /// <summary>
    /// Disposal is on hold
    /// </summary>
    OnHold = 8
}
