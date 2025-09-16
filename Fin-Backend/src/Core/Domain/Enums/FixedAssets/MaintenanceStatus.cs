namespace FinTech.Domain.Enums.FixedAssets;

/// <summary>
/// Represents the status of a maintenance record
/// </summary>
public enum MaintenanceStatus
{
    /// <summary>
    /// Maintenance is scheduled for the future
    /// </summary>
    Scheduled = 1,
    
    /// <summary>
    /// Maintenance is in progress
    /// </summary>
    InProgress = 2,
    
    /// <summary>
    /// Maintenance has been completed
    /// </summary>
    Completed = 3,
    
    /// <summary>
    /// Maintenance has been postponed
    /// </summary>
    Postponed = 4,
    
    /// <summary>
    /// Maintenance has been cancelled
    /// </summary>
    Cancelled = 5,
    
    /// <summary>
    /// Maintenance was completed but requires follow-up
    /// </summary>
    RequiresFollowup = 6,
    
    /// <summary>
    /// Maintenance is waiting for parts
    /// </summary>
    WaitingForParts = 7,
    
    /// <summary>
    /// Maintenance is on hold
    /// </summary>
    OnHold = 8
}