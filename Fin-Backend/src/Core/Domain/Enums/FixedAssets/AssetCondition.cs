namespace FinTech.Domain.Enums.FixedAssets;

/// <summary>
/// Represents the condition of an asset
/// </summary>
public enum AssetCondition
{
    /// <summary>
    /// Asset is in excellent condition (like new)
    /// </summary>
    Excellent = 1,
    
    /// <summary>
    /// Asset is in good condition (minor wear)
    /// </summary>
    Good = 2,
    
    /// <summary>
    /// Asset is in fair condition (moderate wear but functional)
    /// </summary>
    Fair = 3,
    
    /// <summary>
    /// Asset is in poor condition (significant wear, limited functionality)
    /// </summary>
    Poor = 4,
    
    /// <summary>
    /// Asset is non-functional and needs repair
    /// </summary>
    NonFunctional = 5,
    
    /// <summary>
    /// Asset is beyond repair
    /// </summary>
    BeyondRepair = 6,
    
    /// <summary>
    /// Asset condition is unknown
    /// </summary>
    Unknown = 7
}