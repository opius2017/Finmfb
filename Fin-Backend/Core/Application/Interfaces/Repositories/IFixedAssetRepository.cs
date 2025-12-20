using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.FixedAssets;
using FinTech.Core.Domain.Enums.FixedAssets;

namespace FinTech.Core.Application.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for Fixed Asset operations
    /// </summary>
    public interface IFixedAssetRepository
    {
        // Asset operations
        Task<Asset> GetAssetByIdAsync(string id);
        Task<Asset> GetAssetByNumberAsync(string assetNumber);
        Task<IEnumerable<Asset>> GetAssetsByStatusAsync(AssetStatus status);
        Task<IEnumerable<Asset>> GetAssetsByCategoryAsync(string categoryId);
        Task<IEnumerable<Asset>> GetAssetsByDepartmentAsync(string department);
        Task<IEnumerable<Asset>> GetAssetsByLocationAsync(string location);
        Task<IEnumerable<Asset>> GetFullyDepreciatedAssetsAsync();
        Task<IEnumerable<Asset>> GetAssetsNearingEndOfLifeAsync(int monthsThreshold);
        Task<Asset> AddAssetAsync(Asset asset);
        Task<Asset> UpdateAssetAsync(Asset asset);
        Task<bool> DeleteAssetAsync(string id);
        Task<decimal> GetTotalAssetValueAsync();
        Task<IDictionary<string, decimal>> GetAssetValueByDepartmentAsync();
        Task<IDictionary<string, decimal>> GetAssetValueByCategoryAsync();

        // Asset Category operations
        Task<AssetCategory> GetAssetCategoryByIdAsync(string id);
        Task<AssetCategory> GetAssetCategoryByCodeAsync(string categoryCode, string tenantId);
        Task<IEnumerable<AssetCategory>> GetAllAssetCategoriesAsync(string tenantId);
        Task<AssetCategory> AddAssetCategoryAsync(AssetCategory category);
        Task<AssetCategory> UpdateAssetCategoryAsync(AssetCategory category);
        Task<bool> DeleteAssetCategoryAsync(string id);

        // Depreciation Schedule operations
        Task<IEnumerable<AssetDepreciationSchedule>> GetDepreciationScheduleForAssetAsync(string assetId);
        Task<IEnumerable<AssetDepreciationSchedule>> AddDepreciationSchedulesAsync(IEnumerable<AssetDepreciationSchedule> schedules);
        Task<IEnumerable<AssetDepreciationSchedule>> GetUnpostedDepreciationSchedulesAsync(DateTime asOfDate);
        Task<decimal> GetTotalDepreciationForPeriodAsync(DateTime periodStart, DateTime periodEnd);
        Task<bool> UpdateDepreciationSchedulesAsync(IEnumerable<AssetDepreciationSchedule> schedules);

        // Asset Maintenance operations
        Task<AssetMaintenance> GetMaintenanceRecordByIdAsync(string id);
        Task<IEnumerable<AssetMaintenance>> GetMaintenanceHistoryForAssetAsync(string assetId);
        Task<IEnumerable<AssetMaintenance>> GetScheduledMaintenanceAsync(DateTime fromDate, DateTime toDate);
        Task<AssetMaintenance> AddMaintenanceRecordAsync(AssetMaintenance maintenance);
        Task<AssetMaintenance> UpdateMaintenanceRecordAsync(AssetMaintenance maintenance);
        Task<bool> DeleteMaintenanceRecordAsync(string id);
        Task<string> GenerateMaintenanceNumberAsync(string tenantId, DateTime date);

        // Asset Transfer operations
        Task<AssetTransfer> GetAssetTransferByIdAsync(string id);
        Task<IEnumerable<AssetTransfer>> GetTransferHistoryForAssetAsync(string assetId);
        Task<AssetTransfer> AddAssetTransferAsync(AssetTransfer transfer);
        Task<AssetTransfer> UpdateAssetTransferAsync(AssetTransfer transfer);
        Task<string> GenerateTransferNumberAsync(string tenantId, DateTime date);

        // Asset Inventory Count operations
        Task<AssetInventoryCount> GetInventoryCountByIdAsync(string id);
        Task<IEnumerable<AssetInventoryCount>> GetInventoryCountsByStatusAsync(InventoryCountStatus status);
        Task<AssetInventoryCount> AddInventoryCountAsync(AssetInventoryCount inventoryCount);
        Task<AssetInventoryCount> UpdateInventoryCountAsync(AssetInventoryCount inventoryCount);
        Task<AssetInventoryCountItem> AddInventoryCountItemAsync(AssetInventoryCountItem countItem);
        Task<IEnumerable<AssetInventoryCountItem>> GetDiscrepanciesForInventoryCountAsync(string inventoryCountId);
        Task<string> GenerateInventoryCountNumberAsync(string tenantId, DateTime date);

        // Asset Disposal operations
        Task<AssetDisposal> GetAssetDisposalByIdAsync(string id);
        Task<IEnumerable<AssetDisposal>> GetAssetDisposalsByStatusAsync(DisposalStatus status);
        Task<AssetDisposal> AddAssetDisposalAsync(AssetDisposal disposal);
        Task<AssetDisposal> UpdateAssetDisposalAsync(AssetDisposal disposal);
        Task<string> GenerateDisposalNumberAsync(string tenantId, DateTime date);

        // Asset Revaluation operations
        Task<AssetRevaluation> GetAssetRevaluationByIdAsync(string id);
        Task<IEnumerable<AssetRevaluation>> GetRevaluationHistoryForAssetAsync(string assetId);
        Task<AssetRevaluation> AddAssetRevaluationAsync(AssetRevaluation revaluation);
        Task<AssetRevaluation> UpdateAssetRevaluationAsync(AssetRevaluation revaluation);
    }
}
