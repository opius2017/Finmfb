using System;

namespace FinTech.Core.Application.DTOs.FixedAssets
{
    /// <summary>
    /// Data Transfer Object for updating an Asset
    /// </summary>
    public class UpdateAssetDto
    {
        public Guid Id { get; set; }
        public string AssetName { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string Department { get; set; }
        public Guid? CustodianId { get; set; }
        public string AssetTag { get; set; }
        public string SerialNumber { get; set; }
        public string Notes { get; set; }
        public DateTime? WarrantyExpiryDate { get; set; }
    }
}
