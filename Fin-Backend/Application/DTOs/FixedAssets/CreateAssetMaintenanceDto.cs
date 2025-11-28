using System;
using FinTech.Core.Domain.Enums.FixedAssets;

namespace FinTech.Core.Application.DTOs.FixedAssets
{
    /// <summary>
    /// Data Transfer Object for creating a new Asset Maintenance record
    /// </summary>
    public class CreateAssetMaintenanceDto
    {
        public Guid AssetId { get; set; }
        public DateTime MaintenanceDate { get; set; }
        public MaintenanceType MaintenanceType { get; set; }
        public string Description { get; set; }
        public Guid? VendorId { get; set; }
        public decimal Cost { get; set; }
        public string Notes { get; set; }
    }
}
