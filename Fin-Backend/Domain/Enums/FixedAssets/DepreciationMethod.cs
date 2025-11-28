namespace FinTech.Core.Domain.Enums.FixedAssets;

/// <summary>
/// Represents the different methods of depreciation for fixed assets
/// </summary>
public enum DepreciationMethod
{
    /// <summary>
    /// Straight Line depreciation (equal amounts over the useful life)
    /// </summary>
    StraightLine = 1,
    
    /// <summary>
    /// Declining Balance depreciation (accelerated depreciation based on a percentage of remaining book value)
    /// </summary>
    DecliningBalance = 2,
    
    /// <summary>
    /// Double Declining Balance (accelerated method using twice the straight-line rate)
    /// </summary>
    DoubleDecliningBalance = 3,
    
    /// <summary>
    /// Sum of Years' Digits (accelerated method based on fraction of years remaining)
    /// </summary>
    SumOfYearsDigits = 4,
    
    /// <summary>
    /// Units of Production (based on actual usage/production)
    /// </summary>
    UnitsOfProduction = 5,
    
    /// <summary>
    /// No depreciation (for non-depreciable assets)
    /// </summary>
    None = 6
}
