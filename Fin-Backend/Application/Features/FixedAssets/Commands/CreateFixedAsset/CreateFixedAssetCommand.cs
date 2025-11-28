using MediatR;
using FinTech.Core.Application.Common.Models;

namespace FinTech.Core.Application.Features.FixedAssets.Commands.CreateFixedAsset
{
    /// <summary>
    /// Command to create a new fixed asset
    /// </summary>
    public class CreateFixedAssetCommand : IRequest<Result<CreateFixedAssetResponse>>
    {
        public string AssetCode { get; set; }
        public string AssetName { get; set; }
        public string Description { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SalvageValue { get; set; }
        public int UsefulLifeYears { get; set; }
        public string CategoryId { get; set; }
        public string LocationId { get; set; }
        public string DepartmentId { get; set; }
        public DateTime AcquisitionDate { get; set; }
    }

    /// <summary>
    /// Response DTO after creating a fixed asset
    /// </summary>
    public class CreateFixedAssetResponse
    {
        public string Id { get; set; }
        public string AssetCode { get; set; }
        public string AssetName { get; set; }
        public decimal BookValue { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
