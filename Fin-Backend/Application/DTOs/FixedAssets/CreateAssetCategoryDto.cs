using System;
using FinTech.Core.Domain.Enums.FixedAssets;

namespace FinTech.Core.Application.DTOs.FixedAssets
{
    /// <summary>
    /// Data Transfer Object for creating a new Asset Category
    /// </summary>
    public class CreateAssetCategoryDto
    {
        public string CategoryCode { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public DepreciationMethod DefaultDepreciationMethod { get; set; }
        public int DefaultUsefulLifeYears { get; set; }
        public decimal DefaultSalvageValuePercent { get; set; }
        public Guid? AssetAccountId { get; set; }
        public Guid? DepreciationExpenseAccountId { get; set; }
        public Guid? AccumulatedDepreciationAccountId { get; set; }
    }
}
