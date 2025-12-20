using System;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Enums.FixedAssets;

namespace FinTech.Core.Domain.Entities.FixedAssets;

/// <summary>
/// Represents a transfer or movement of an asset from one location/department to another
/// </summary>
public class AssetTransfer : BaseEntity
{
    /// <summary>
    /// Reference to the asset
    /// </summary>
    public string AssetId { get; set; } = string.Empty;
    
    /// <summary>
    /// Navigation property for the asset
    /// </summary>
    public virtual Asset? Asset { get; set; }
    
    /// <summary>
    /// Transfer reference number
    /// </summary>
    public string TransferNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Date of the transfer
    /// </summary>
    public DateTime TransferDate { get; set; }
    
    /// <summary>
    /// Source location before transfer
    /// </summary>
    public string SourceLocation { get; set; } = string.Empty;
    
    /// <summary>
    /// Destination location after transfer
    /// </summary>
    public string DestinationLocation { get; set; } = string.Empty;
    
    /// <summary>
    /// Source department before transfer
    /// </summary>
    public string SourceDepartment { get; set; } = string.Empty;
    
    /// <summary>
    /// Destination department after transfer
    /// </summary>
    public string DestinationDepartment { get; set; } = string.Empty;
    
    /// <summary>
    /// Previous responsible employee before transfer
    /// </summary>
    public string? SourceEmployeeId { get; set; }
    
    /// <summary>
    /// New responsible employee after transfer
    /// </summary>
    public string? DestinationEmployeeId { get; set; }
    
    /// <summary>
    /// Reason for the transfer
    /// </summary>
    public string TransferReason { get; set; } = string.Empty;
    
    /// <summary>
    /// Status of the transfer
    /// </summary>
    public TransferStatus Status { get; set; }
    
    /// <summary>
    /// Employee who authorized the transfer
    /// </summary>
    public string? AuthorizedById { get; set; }
    
    /// <summary>
    /// Date when the transfer was authorized
    /// </summary>
    public DateTime? AuthorizationDate { get; set; }
    
    /// <summary>
    /// Employee who initiated the transfer request
    /// </summary>
    public string RequestedById { get; set; } = string.Empty;
    
    /// <summary>
    /// Any additional notes about the transfer
    /// </summary>
    public string Notes { get; set; } = string.Empty;
    
    /// <summary>
    /// Document reference related to the transfer
    /// </summary>
    public string DocumentReference { get; set; } = string.Empty;
}
