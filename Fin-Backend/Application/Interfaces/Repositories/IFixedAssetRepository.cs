using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Domain.Entities.FixedAssets;
using FinTech.Domain.Enums.FixedAssets;

namespace FinTech.Core.Application.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for Fixed Asset operations
    /// </summary>
    public interface IFixedAssetRepository
    {
        // Asset operations
        Task<Asset> GetAssetByIdAsync(Guid id);
        Task<Asset> GetAssetByNumberAsync(string assetNumber);
        Task<IEnumerable<Asset>> GetAssetsByStatusAsync(AssetStatus status);
        Task<IEnumerable<Asset>> GetAssetsByCategoryAsync(Guid categoryId);
        Task<IEnumerable<Asset>> GetAssetsByDepartmentAsync(string department);
        Task<IEnumerable<Asset>> GetAssetsByLocationAsync(string location);
        Task<IEnumerable<Asset>> GetFullyDepreciatedAssetsAsync();
        Task<IEnumerable<Asset>> GetAssetsNearingEndOfLifeAsync(int monthsThreshold);
        Task<Asset> AddAssetAsync(Asset asset);
        Task<Asset> UpdateAssetAsync(Asset asset);
        Task<bool> DeleteAssetAsync(Guid id);
        Task<decimal> GetTotalAssetValueAsync();
        Task<IDictionary<string, decimal>> GetAssetValueByDepartmentAsync();
        Task<IDictionary<string, decimal>> GetAssetValueByCategoryAsync();

        // Asset Category operations
        Task<AssetCategory> GetAssetCategoryByIdAsync(Guid id);
        Task<AssetCategory> GetAssetCategoryByCodeAsync(string categoryCode, Guid tenantId);
        Task<IEnumerable<AssetCategory>> GetAllAssetCategoriesAsync(Guid tenantId);
        Task<AssetCategory> AddAssetCategoryAsync(AssetCategory category);
        Task<AssetCategory> UpdateAssetCategoryAsync(AssetCategory category);
        Task<bool> DeleteAssetCategoryAsync(Guid id);

        // Depreciation Schedule operations
        Task<IEnumerable<AssetDepreciationSchedule>> GetDepreciationScheduleForAssetAsync(Guid assetId);
        Task<IEnumerable<AssetDepreciationSchedule>> AddDepreciationSchedulesAsync(IEnumerable<AssetDepreciationSchedule> schedules);
        Task<IEnumerable<AssetDepreciationSchedule>> GetUnpostedDepreciationSchedulesAsync(DateTime asOfDate);
        Task<decimal> GetTotalDepreciationForPeriodAsync(DateTime periodStart, DateTime periodEnd);
        Task<bool> UpdateDepreciationSchedulesAsync(IEnumerable<AssetDepreciationSchedule> schedules);

        // Asset Maintenance operations
        Task<AssetMaintenance> GetMaintenanceRecordByIdAsync(Guid id);
        Task<IEnumerable<AssetMaintenance>> GetMaintenanceHistoryForAssetAsync(Guid assetId);
        Task<IEnumerable<AssetMaintenance>> GetScheduledMaintenanceAsync(DateTime fromDate, DateTime toDate);
        Task<AssetMaintenance> AddMaintenanceRecordAsync(AssetMaintenance maintenance);
        Task<AssetMaintenance> UpdateMaintenanceRecordAsync(AssetMaintenance maintenance);
        Task<bool> DeleteMaintenanceRecordAsync(Guid id);
        Task<string> GenerateMaintenanceNumberAsync(Guid tenantId, DateTime date);

        // Asset Transfer operations
        Task<AssetTransfer> GetAssetTransferByIdAsync(Guid id);
        Task<IEnumerable<AssetTransfer>> GetTransferHistoryForAssetAsync(Guid assetId);
        Task<AssetTransfer> AddAssetTransferAsync(AssetTransfer transfer);
        Task<AssetTransfer> UpdateAssetTransferAsync(AssetTransfer transfer);
        Task<string> GenerateTransferNumberAsync(Guid tenantId, DateTime date);

        // Asset Inventory Count operations
        Task<AssetInventoryCount> GetInventoryCountByIdAsync(Guid id);
        Task<IEnumerable<AssetInventoryCount>> GetInventoryCountsByStatusAsync(InventoryCountStatus status);
        Task<AssetInventoryCount> AddInventoryCountAsync(AssetInventoryCount inventoryCount);
        Task<AssetInventoryCount> UpdateInventoryCountAsync(AssetInventoryCount inventoryCount);
        Task<AssetInventoryCountItem> AddInventoryCountItemAsync(AssetInventoryCountItem countItem);
        Task<IEnumerable<AssetInventoryCountItem>> GetDiscrepanciesForInventoryCountAsync(Guid inventoryCountId);
        Task<string> GenerateInventoryCountNumberAsync(Guid tenantId, DateTime date);

        // Asset Disposal operations
        Task<AssetDisposal> GetAssetDisposalByIdAsync(Guid id);
        Task<IEnumerable<AssetDisposal>> GetAssetDisposalsByStatusAsync(DisposalStatus status);
        Task<AssetDisposal> AddAssetDisposalAsync(AssetDisposal disposal);
        Task<AssetDisposal> UpdateAssetDisposalAsync(AssetDisposal disposal);
        Task<string> GenerateDisposalNumberAsync(Guid tenantId, DateTime date);

        // Asset Revaluation operations
        Task<AssetRevaluation> GetAssetRevaluationByIdAsync(Guid id);
        Task<IEnumerable<AssetRevaluation>> GetRevaluationHistoryForAssetAsync(Guid assetId);
        Task<AssetRevaluation> AddAssetRevaluationAsync(AssetRevaluation revaluation);
        Task<AssetRevaluation> UpdateAssetRevaluationAsync(AssetRevaluation revaluation);
    }
}