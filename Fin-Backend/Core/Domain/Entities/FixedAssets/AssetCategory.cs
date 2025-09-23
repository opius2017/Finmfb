using System;
using System.Collections.Generic;
using FinTech.Domain.Common;
using FinTech.Domain.Entities.Common;

namespace FinTech.Domain.Entities.FixedAssets
{
    public class AssetCategory : BaseEntity
    {
        public string CategoryName { get; private set; }
        public string Description { get; private set; }
        public decimal DefaultUsefulLifeYears { get; private set; }
        public string DefaultDepreciationMethod { get; private set; }
        public string GLAccountFixedAsset { get; private set; }
        public string GLAccountDepreciation { get; private set; }
        public string GLAccountAccumDepreciation { get; private set; }
        public string GLAccountDisposalGain { get; private set; }
        public string GLAccountDisposalLoss { get; private set; }
        public virtual ICollection<FixedAsset> Assets { get; private set; } = new List<FixedAsset>();

        private AssetCategory() { } // For EF Core

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