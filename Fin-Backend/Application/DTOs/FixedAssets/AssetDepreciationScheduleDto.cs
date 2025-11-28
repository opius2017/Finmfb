using System;

namespace FinTech.Core.Application.DTOs.FixedAssets
{
    /// <summary>
    /// Data Transfer Object for Asset Depreciation Schedule information
    /// </summary>
    public class AssetDepreciationScheduleDto
    {
        public Guid Id { get; set; }
        public Guid AssetId { get; set; }
        public string AssetNumber { get; set; }
        public string AssetName { get; set; }
        public int PeriodNumber { get; set; }
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public decimal DepreciationAmount { get; set; }
        public decimal BookValueBeforeDepreciation { get; set; }
        public decimal BookValueAfterDepreciation { get; set; }
        public bool IsPosted { get; set; }
        public DateTime? PostedDate { get; set; }
        public bool PostedToGL { get; set; }
        public DateTime? PostedToGLDate { get; set; }
    }
}
