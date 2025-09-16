namespace FinTech.Domain.Enums.FixedAssets;

/// <summary>
/// Represents the method used for disposing an asset
/// </summary>
public enum DisposalMethod
{
    /// <summary>
    /// Asset was sold for cash
    /// </summary>
    Sale = 1,
    
    /// <summary>
    /// Asset was scrapped or demolished
    /// </summary>
    Scrapped = 2,
    
    /// <summary>
    /// Asset was donated
    /// </summary>
    Donation = 3,
    
    /// <summary>
    /// Asset was traded in for another asset
    /// </summary>
    TradeIn = 4,
    
    /// <summary>
    /// Asset was stolen or lost
    /// </summary>
    StolenOrLost = 5,
    
    /// <summary>
    /// Asset was destroyed by accident or disaster
    /// </summary>
    Destroyed = 6,
    
    /// <summary>
    /// Asset was transferred to another entity
    /// </summary>
    Transfer = 7,
    
    /// <summary>
    /// Asset was returned to lessor
    /// </summary>
    ReturnToLessor = 8,
    
    /// <summary>
    /// Other disposal method
    /// </summary>
    Other = 9
}