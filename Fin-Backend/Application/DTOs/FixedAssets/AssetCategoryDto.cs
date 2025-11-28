using System;
using FinTech.Core.Domain.Enums.FixedAssets;

namespace FinTech.Core.Application.DTOs.FixedAssets
{
    /// <summary>
    /// Data Transfer Object for Asset Category information
    /// </summary>
    public class AssetCategoryDto
    {
        public Guid Id { get; set; }
        public string CategoryCode { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public string ParentCategoryName { get; set; }
        public DepreciationMethod DefaultDepreciationMethod { get; set; }
        public int DefaultUsefulLifeYears { get; set; }
        public decimal DefaultSalvageValuePercent { get; set; }
        public Guid? AssetAccountId { get; set; }
        public Guid? DepreciationExpenseAccountId { get; set; }
        public Guid? AccumulatedDepreciationAccountId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
