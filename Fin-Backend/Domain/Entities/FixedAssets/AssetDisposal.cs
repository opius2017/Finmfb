using System;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Enums.FixedAssets;

namespace FinTech.Core.Domain.Entities.FixedAssets;

/// <summary>
/// Represents a disposal of an asset
/// </summary>
public class AssetDisposal : BaseEntity
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
    /// Unique disposal reference number
    /// </summary>
    public string DisposalNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Date of the disposal
    /// </summary>
    public DateTime DisposalDate { get; set; }
    
    /// <summary>
    /// Method used for disposal
    /// </summary>
    public DisposalMethod DisposalMethod { get; set; }
    
    /// <summary>
    /// Book value at the time of disposal
    /// </summary>
    public decimal BookValueAtDisposal { get; set; }
    
    /// <summary>
    /// Accumulated depreciation at the time of disposal
    /// </summary>
    public decimal AccumulatedDepreciationAtDisposal { get; set; }
    
    /// <summary>
    /// Proceeds received from the disposal
    /// </summary>
    public decimal DisposalProceeds { get; set; }
    
    /// <summary>
    /// Gain or loss on disposal (proceeds - book value)
    /// </summary>
    public decimal GainOrLoss { get; set; }
    
    /// <summary>
    /// Reason for the disposal
    /// </summary>
    public string DisposalReason { get; set; } = string.Empty;
    
    /// <summary>
    /// Buyer name (if sold)
    /// </summary>
    public string BuyerName { get; set; } = string.Empty;
    
    /// <summary>
    /// Buyer contact information
    /// </summary>
    public string BuyerContactInfo { get; set; } = string.Empty;
    
    /// <summary>
    /// Employee who authorized the disposal
    /// </summary>
    public Guid? AuthorizedById { get; set; }
    
    /// <summary>
    /// Date when the disposal was authorized
    /// </summary>
    public DateTime? AuthorizationDate { get; set; }
    
    /// <summary>
    /// Status of the disposal
    /// </summary>
    public DisposalStatus Status { get; set; }
    
    /// <summary>
    /// Whether this disposal has been posted to the general ledger
    /// </summary>
    public bool IsPosted { get; set; }
    
    /// <summary>
    /// Date when the disposal was posted to GL
    /// </summary>
    public DateTime? PostedDate { get; set; }
    
    /// <summary>
    /// Reference to the journal entry where this disposal was posted
    /// </summary>
    public string? JournalEntryReference { get; set; }
    
    /// <summary>
    /// Any additional notes about the disposal
    /// </summary>
    public string Notes { get; set; } = string.Empty;
    
    /// <summary>
    /// Document reference related to the disposal
    /// </summary>
    public string DocumentReference { get; set; } = string.Empty;
}
