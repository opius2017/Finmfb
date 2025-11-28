using System;
using FinTech.Core.Domain.Enums.FixedAssets;

namespace FinTech.Core.Application.DTOs.FixedAssets
{
    /// <summary>
    /// Data Transfer Object for Asset Maintenance information
    /// </summary>
    public class AssetMaintenanceDto
    {
        public Guid Id { get; set; }
        public string MaintenanceNumber { get; set; }
        public Guid AssetId { get; set; }
        public string AssetNumber { get; set; }
        public string AssetName { get; set; }
        public DateTime MaintenanceDate { get; set; }
        public MaintenanceType MaintenanceType { get; set; }
        public MaintenanceStatus Status { get; set; }
        public string Description { get; set; }
        public Guid? VendorId { get; set; }
        public string VendorName { get; set; }
        public decimal Cost { get; set; }
        public string Notes { get; set; }
        public DateTime? CompletionDate { get; set; }
        public Guid? CompletedById { get; set; }
    }
}
