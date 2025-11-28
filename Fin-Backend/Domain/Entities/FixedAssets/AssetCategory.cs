using System;
using System.Collections.Generic;
using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.FixedAssets
{
    public class AssetCategory : BaseEntity
    {
        // Make properties writable for Application layer compatibility
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public decimal DefaultUsefulLifeYears { get; set; }
        public string DefaultDepreciationMethod { get; set; }
        public string GLAccountFixedAsset { get; set; }
        public string GLAccountDepreciation { get; set; }
        public string GLAccountAccumDepreciation { get; set; }
        public string GLAccountDisposalGain { get; set; }
        public string GLAccountDisposalLoss { get; set; }
        public virtual ICollection<FixedAsset> Assets { get; set; } = new List<FixedAsset>();

        // Compatibility shims for Application layer
        public string ParentCategoryId { get; set; }
        
        public decimal DefaultSalvageValuePercent { get; set; } = 0m;
        
        // Alias GL accounts to expected Application names
        public string AssetAccountId
        {
            get => GLAccountFixedAsset;
            set => GLAccountFixedAsset = value;
        }
        
        public string DepreciationExpenseAccountId
        {
            get => GLAccountDepreciation;
            set => GLAccountDepreciation = value;
        }
        
        public string AccumulatedDepreciationAccountId
        {
            get => GLAccountAccumDepreciation;
            set => GLAccountAccumDepreciation = value;
        }
        
        public string UpdatedById { get; set; }

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
