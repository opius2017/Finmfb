namespace FinTech.Core.Domain.Enums.FixedAssets;

/// <summary>
/// Represents the current status of an asset
/// </summary>
public enum AssetStatus
{
    /// <summary>
    /// Asset is active (generic status)
    /// </summary>
    Active = 0,

    /// <summary>
    /// Asset is in active use
    /// </summary>
    InUse = 1,
    
    /// <summary>
    /// Asset is available but not currently in use
    /// </summary>
    Available = 2,
    
    /// <summary>
    /// Asset is under maintenance or repair
    /// </summary>
    UnderMaintenance = 3,
    
    /// <summary>
    /// Asset has been damaged
    /// </summary>
    Damaged = 4,
    
    /// <summary>
    /// Asset is obsolete but still owned
    /// </summary>
    Obsolete = 5,
    
    /// <summary>
    /// Asset is currently being retired/disposed
    /// </summary>
    InRetirement = 6,
    
    /// <summary>
    /// Asset has been disposed of
    /// </summary>
    Disposed = 7,
    
    /// <summary>
    /// Asset is missing or cannot be located
    /// </summary>
    Missing = 8,
    
    /// <summary>
    /// Asset has been stolen
    /// </summary>
    Stolen = 9,
    
    /// <summary>
    /// Asset has been transferred to another location/department
    /// </summary>
    Transferred = 10
}
