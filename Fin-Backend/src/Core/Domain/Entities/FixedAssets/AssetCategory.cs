using System;
using System.Collections.Generic;
using FinTech.Domain.Entities.Common;

namespace FinTech.Domain.Entities.FixedAssets;

/// <summary>
/// Represents a category of fixed assets
/// </summary>
public class AssetCategory : BaseEntity
{
    /// <summary>
    /// Category code
    /// </summary>
    public string CategoryCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Category name
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of the category
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Default useful life in months for assets in this category
    /// </summary>
    public int DefaultUsefulLifeMonths { get; set; }
    
    /// <summary>
    /// Default depreciation method for assets in this category
    /// </summary>
    public int DefaultDepreciationMethodId { get; set; }
    
    /// <summary>
    /// Default depreciation rate (annual percentage)
    /// </summary>
    public decimal DefaultDepreciationRate { get; set; }
    
    /// <summary>
    /// Default salvage value percentage of original cost
    /// </summary>
    public decimal DefaultSalvageValuePercentage { get; set; }
    
    /// <summary>
    /// General ledger asset account for this category
    /// </summary>
    public Guid? GLAssetAccountId { get; set; }
    
    /// <summary>
    /// General ledger accumulated depreciation account for this category
    /// </summary>
    public Guid? GLAccumulatedDepreciationAccountId { get; set; }
    
    /// <summary>
    /// General ledger depreciation expense account for this category
    /// </summary>
    public Guid? GLDepreciationExpenseAccountId { get; set; }
    
    /// <summary>
    /// General ledger disposal gain/loss account for this category
    /// </summary>
    public Guid? GLDisposalGainLossAccountId { get; set; }
    
    /// <summary>
    /// Whether assets in this category require tracking by serial number
    /// </summary>
    public bool RequireSerialNumber { get; set; }
    
    /// <summary>
    /// Whether assets in this category require insurance
    /// </summary>
    public bool RequireInsurance { get; set; }
    
    /// <summary>
    /// Whether assets in this category are subject to depreciation
    /// </summary>
    public bool IsDepreciable { get; set; } = true;
    
    /// <summary>
    /// Whether this category is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Parent category ID for hierarchical categorization
    /// </summary>
    public Guid? ParentCategoryId { get; set; }
    
    /// <summary>
    /// Navigation property for parent category
    /// </summary>
    public virtual AssetCategory? ParentCategory { get; set; }
    
    /// <summary>
    /// Navigation property for child categories
    /// </summary>
    public virtual ICollection<AssetCategory> ChildCategories { get; set; } = new List<AssetCategory>();
    
    /// <summary>
    /// Navigation property for assets in this category
    /// </summary>
    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();
}