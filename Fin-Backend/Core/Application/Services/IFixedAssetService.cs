using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.FixedAssets;
using FinTech.Core.Domain.Enums.FixedAssets;

namespace FinTech.Core.Application.Services;

/// <summary>
/// Interface for Fixed Asset Management Service
/// </summary>
public interface IFixedAssetService
{
    // Asset Management
    Task<Asset> CreateAssetAsync(Asset asset);
    Task<Asset> GetAssetByIdAsync(Guid id);
    Task<Asset> GetAssetByNumberAsync(string assetNumber);
    Task<IEnumerable<Asset>> GetAssetsByStatusAsync(AssetStatus status);
    Task<IEnumerable<Asset>> GetAssetsByCategoryAsync(Guid categoryId);
    Task<IEnumerable<Asset>> GetAssetsByDepartmentAsync(string department);
    Task<IEnumerable<Asset>> GetAssetsByLocationAsync(string location);
    Task<Asset> UpdateAssetAsync(Asset asset);
    Task<bool> DeleteAssetAsync(Guid id);
    
    // Asset Category Management
    Task<AssetCategory> CreateAssetCategoryAsync(AssetCategory category);
    Task<AssetCategory> GetAssetCategoryByIdAsync(Guid id);
    Task<AssetCategory> GetAssetCategoryByCodeAsync(string categoryCode);
    Task<IEnumerable<AssetCategory>> GetAllAssetCategoriesAsync();
    Task<AssetCategory> UpdateAssetCategoryAsync(AssetCategory category);
    Task<bool> DeleteAssetCategoryAsync(Guid id);
    
    // Depreciation Management
    Task<IEnumerable<AssetDepreciationSchedule>> GenerateDepreciationScheduleAsync(Guid assetId);
    Task<IEnumerable<AssetDepreciationSchedule>> GetDepreciationScheduleForAssetAsync(Guid assetId);
    Task<decimal> CalculateMonthlyDepreciationAsync(Guid assetId);
    Task<bool> ProcessMonthlyDepreciationAsync(DateTime asOfDate);
    Task<bool> PostDepreciationToGLAsync(DateTime periodStart, DateTime periodEnd);
    
    // Asset Maintenance Management
    Task<AssetMaintenance> CreateMaintenanceRecordAsync(AssetMaintenance maintenance);
    Task<AssetMaintenance> GetMaintenanceRecordByIdAsync(Guid id);
    Task<IEnumerable<AssetMaintenance>> GetMaintenanceHistoryForAssetAsync(Guid assetId);
    Task<IEnumerable<AssetMaintenance>> GetScheduledMaintenanceAsync(DateTime fromDate, DateTime toDate);
    Task<AssetMaintenance> UpdateMaintenanceRecordAsync(AssetMaintenance maintenance);
    Task<bool> DeleteMaintenanceRecordAsync(Guid id);
    
    // Asset Transfer Management
    Task<AssetTransfer> CreateAssetTransferAsync(AssetTransfer transfer);
    Task<AssetTransfer> GetAssetTransferByIdAsync(Guid id);
    Task<IEnumerable<AssetTransfer>> GetTransferHistoryForAssetAsync(Guid assetId);
    Task<AssetTransfer> UpdateAssetTransferAsync(AssetTransfer transfer);
    Task<bool> ApproveAssetTransferAsync(Guid transferId, Guid approverId);
    Task<bool> CompleteAssetTransferAsync(Guid transferId);
    
    // Asset Inventory Management
    Task<AssetInventoryCount> CreateInventoryCountAsync(AssetInventoryCount inventoryCount);
    Task<AssetInventoryCount> GetInventoryCountByIdAsync(Guid id);
    Task<IEnumerable<AssetInventoryCount>> GetInventoryCountsByStatusAsync(InventoryCountStatus status);
    Task<AssetInventoryCount> UpdateInventoryCountAsync(AssetInventoryCount inventoryCount);
    Task<bool> CompleteInventoryCountAsync(Guid inventoryCountId);
    Task<AssetInventoryCountItem> RecordInventoryCountItemAsync(AssetInventoryCountItem countItem);
    Task<IEnumerable<AssetInventoryCountItem>> GetDiscrepanciesForInventoryCountAsync(Guid inventoryCountId);
    
    // Asset Disposal Management
    Task<AssetDisposal> CreateAssetDisposalAsync(AssetDisposal disposal);
    Task<AssetDisposal> GetAssetDisposalByIdAsync(Guid id);
    Task<IEnumerable<AssetDisposal>> GetAssetDisposalsByStatusAsync(DisposalStatus status);
    Task<AssetDisposal> UpdateAssetDisposalAsync(AssetDisposal disposal);
    Task<bool> ApproveAssetDisposalAsync(Guid disposalId, Guid approverId);
    Task<bool> CompleteAssetDisposalAsync(Guid disposalId);
    Task<bool> PostDisposalToGLAsync(Guid disposalId);
    
    // Asset Revaluation Management
    Task<AssetRevaluation> CreateAssetRevaluationAsync(AssetRevaluation revaluation);
    Task<AssetRevaluation> GetAssetRevaluationByIdAsync(Guid id);
    Task<IEnumerable<AssetRevaluation>> GetRevaluationHistoryForAssetAsync(Guid assetId);
    Task<AssetRevaluation> UpdateAssetRevaluationAsync(AssetRevaluation revaluation);
    Task<bool> PostRevaluationToGLAsync(Guid revaluationId);
    
    // Reporting
    Task<decimal> GetTotalAssetValueAsync();
    Task<decimal> GetTotalDepreciationForPeriodAsync(DateTime periodStart, DateTime periodEnd);
    Task<IDictionary<string, decimal>> GetAssetValueByDepartmentAsync();
    Task<IDictionary<string, decimal>> GetAssetValueByCategoryAsync();
    Task<IEnumerable<Asset>> GetFullyDepreciatedAssetsAsync();
    Task<IEnumerable<Asset>> GetAssetsNearingEndOfLifeAsync(int monthsThreshold);
}
