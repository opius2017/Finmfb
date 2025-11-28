using System;
using FinTech.Core.Domain.Enums.FixedAssets;

namespace FinTech.Core.Application.DTOs.FixedAssets
{
    /// <summary>
    /// Data Transfer Object for Asset information
    /// </summary>
    public class AssetDto
    {
        public Guid Id { get; set; }
        public string AssetNumber { get; set; }
        public string AssetName { get; set; }
        public string Description { get; set; }
        public Guid AssetCategoryId { get; set; }
        public string AssetCategoryName { get; set; }
        public string Location { get; set; }
        public string Department { get; set; }
        public Guid? CustodianId { get; set; }
        public string CustodianName { get; set; }
        public string AssetTag { get; set; }
        public string SerialNumber { get; set; }
        public AssetStatus Status { get; set; }
        public DateTime AcquisitionDate { get; set; }
        public decimal AcquisitionCost { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal BookValue { get; set; }
        public decimal AccumulatedDepreciation { get; set; }
        public bool IsDepreciable { get; set; }
        public DepreciationMethod DepreciationMethod { get; set; }
        public int UsefulLifeYears { get; set; }
        public decimal SalvageValuePercent { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }
        public DateTime? WarrantyExpiryDate { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedById { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedById { get; set; }
    }
}
