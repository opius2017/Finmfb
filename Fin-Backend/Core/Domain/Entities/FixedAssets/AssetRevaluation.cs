using System;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.FixedAssets;

/// <summary>
/// Represents a revaluation of an asset
/// </summary>
public class AssetRevaluation : BaseEntity
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
    /// Date of revaluation
    /// </summary>
    public DateTime RevaluationDate { get; set; }
    
    /// <summary>
    /// Book value before revaluation
    /// </summary>
    public decimal PreviousBookValue { get; set; }
    
    /// <summary>
    /// New book value after revaluation
    /// </summary>
    public decimal NewBookValue { get; set; }
    
    /// <summary>
    /// Change in value (can be positive or negative)
    /// </summary>
    public decimal ValueChange { get; set; }
    
    /// <summary>
    /// Reason for the revaluation
    /// </summary>
    public string RevaluationReason { get; set; } = string.Empty;
    
    /// <summary>
    /// Method used for revaluation
    /// </summary>
    public string RevaluationMethod { get; set; } = string.Empty;
    
    /// <summary>
    /// Who performed the revaluation (e.g., internal staff, external appraiser)
    /// </summary>
    public string PerformedBy { get; set; } = string.Empty;
    
    /// <summary>
    /// Authorization reference/document number
    /// </summary>
    public string AuthorizationReference { get; set; } = string.Empty;
    
    /// <summary>
    /// Employee who authorized the revaluation
    /// </summary>
    public Guid? AuthorizedById { get; set; }
    
    /// <summary>
    /// Whether this revaluation has been posted to the general ledger
    /// </summary>
    public bool IsPosted { get; set; }
    
    /// <summary>
    /// Date when the revaluation was posted to GL
    /// </summary>
    public DateTime? PostedDate { get; set; }
    
    /// <summary>
    /// Reference to the journal entry where this revaluation was posted
    /// </summary>
    public string? JournalEntryReference { get; set; }
    
    /// <summary>
    /// Any additional notes about the revaluation
    /// </summary>
    public string Notes { get; set; } = string.Empty;
}
