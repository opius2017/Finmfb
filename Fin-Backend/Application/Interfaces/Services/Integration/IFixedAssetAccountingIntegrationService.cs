using System.Threading.Tasks;

namespace FinTech.Core.Application.Services.Integration
{
    public interface IFixedAssetAccountingIntegrationService
    {
        Task ProcessAssetAcquisitionAsync(int assetId, decimal acquisitionCost, decimal taxAmount, string assetCategory, 
            string reference, string description);
        Task ProcessAssetDepreciationAsync(int assetId, decimal depreciationAmount, string period, string reference, string description);
        Task ProcessAssetDisposalAsync(int assetId, decimal disposalProceeds, decimal netBookValue, decimal gainLoss, 
            string reference, string description);
        Task ProcessAssetRevaluationAsync(int assetId, decimal revaluationAmount, string reference, string description);
        Task ProcessAssetImpairmentAsync(int assetId, decimal impairmentAmount, string reference, string description);
    }
}
