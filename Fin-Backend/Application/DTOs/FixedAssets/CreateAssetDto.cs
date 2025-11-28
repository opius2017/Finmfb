using System;
using FinTech.Core.Domain.Enums.FixedAssets;

namespace FinTech.Core.Application.DTOs.FixedAssets
{
    /// <summary>
    /// Data Transfer Object for creating a new Asset
    /// </summary>
    public class CreateAssetDto
    {
        public string AssetName { get; set; }
        public string Description { get; set; }
        public Guid AssetCategoryId { get; set; }
        public string Location { get; set; }
        public string Department { get; set; }
        public Guid? CustodianId { get; set; }
        public string AssetTag { get; set; }
        public string SerialNumber { get; set; }
        public DateTime AcquisitionDate { get; set; }
        public decimal AcquisitionCost { get; set; }
        public bool IsDepreciable { get; set; }
        public DepreciationMethod DepreciationMethod { get; set; }
        public int UsefulLifeYears { get; set; }
        public decimal SalvageValuePercent { get; set; }
        public DateTime? WarrantyExpiryDate { get; set; }
        public string Notes { get; set; }
    }
}
