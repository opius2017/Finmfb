using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Enums.FixedAssets;

namespace FinTech.Core.Domain.Entities.FixedAssets;

/// <summary>
/// Represents a fixed asset in the system
/// </summary>
public class Asset : BaseEntity
{
    /// <summary>
    /// Unique asset number for identification
    /// </summary>
    public string AssetNumber { get; set; } = string.Empty;

    [NotMapped]
    public string AssetCode { get => AssetNumber; set => AssetNumber = value; }
    
    /// <summary>
    /// Name of the asset
    /// </summary>
    public string AssetName { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of the asset
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Category of the asset
    /// </summary>
    public AssetCategory Category { get; set; }
    
    /// <summary>
    /// Reference to the asset category
    /// </summary>
    public string AssetCategoryId { get; set; } = string.Empty;
    
    /// <summary>
    /// Navigation property for asset category
    /// </summary>
    public virtual AssetCategory? AssetCategory { get; set; }
    
    /// <summary>
    /// Date when the asset was acquired
    /// </summary>
    public DateTime AcquisitionDate { get; set; }
    
    /// <summary>
    /// Original purchase cost of the asset
    /// </summary>
    public decimal PurchaseCost { get; set; }

    [NotMapped]
    public decimal AcquisitionCost { get => PurchaseCost; set => PurchaseCost = value; }
    
    /// <summary>
    /// Current book value of the asset
    /// </summary>
    public decimal CurrentBookValue { get; set; }
    
    /// <summary>
    /// Salvage/residual value expected at the end of useful life
    /// </summary>
    public decimal SalvageValue { get; set; }

    [NotMapped]
    public decimal ResidualValue { get => SalvageValue; set => SalvageValue = value; }
    
    /// <summary>
    /// Useful life of the asset in months
    /// </summary>
    public int UsefulLifeMonths { get; set; }
    
    /// <summary>
    /// Useful life of the asset in years (Derived/Helper)
    /// </summary>
    [NotMapped]
    public decimal UsefulLifeYears 
    { 
        get => UsefulLifeMonths / 12.0m; 
        set => UsefulLifeMonths = (int)(value * 12); 
    }

    /// <summary>
    /// Depreciation method used for this asset
    /// </summary>
    public DepreciationMethod DepreciationMethod { get; set; }
    
    /// <summary>
    /// Current status of the asset
    /// </summary>
    public AssetStatus Status { get; set; }

    [NotMapped]
    public string AssetStatus { get => Status.ToString(); set => Status = Enum.Parse<Domain.Enums.FixedAssets.AssetStatus>(value, true); }
    
    /// <summary>
    /// Barcode or tag number physically attached to the asset
    /// </summary>
    public string AssetTag { get; set; } = string.Empty;
    
    /// <summary>
    /// Serial number of the asset (if applicable)
    /// </summary>
    public string SerialNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Location where the asset is currently kept
    /// </summary>
    public string Location { get; set; } = string.Empty;
    
    /// <summary>
    /// Department that owns/uses this asset
    /// </summary>
    public string Department { get; set; } = string.Empty;
    
    /// <summary>
    /// Employee responsible for the asset
    /// </summary>
    public string? ResponsibleEmployeeId { get; set; }
    
    /// <summary>
    /// Current custodian of the asset
    /// </summary>
    public string? Custodian { get; set; }
    
    /// <summary>
    /// Vendor from whom the asset was purchased
    /// </summary>
    public string? VendorId { get; set; }
    
    /// <summary>
    /// Purchase order reference if asset was purchased through a PO
    /// </summary>
    public string PurchaseOrderReference { get; set; } = string.Empty;
    
    /// <summary>
    /// Invoice reference for the asset purchase
    /// </summary>
    public string InvoiceReference { get; set; } = string.Empty;
    
    /// <summary>
    /// Warranty expiration date
    /// </summary>
    public DateTime? WarrantyExpiryDate { get; set; }
    
    /// <summary>
    /// Notes about warranty details
    /// </summary>
    public string WarrantyNotes { get; set; } = string.Empty;
    
    /// <summary>
    /// Date when the asset is expected to be disposed
    /// </summary>
    public DateTime? PlannedDisposalDate { get; set; }
    
    /// <summary>
    /// Actual date when the asset was disposed
    /// </summary>
    public DateTime? ActualDisposalDate { get; set; }
    
    /// <summary>
    /// Amount received when asset was disposed
    /// </summary>
    public decimal? DisposalProceeds { get; set; }
    
    /// <summary>
    /// Gain/loss on disposal (calculated as difference between book value and disposal proceeds)
    /// </summary>
    public decimal? DisposalGainLoss { get; set; }
    
    /// <summary>
    /// Insurance policy number covering this asset
    /// </summary>
    public string InsurancePolicyNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Insurance coverage amount
    /// </summary>
    public decimal? InsuredValue { get; set; }
    
    /// <summary>
    /// Insurance policy expiration date
    /// </summary>
    public DateTime? InsuranceExpiryDate { get; set; }
    
    /// <summary>
    /// Any additional notes about the asset
    /// </summary>
    public string Notes { get; set; } = string.Empty;
    
    /// <summary>
    /// Custom fields stored as JSON
    /// </summary>
    public string CustomFields { get; set; } = string.Empty;
    
    /// <summary>
    /// Navigation property for depreciation schedules
    /// </summary>
    public virtual ICollection<AssetDepreciationSchedule> DepreciationSchedules { get; set; } = new List<AssetDepreciationSchedule>();
    
    /// <summary>
    /// Navigation property for maintenance records
    /// </summary>
    public virtual ICollection<AssetMaintenance> MaintenanceRecords { get; set; } = new List<AssetMaintenance>();
    
    /// <summary>
    /// Navigation property for asset movements/transfers
    /// </summary>
    public virtual ICollection<AssetTransfer> Transfers { get; set; } = new List<AssetTransfer>();
    
    /// <summary>
    /// Navigation property for asset revaluations
    /// </summary>
    public virtual ICollection<AssetRevaluation> Revaluations { get; set; } = new List<AssetRevaluation>();

    [NotMapped]
    public decimal AccumulatedDepreciation 
    { 
        get => PurchaseCost - CurrentBookValue; 
        set => CurrentBookValue = PurchaseCost - value;
    }

    [NotMapped]
    public bool IsDepreciable => DepreciationMethod != DepreciationMethod.None;

    [NotMapped]
    public decimal BookValue { get => CurrentBookValue; set => CurrentBookValue = value; }

    [NotMapped]
    public decimal SalvageValuePercent 
    { 
        get => PurchaseCost > 0 ? (SalvageValue / PurchaseCost) * 100 : 0; 
        set => SalvageValue = PurchaseCost * (value / 100); 
    }
}
