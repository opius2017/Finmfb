using System;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.FixedAssets;

/// <summary>
/// Represents a depreciation schedule entry for an asset
/// </summary>
public class AssetDepreciationSchedule : BaseEntity
{
    /// <summary>
    /// Reference to the asset
    /// </summary>
    public Guid AssetId { get; set; }
    
    /// <summary>
    /// Navigation property for the asset
    /// </summary>
    public virtual Asset? Asset { get; set; }
    
    /// <summary>
    /// Period number in the depreciation schedule
    /// </summary>
    public int PeriodNumber { get; set; }
    
    /// <summary>
    /// Period start date
    /// </summary>
    public DateTime PeriodStartDate { get; set; }
    
    /// <summary>
    /// Period end date
    /// </summary>
    public DateTime PeriodEndDate { get; set; }
    
    /// <summary>
    /// Opening book value at the start of the period
    /// </summary>
    public decimal OpeningBookValue { get; set; }
    
    /// <summary>
    /// Depreciation amount for this period
    /// </summary>
    public decimal DepreciationAmount { get; set; }
    
    /// <summary>
    /// Accumulated depreciation up to and including this period
    /// </summary>
    public decimal AccumulatedDepreciation { get; set; }
    
    /// <summary>
    /// Closing book value at the end of the period
    /// </summary>
    public decimal ClosingBookValue { get; set; }
    
    /// <summary>
    /// Whether this depreciation has been posted to the general ledger
    /// </summary>
    public bool IsPosted { get; set; }
    
    /// <summary>
    /// Date when the depreciation was posted to GL
    /// </summary>
    public DateTime? PostedDate { get; set; }
    
    /// <summary>
    /// Reference to the journal entry where this depreciation was posted
    /// </summary>
    public string? JournalEntryReference { get; set; }
    
    /// <summary>
    /// Any notes related to this depreciation period
    /// </summary>
    public string Notes { get; set; } = string.Empty;
}
