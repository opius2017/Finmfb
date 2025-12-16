using System.Threading.Tasks;

namespace FinTech.Core.Application.Interfaces.Services.Integration
{
    public interface IFixedAssetAccountingIntegrationService
    {
        Task ProcessAssetAcquisitionAsync(string assetId, decimal acquisitionCost, decimal taxAmount, string assetCategory, 
            string reference, string description);
        Task ProcessAssetDepreciationAsync(string assetId, decimal depreciationAmount, string period, string reference, string description);
        Task ProcessAssetDisposalAsync(string assetId, decimal disposalProceeds, decimal netBookValue, decimal gainLoss, 
            string reference, string description);
        Task ProcessAssetRevaluationAsync(string assetId, decimal revaluationAmount, string reference, string description);
        Task ProcessAssetImpairmentAsync(string assetId, decimal impairmentAmount, string reference, string description);
    }
}
