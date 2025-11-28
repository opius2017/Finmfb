namespace FinTech.Core.Domain.Enums.FixedAssets;

/// <summary>
/// Represents the type of maintenance performed on an asset
/// </summary>
public enum MaintenanceType
{
    /// <summary>
    /// Regular preventive maintenance to prevent breakdown
    /// </summary>
    Preventive = 1,
    
    /// <summary>
    /// Corrective maintenance to fix a failure or breakdown
    /// </summary>
    Corrective = 2,
    
    /// <summary>
    /// Predictive maintenance based on condition monitoring
    /// </summary>
    Predictive = 3,
    
    /// <summary>
    /// Condition-based maintenance based on asset condition
    /// </summary>
    ConditionBased = 4,
    
    /// <summary>
    /// Emergency maintenance for critical failures
    /// </summary>
    Emergency = 5,
    
    /// <summary>
    /// Planned shutdown maintenance during scheduled downtime
    /// </summary>
    Shutdown = 6,
    
    /// <summary>
    /// Upgrades or improvements to the asset
    /// </summary>
    Upgrade = 7,
    
    /// <summary>
    /// Regulatory or compliance-related maintenance
    /// </summary>
    Regulatory = 8,
    
    /// <summary>
    /// Safety-related maintenance
    /// </summary>
    Safety = 9,
    
    /// <summary>
    /// Calibration of measuring equipment
    /// </summary>
    Calibration = 10,
    
    /// <summary>
    /// Other types of maintenance
    /// </summary>
    Other = 11
}
