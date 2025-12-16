using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FinTech.Core.Application.Common.Interfaces;
using FinTech.Core.Application.Interfaces.Repositories;
using FinTech.Core.Domain.Entities.FixedAssets;
using FinTech.Core.Domain.Enums.FixedAssets;

namespace FinTech.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for Fixed Asset operations
    /// </summary>
    public class FixedAssetRepository : IFixedAssetRepository
    {
        private readonly IApplicationDbContext _dbContext;

        public FixedAssetRepository(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        #region Asset Operations

        public async Task<Asset> GetAssetByIdAsync(Guid id)
        {
            return await _dbContext.Assets
                .Include(a => a.AssetCategory)
                .FirstOrDefaultAsync(a => a.Id == id.ToString());
        }

        public async Task<Asset> GetAssetByNumberAsync(string assetNumber)
        {
            return await _dbContext.Assets
                .Include(a => a.AssetCategory)
                .FirstOrDefaultAsync(a => a.AssetNumber == assetNumber);
        }

        public async Task<IEnumerable<Asset>> GetAssetsByStatusAsync(AssetStatus status)
        {
            return await _dbContext.Assets
                .Include(a => a.AssetCategory)
                .Where(a => a.Status == status)
                .ToListAsync();
        }

        public async Task<IEnumerable<Asset>> GetAssetsByCategoryAsync(Guid categoryId)
        {
            return await _dbContext.Assets
                .Include(a => a.AssetCategory)
                .Where(a => a.AssetCategoryId == categoryId.ToString())
                .ToListAsync();
        }

        public async Task<IEnumerable<Asset>> GetAssetsByDepartmentAsync(string department)
        {
            return await _dbContext.Assets
                .Include(a => a.AssetCategory)
                .Where(a => a.Department == department)
                .ToListAsync();
        }

        public async Task<IEnumerable<Asset>> GetAssetsByLocationAsync(string location)
        {
            return await _dbContext.Assets
                .Include(a => a.AssetCategory)
                .Where(a => a.Location == location)
                .ToListAsync();
        }

        public async Task<IEnumerable<Asset>> GetFullyDepreciatedAssetsAsync()
        {
            return await _dbContext.Assets
                .Include(a => a.AssetCategory)
                .Where(a => a.IsDepreciable && a.BookValue <= a.AcquisitionCost * (a.SalvageValuePercent / 100))
                .ToListAsync();
        }

        public async Task<IEnumerable<Asset>> GetAssetsNearingEndOfLifeAsync(int monthsThreshold)
        {
            var thresholdDate = DateTime.UtcNow.AddMonths(monthsThreshold);
            
            return await _dbContext.Assets
                .Include(a => a.AssetCategory)
                .Where(a => a.IsDepreciable && 
                       a.AcquisitionDate.AddMonths((int)a.UsefulLifeYears * 12) <= thresholdDate &&
                       a.Status == AssetStatus.Active)
                .ToListAsync();
        }

        public async Task<Asset> AddAssetAsync(Asset asset)
        {
            await _dbContext.Assets.AddAsync(asset);
            await _dbContext.SaveChangesAsync();
            return asset;
        }

        public async Task<Asset> UpdateAssetAsync(Asset asset)
        {
            _dbContext.Assets.Update(asset);
            await _dbContext.SaveChangesAsync();
            return asset;
        }

        public async Task<bool> DeleteAssetAsync(Guid id)
        {
            var asset = await _dbContext.Assets.FindAsync(id.ToString());
            if (asset == null)
                return false;
                
            _dbContext.Assets.Remove(asset);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> GetTotalAssetValueAsync()
        {
            return await _dbContext.Assets
                .Where(a => a.Status == AssetStatus.Active)
                .SumAsync(a => a.BookValue);
        }

        public async Task<IDictionary<string, decimal>> GetAssetValueByDepartmentAsync()
        {
            return await _dbContext.Assets
                .Where(a => a.Status == AssetStatus.Active && !string.IsNullOrEmpty(a.Department))
                .GroupBy(a => a.Department)
                .Select(g => new { Department = g.Key, Value = g.Sum(a => a.BookValue) })
                .ToDictionaryAsync(x => x.Department, x => x.Value);
        }

        public async Task<IDictionary<string, decimal>> GetAssetValueByCategoryAsync()
        {
            return await _dbContext.Assets
                .Where(a => a.Status == AssetStatus.Active)
                .Include(a => a.AssetCategory)
                .GroupBy(a => a.AssetCategory.CategoryName)
                .Select(g => new { Category = g.Key, Value = g.Sum(a => a.BookValue) })
                .ToDictionaryAsync(x => x.Category, x => x.Value);
        }

        #endregion

        #region Asset Category Operations

        public async Task<AssetCategory> GetAssetCategoryByIdAsync(Guid id)
        {
            return await _dbContext.AssetCategories
                .Include(c => c.ParentCategory)
                .Include(c => c.ChildCategories)
                .FirstOrDefaultAsync(c => c.Id == id.ToString());
        }

        public async Task<AssetCategory> GetAssetCategoryByCodeAsync(string categoryCode, Guid tenantId)
        {
            return await _dbContext.AssetCategories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(c => c.CategoryCode == categoryCode && c.TenantId == tenantId.ToString());
        }

        public async Task<IEnumerable<AssetCategory>> GetAllAssetCategoriesAsync(Guid tenantId)
        {
            return await _dbContext.AssetCategories
                .Where(c => c.TenantId == tenantId.ToString())
                .Include(c => c.ParentCategory)
                .ToListAsync();
        }

        public async Task<AssetCategory> AddAssetCategoryAsync(AssetCategory category)
        {
            await _dbContext.AssetCategories.AddAsync(category);
            await _dbContext.SaveChangesAsync();
            return category;
        }

        public async Task<AssetCategory> UpdateAssetCategoryAsync(AssetCategory category)
        {
            _dbContext.AssetCategories.Update(category);
            await _dbContext.SaveChangesAsync();
            return category;
        }

        public async Task<bool> DeleteAssetCategoryAsync(Guid id)
        {
            var category = await _dbContext.AssetCategories.FindAsync(id.ToString());
            if (category == null)
                return false;
                
            _dbContext.AssetCategories.Remove(category);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Depreciation Schedule Operations

        public async Task<IEnumerable<AssetDepreciationSchedule>> GetDepreciationScheduleForAssetAsync(Guid assetId)
        {
            return await _dbContext.AssetDepreciationSchedules
                .Where(s => s.AssetId == assetId.ToString())
                .OrderBy(s => s.PeriodNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<AssetDepreciationSchedule>> AddDepreciationSchedulesAsync(IEnumerable<AssetDepreciationSchedule> schedules)
        {
            await _dbContext.AssetDepreciationSchedules.AddRangeAsync(schedules);
            await _dbContext.SaveChangesAsync();
            return schedules;
        }

        public async Task<IEnumerable<AssetDepreciationSchedule>> GetUnpostedDepreciationSchedulesAsync(DateTime asOfDate)
        {
            return await _dbContext.AssetDepreciationSchedules
                .Include(s => s.Asset)
                .Where(s => 
                    s.PeriodStartDate <= asOfDate && 
                    s.PeriodEndDate >= asOfDate &&
                    !s.IsPosted &&
                    s.Asset.Status == AssetStatus.Active)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalDepreciationForPeriodAsync(DateTime periodStart, DateTime periodEnd)
        {
            return await _dbContext.AssetDepreciationSchedules
                .Where(s => 
                    s.PeriodStartDate >= periodStart && 
                    s.PeriodEndDate <= periodEnd &&
                    s.IsPosted)
                .SumAsync(s => s.DepreciationAmount);
        }

        public async Task<bool> UpdateDepreciationSchedulesAsync(IEnumerable<AssetDepreciationSchedule> schedules)
        {
            _dbContext.AssetDepreciationSchedules.UpdateRange(schedules);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Asset Maintenance Operations

        public async Task<AssetMaintenance> GetMaintenanceRecordByIdAsync(Guid id)
        {
            return await _dbContext.AssetMaintenances
                .Include(m => m.Asset)
                .FirstOrDefaultAsync(m => m.Id == id.ToString());
        }

        public async Task<IEnumerable<AssetMaintenance>> GetMaintenanceHistoryForAssetAsync(Guid assetId)
        {
            return await _dbContext.AssetMaintenances
                .Where(m => m.AssetId == assetId.ToString())
                .OrderByDescending(m => m.MaintenanceDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<AssetMaintenance>> GetScheduledMaintenanceAsync(DateTime fromDate, DateTime toDate)
        {
            return await _dbContext.AssetMaintenances
                .Include(m => m.Asset)
                .Where(m => m.MaintenanceDate >= fromDate && 
                       m.MaintenanceDate <= toDate &&
                       m.Status == MaintenanceStatus.Scheduled)
                .OrderBy(m => m.MaintenanceDate)
                .ToListAsync();
        }

        public async Task<AssetMaintenance> AddMaintenanceRecordAsync(AssetMaintenance maintenance)
        {
            await _dbContext.AssetMaintenances.AddAsync(maintenance);
            await _dbContext.SaveChangesAsync();
            return maintenance;
        }

        public async Task<AssetMaintenance> UpdateMaintenanceRecordAsync(AssetMaintenance maintenance)
        {
            _dbContext.AssetMaintenances.Update(maintenance);
            await _dbContext.SaveChangesAsync();
            return maintenance;
        }

        public async Task<bool> DeleteMaintenanceRecordAsync(Guid id)
        {
            var maintenance = await _dbContext.AssetMaintenances.FindAsync(id.ToString());
            if (maintenance == null)
                return false;
                
            _dbContext.AssetMaintenances.Remove(maintenance);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<string> GenerateMaintenanceNumberAsync(Guid tenantId, DateTime date)
        {
            var prefix = $"MAINT-{date:yyyyMM}-";
            
            var lastMaintenance = await _dbContext.AssetMaintenances
                .Where(m => m.MaintenanceNumber.StartsWith(prefix))
                .OrderByDescending(m => m.MaintenanceNumber)
                .FirstOrDefaultAsync();
            
            int sequence = 1;
            if (lastMaintenance != null && int.TryParse(lastMaintenance.MaintenanceNumber.Substring(prefix.Length), out int lastSequence))
            {
                sequence = lastSequence + 1;
            }
            
            return $"{prefix}{sequence:D4}";
        }

        #endregion

        #region Asset Transfer Operations

        public async Task<AssetTransfer> GetAssetTransferByIdAsync(Guid id)
        {
            return await _dbContext.AssetTransfers
                .Include(t => t.Asset)
                .FirstOrDefaultAsync(t => t.Id == id.ToString());
        }

        public async Task<IEnumerable<AssetTransfer>> GetTransferHistoryForAssetAsync(Guid assetId)
        {
            return await _dbContext.AssetTransfers
                .Where(t => t.AssetId == assetId)
                .OrderByDescending(t => t.TransferDate)
                .ToListAsync();
        }

        public async Task<AssetTransfer> AddAssetTransferAsync(AssetTransfer transfer)
        {
            await _dbContext.AssetTransfers.AddAsync(transfer);
            await _dbContext.SaveChangesAsync();
            return transfer;
        }

        public async Task<AssetTransfer> UpdateAssetTransferAsync(AssetTransfer transfer)
        {
            _dbContext.AssetTransfers.Update(transfer);
            await _dbContext.SaveChangesAsync();
            return transfer;
        }

        public async Task<string> GenerateTransferNumberAsync(Guid tenantId, DateTime date)
        {
            var prefix = $"TRF-{date:yyyyMM}-";
            
            var lastTransfer = await _dbContext.AssetTransfers
                .Where(t => t.TransferNumber.StartsWith(prefix))
                .OrderByDescending(t => t.TransferNumber)
                .FirstOrDefaultAsync();
            
            int sequence = 1;
            if (lastTransfer != null && int.TryParse(lastTransfer.TransferNumber.Substring(prefix.Length), out int lastSequence))
            {
                sequence = lastSequence + 1;
            }
            
            return $"{prefix}{sequence:D4}";
        }

        #endregion

        #region Asset Inventory Count Operations

        public async Task<AssetInventoryCount> GetInventoryCountByIdAsync(Guid id)
        {
            return await _dbContext.AssetInventoryCounts
                .Include(i => i.CountItems)
                .ThenInclude(ci => ci.Asset)
                .FirstOrDefaultAsync(i => i.Id == id.ToString());
        }

        public async Task<IEnumerable<AssetInventoryCount>> GetInventoryCountsByStatusAsync(InventoryCountStatus status)
        {
            return await _dbContext.AssetInventoryCounts
                .Where(i => i.Status == status)
                .OrderByDescending(i => i.CountDate)
                .ToListAsync();
        }

        public async Task<AssetInventoryCount> AddInventoryCountAsync(AssetInventoryCount inventoryCount)
        {
            await _dbContext.AssetInventoryCounts.AddAsync(inventoryCount);
            await _dbContext.SaveChangesAsync();
            return inventoryCount;
        }

        public async Task<AssetInventoryCount> UpdateInventoryCountAsync(AssetInventoryCount inventoryCount)
        {
            _dbContext.AssetInventoryCounts.Update(inventoryCount);
            await _dbContext.SaveChangesAsync();
            return inventoryCount;
        }

        public async Task<AssetInventoryCountItem> AddInventoryCountItemAsync(AssetInventoryCountItem countItem)
        {
            await _dbContext.AssetInventoryCountItems.AddAsync(countItem);
            await _dbContext.SaveChangesAsync();
            return countItem;
        }

        public async Task<IEnumerable<AssetInventoryCountItem>> GetDiscrepanciesForInventoryCountAsync(Guid inventoryCountId)
        {
            return await _dbContext.AssetInventoryCountItems
                .Include(i => i.Asset)
                .Where(i => i.InventoryCountId == inventoryCountId && !i.WasFound)
                .ToListAsync();
        }

        public async Task<string> GenerateInventoryCountNumberAsync(Guid tenantId, DateTime date)
        {
            var prefix = $"INV-{date:yyyyMM}-";
            
            var lastInventory = await _dbContext.AssetInventoryCounts
                .Where(i => i.InventoryCountNumber.StartsWith(prefix))
                .OrderByDescending(i => i.InventoryCountNumber)
                .FirstOrDefaultAsync();
            
            int sequence = 1;
            if (lastInventory != null && int.TryParse(lastInventory.InventoryCountNumber.Substring(prefix.Length), out int lastSequence))
            {
                sequence = lastSequence + 1;
            }
            
            return $"{prefix}{sequence:D4}";
        }

        #endregion

        #region Asset Disposal Operations

        public async Task<AssetDisposal> GetAssetDisposalByIdAsync(Guid id)
        {
            return await _dbContext.AssetDisposals
                .Include(d => d.Asset)
                .FirstOrDefaultAsync(d => d.Id == id.ToString());
        }

        public async Task<IEnumerable<AssetDisposal>> GetAssetDisposalsByStatusAsync(DisposalStatus status)
        {
            return await _dbContext.AssetDisposals
                .Include(d => d.Asset)
                .Where(d => d.Status == status)
                .OrderByDescending(d => d.DisposalDate)
                .ToListAsync();
        }

        public async Task<AssetDisposal> AddAssetDisposalAsync(AssetDisposal disposal)
        {
            await _dbContext.AssetDisposals.AddAsync(disposal);
            await _dbContext.SaveChangesAsync();
            return disposal;
        }

        public async Task<AssetDisposal> UpdateAssetDisposalAsync(AssetDisposal disposal)
        {
            _dbContext.AssetDisposals.Update(disposal);
            await _dbContext.SaveChangesAsync();
            return disposal;
        }

        public async Task<string> GenerateDisposalNumberAsync(Guid tenantId, DateTime date)
        {
            var prefix = $"DISP-{date:yyyyMM}-";
            
            var lastDisposal = await _dbContext.AssetDisposals
                .Where(d => d.DisposalNumber.StartsWith(prefix))
                .OrderByDescending(d => d.DisposalNumber)
                .FirstOrDefaultAsync();
            
            int sequence = 1;
            if (lastDisposal != null && int.TryParse(lastDisposal.DisposalNumber.Substring(prefix.Length), out int lastSequence))
            {
                sequence = lastSequence + 1;
            }
            
            return $"{prefix}{sequence:D4}";
        }

        #endregion

        #region Asset Revaluation Operations

        public async Task<AssetRevaluation> GetAssetRevaluationByIdAsync(Guid id)
        {
            return await _dbContext.AssetRevaluations
                .Include(r => r.Asset)
                .FirstOrDefaultAsync(r => r.Id == id.ToString());
        }

        public async Task<IEnumerable<AssetRevaluation>> GetRevaluationHistoryForAssetAsync(Guid assetId)
        {
            return await _dbContext.AssetRevaluations
                .Where(r => r.AssetId == assetId)
                .OrderByDescending(r => r.RevaluationDate)
                .ToListAsync();
        }

        public async Task<AssetRevaluation> AddAssetRevaluationAsync(AssetRevaluation revaluation)
        {
            await _dbContext.AssetRevaluations.AddAsync(revaluation);
            await _dbContext.SaveChangesAsync();
            return revaluation;
        }

        public async Task<AssetRevaluation> UpdateAssetRevaluationAsync(AssetRevaluation revaluation)
        {
            _dbContext.AssetRevaluations.Update(revaluation);
            await _dbContext.SaveChangesAsync();
            return revaluation;
        }

        #endregion
    }
}
