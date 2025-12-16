using System;
using System.Collections.Generic;
using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.FixedAssets
{
    public class AssetCategory : BaseEntity
    {
        public string CategoryName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal DefaultUsefulLifeYears { get; set; }
        public string DefaultDepreciationMethod { get; set; } = string.Empty;
        public string GLAccountFixedAsset { get; set; } = string.Empty;
        public string GLAccountDepreciation { get; set; } = string.Empty;
        public string GLAccountAccumDepreciation { get; set; } = string.Empty;
        public string GLAccountDisposalGain { get; set; } = string.Empty;
        public string GLAccountDisposalLoss { get; set; } = string.Empty;
        public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();

        // Additional properties for Compatibility
        public string? TenantId { get; set; }
        public string? ParentCategoryId { get; set; }
        public virtual AssetCategory? ParentCategory { get; set; }
        public virtual ICollection<AssetCategory> ChildCategories { get; set; } = new List<AssetCategory>();
        public string? CategoryCode { get; set; }
        public decimal DefaultSalvageValuePercent { get; set; }

        public string AssetAccountId { get => GLAccountFixedAsset; set => GLAccountFixedAsset = value; }
        public string DepreciationExpenseAccountId { get => GLAccountDepreciation; set => GLAccountDepreciation = value; }
        public string AccumulatedDepreciationAccountId { get => GLAccountAccumDepreciation; set => GLAccountAccumDepreciation = value; }

        public AssetCategory() { } // For EF Core

        public AssetCategory(
            string categoryName,
            string description,
            decimal defaultUsefulLifeYears,
            string defaultDepreciationMethod,
            string glAccountFixedAsset,
            string glAccountDepreciation,
            string glAccountAccumDepreciation,
            string glAccountDisposalGain,
            string glAccountDisposalLoss)
        {
            CategoryName = categoryName;
            Description = description;
            DefaultUsefulLifeYears = defaultUsefulLifeYears;
            DefaultDepreciationMethod = defaultDepreciationMethod;
            GLAccountFixedAsset = glAccountFixedAsset;
            GLAccountDepreciation = glAccountDepreciation;
            GLAccountAccumDepreciation = glAccountAccumDepreciation;
            GLAccountDisposalGain = glAccountDisposalGain;
            GLAccountDisposalLoss = glAccountDisposalLoss;
        }

        public void Update(
            string description,
            decimal defaultUsefulLifeYears,
            string defaultDepreciationMethod,
            string glAccountFixedAsset,
            string glAccountDepreciation,
            string glAccountAccumDepreciation,
            string glAccountDisposalGain,
            string glAccountDisposalLoss)
        {
            Description = description;
            DefaultUsefulLifeYears = defaultUsefulLifeYears;
            DefaultDepreciationMethod = defaultDepreciationMethod;
            GLAccountFixedAsset = glAccountFixedAsset;
            GLAccountDepreciation = glAccountDepreciation;
            GLAccountAccumDepreciation = glAccountAccumDepreciation;
            GLAccountDisposalGain = glAccountDisposalGain;
            GLAccountDisposalLoss = glAccountDisposalLoss;
        }
    }
}
