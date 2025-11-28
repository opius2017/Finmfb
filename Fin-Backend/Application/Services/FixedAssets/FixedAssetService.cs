using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FinTech.Core.Application.Common.Interfaces;
using FinTech.Core.Application.Interfaces.Services;
using FinTech.Core.Application.Services;
using FinTech.Core.Domain.Entities.FixedAssets;
using FinTech.Core.Domain.Entities.GeneralLedger;
using FinTech.Core.Domain.Enums.FixedAssets;

namespace FinTech.Core.Application.Services.Implementation
{
    /// <summary>
    /// Implementation of the Fixed Asset Management Service
    /// </summary>
    public class FixedAssetService : IFixedAssetService
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IDateTimeService _dateTimeService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IGeneralLedgerService _glService;

        public FixedAssetService(
            IApplicationDbContext dbContext,
            IDateTimeService dateTimeService,
            ICurrentUserService currentUserService,
            IGeneralLedgerService glService)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _glService = glService ?? throw new ArgumentNullException(nameof(glService));
        }

        #region Asset Management

        /// <summary>
        /// Creates a new asset in the system
        /// </summary>
        public async Task<Asset> CreateAssetAsync(Asset asset)
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));

            // Validate asset category exists
            var category = await _dbContext.AssetCategories.FindAsync(asset.AssetCategoryId);
            if (category == null)
                throw new InvalidOperationException($"Asset category with ID {asset.AssetCategoryId} not found");

            // Set initial values
            asset.Status = AssetStatus.Active;
            asset.CurrentValue = asset.AcquisitionCost;
            asset.BookValue = asset.AcquisitionCost;
            asset.AccumulatedDepreciation = 0;
            asset.CreatedAt = _dateTimeService.Now;
            asset.CreatedById = _currentUserService.UserId;

            // Add asset to database
            await _dbContext.Assets.AddAsync(asset);
            await _dbContext.SaveChangesAsync();

            // Generate depreciation schedule if applicable
            if (asset.IsDepreciable && asset.DepreciationMethod != DepreciationMethod.None)
            {
                await GenerateDepreciationScheduleAsync(asset.Id);
            }

            return asset;
        }

        /// <summary>
        /// Retrieves an asset by its ID
        /// </summary>
        public async Task<Asset> GetAssetByIdAsync(Guid id)
        {
            return await _dbContext.Assets
                .Include(a => a.AssetCategory)
                .Include(a => a.DepreciationSchedules)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        /// <summary>
        /// Retrieves an asset by its asset number
        /// </summary>
        public async Task<Asset> GetAssetByNumberAsync(string assetNumber)
        {
            if (string.IsNullOrEmpty(assetNumber))
                throw new ArgumentException("Asset number cannot be null or empty", nameof(assetNumber));

            return await _dbContext.Assets
                .Include(a => a.AssetCategory)
                .Include(a => a.DepreciationSchedules)
                .FirstOrDefaultAsync(a => a.AssetNumber == assetNumber);
        }

        /// <summary>
        /// Retrieves assets by status
        /// </summary>
        public async Task<IEnumerable<Asset>> GetAssetsByStatusAsync(AssetStatus status)
        {
            return await _dbContext.Assets
                .Include(a => a.AssetCategory)
                .Where(a => a.Status == status)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves assets by category
        /// </summary>
        public async Task<IEnumerable<Asset>> GetAssetsByCategoryAsync(Guid categoryId)
        {
            return await _dbContext.Assets
                .Include(a => a.AssetCategory)
                .Where(a => a.AssetCategoryId == categoryId)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves assets by department
        /// </summary>
        public async Task<IEnumerable<Asset>> GetAssetsByDepartmentAsync(string department)
        {
            if (string.IsNullOrEmpty(department))
                throw new ArgumentException("Department cannot be null or empty", nameof(department));

            return await _dbContext.Assets
                .Include(a => a.AssetCategory)
                .Where(a => a.Department == department)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves assets by location
        /// </summary>
        public async Task<IEnumerable<Asset>> GetAssetsByLocationAsync(string location)
        {
            if (string.IsNullOrEmpty(location))
                throw new ArgumentException("Location cannot be null or empty", nameof(location));

            return await _dbContext.Assets
                .Include(a => a.AssetCategory)
                .Where(a => a.Location == location)
                .ToListAsync();
        }

        /// <summary>
        /// Updates an asset
        /// </summary>
        public async Task<Asset> UpdateAssetAsync(Asset asset)
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));

            var existingAsset = await _dbContext.Assets.FindAsync(asset.Id);
            if (existingAsset == null)
                throw new InvalidOperationException($"Asset with ID {asset.Id} not found");

            // Update properties
            existingAsset.AssetName = asset.AssetName;
            existingAsset.Description = asset.Description;
            existingAsset.Location = asset.Location;
            existingAsset.Department = asset.Department;
            existingAsset.CustodianId = asset.CustodianId;
            existingAsset.AssetTag = asset.AssetTag;
            existingAsset.SerialNumber = asset.SerialNumber;
            existingAsset.Notes = asset.Notes;
            existingAsset.WarrantyExpiryDate = asset.WarrantyExpiryDate;
            existingAsset.UpdatedAt = _dateTimeService.Now;
            existingAsset.UpdatedById = _currentUserService.UserId;

            // Don't update critical financial fields directly
            // These should be handled by specific operations like revaluations

            await _dbContext.SaveChangesAsync();
            return existingAsset;
        }

        /// <summary>
        /// Deletes an asset if it's in a deletable state
        /// </summary>
        public async Task<bool> DeleteAssetAsync(Guid id)
        {
            var asset = await _dbContext.Assets.FindAsync(id);
            if (asset == null)
                return false;

            // Only allow deletion of assets with no transactions
            var hasDepreciationSchedule = await _dbContext.AssetDepreciationSchedules
                .AnyAsync(s => s.AssetId == id);
            var hasMaintenanceRecords = await _dbContext.AssetMaintenances
                .AnyAsync(m => m.AssetId == id);
            var hasTransfers = await _dbContext.AssetTransfers
                .AnyAsync(t => t.AssetId == id);
            var hasRevaluations = await _dbContext.AssetRevaluations
                .AnyAsync(r => r.AssetId == id);
            var hasDisposals = await _dbContext.AssetDisposals
                .AnyAsync(d => d.AssetId == id);

            if (hasDepreciationSchedule || hasMaintenanceRecords || hasTransfers || 
                hasRevaluations || hasDisposals)
            {
                throw new InvalidOperationException("Cannot delete asset with associated transactions");
            }

            _dbContext.Assets.Remove(asset);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Asset Category Management

        /// <summary>
        /// Creates a new asset category
        /// </summary>
        public async Task<AssetCategory> CreateAssetCategoryAsync(AssetCategory category)
        {
            if (category == null) throw new ArgumentNullException(nameof(category));

            // Check if parent category exists if specified
            if (category.ParentCategoryId.HasValue)
            {
                var parentExists = await _dbContext.AssetCategories
                    .AnyAsync(c => c.Id == category.ParentCategoryId.Value);
                if (!parentExists)
                    throw new InvalidOperationException($"Parent category with ID {category.ParentCategoryId.Value} not found");
            }

            // Check for duplicate category code
            var categoryCodeExists = await _dbContext.AssetCategories
                .AnyAsync(c => c.CategoryCode == category.CategoryCode && c.TenantId == category.TenantId);
            if (categoryCodeExists)
                throw new InvalidOperationException($"Category with code {category.CategoryCode} already exists");

            // Set audit fields
            category.CreatedAt = _dateTimeService.Now;
            category.CreatedById = _currentUserService.UserId;

            await _dbContext.AssetCategories.AddAsync(category);
            await _dbContext.SaveChangesAsync();
            return category;
        }

        /// <summary>
        /// Retrieves an asset category by ID
        /// </summary>
        public async Task<AssetCategory> GetAssetCategoryByIdAsync(Guid id)
        {
            return await _dbContext.AssetCategories
                .Include(c => c.ParentCategory)
                .Include(c => c.ChildCategories)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        /// <summary>
        /// Retrieves an asset category by code
        /// </summary>
        public async Task<AssetCategory> GetAssetCategoryByCodeAsync(string categoryCode)
        {
            if (string.IsNullOrEmpty(categoryCode))
                throw new ArgumentException("Category code cannot be null or empty", nameof(categoryCode));

            var tenantId = await _currentUserService.GetCurrentTenantId();
            return await _dbContext.AssetCategories
                .Include(c => c.ParentCategory)
                .Include(c => c.ChildCategories)
                .FirstOrDefaultAsync(c => c.CategoryCode == categoryCode && c.TenantId == tenantId);
        }

        /// <summary>
        /// Retrieves all asset categories
        /// </summary>
        public async Task<IEnumerable<AssetCategory>> GetAllAssetCategoriesAsync()
        {
            var tenantId = await _currentUserService.GetCurrentTenantId();
            return await _dbContext.AssetCategories
                .Where(c => c.TenantId == tenantId)
                .Include(c => c.ParentCategory)
                .ToListAsync();
        }

        /// <summary>
        /// Updates an asset category
        /// </summary>
        public async Task<AssetCategory> UpdateAssetCategoryAsync(AssetCategory category)
        {
            if (category == null) throw new ArgumentNullException(nameof(category));

            var existingCategory = await _dbContext.AssetCategories.FindAsync(category.Id);
            if (existingCategory == null)
                throw new InvalidOperationException($"Category with ID {category.Id} not found");

            // Check for circular references in parent-child relationship
            if (category.ParentCategoryId.HasValue && category.ParentCategoryId.Value == category.Id)
                throw new InvalidOperationException("A category cannot be its own parent");

            // Check if new parent exists
            if (category.ParentCategoryId.HasValue)
            {
                var parentExists = await _dbContext.AssetCategories
                    .AnyAsync(c => c.Id == category.ParentCategoryId.Value);
                if (!parentExists)
                    throw new InvalidOperationException($"Parent category with ID {category.ParentCategoryId.Value} not found");
            }

            // Update fields
            existingCategory.CategoryName = category.CategoryName;
            existingCategory.Description = category.Description;
            existingCategory.ParentCategoryId = category.ParentCategoryId;
            existingCategory.DefaultDepreciationMethod = category.DefaultDepreciationMethod;
            existingCategory.DefaultUsefulLifeYears = category.DefaultUsefulLifeYears;
            existingCategory.DefaultSalvageValuePercent = category.DefaultSalvageValuePercent;
            existingCategory.AssetAccountId = category.AssetAccountId;
            existingCategory.DepreciationExpenseAccountId = category.DepreciationExpenseAccountId;
            existingCategory.AccumulatedDepreciationAccountId = category.AccumulatedDepreciationAccountId;
            existingCategory.UpdatedAt = _dateTimeService.Now;
            existingCategory.UpdatedById = _currentUserService.UserId;

            // Don't change category code or tenant ID
            await _dbContext.SaveChangesAsync();
            return existingCategory;
        }

        /// <summary>
        /// Deletes an asset category if not in use
        /// </summary>
        public async Task<bool> DeleteAssetCategoryAsync(Guid id)
        {
            var category = await _dbContext.AssetCategories.FindAsync(id);
            if (category == null)
                return false;

            // Check if category has assets
            var hasAssets = await _dbContext.Assets
                .AnyAsync(a => a.AssetCategoryId == id);
            if (hasAssets)
                throw new InvalidOperationException("Cannot delete category with associated assets");

            // Check if category has child categories
            var hasChildren = await _dbContext.AssetCategories
                .AnyAsync(c => c.ParentCategoryId == id);
            if (hasChildren)
                throw new InvalidOperationException("Cannot delete category with child categories");

            _dbContext.AssetCategories.Remove(category);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Depreciation Management

        /// <summary>
        /// Generates depreciation schedule for an asset
        /// </summary>
        public async Task<IEnumerable<AssetDepreciationSchedule>> GenerateDepreciationScheduleAsync(Guid assetId)
        {
            var asset = await _dbContext.Assets
                .Include(a => a.DepreciationSchedules)
                .FirstOrDefaultAsync(a => a.Id == assetId);

            if (asset == null)
                throw new InvalidOperationException($"Asset with ID {assetId} not found");

            if (!asset.IsDepreciable || asset.DepreciationMethod == DepreciationMethod.None)
                return new List<AssetDepreciationSchedule>();

            // Delete existing depreciation schedule if any
            if (asset.DepreciationSchedules != null && asset.DepreciationSchedules.Any())
            {
                var existingSchedules = asset.DepreciationSchedules.ToList();
                var anyPosted = existingSchedules.Any(s => s.IsPosted);
                
                if (anyPosted)
                    throw new InvalidOperationException("Cannot regenerate depreciation schedule after some periods have been posted");
                
                _dbContext.AssetDepreciationSchedules.RemoveRange(existingSchedules);
                await _dbContext.SaveChangesAsync();
            }

            // Calculate depreciation parameters
            int usefulLifeMonths = asset.UsefulLifeYears * 12;
            decimal salvageValue = asset.AcquisitionCost * (asset.SalvageValuePercent / 100m);
            decimal depreciableAmount = asset.AcquisitionCost - salvageValue;
            
            var schedule = new List<AssetDepreciationSchedule>();
            var startDate = asset.AcquisitionDate;
            
            // For straight-line depreciation
            if (asset.DepreciationMethod == DepreciationMethod.StraightLine)
            {
                decimal monthlyDepreciation = depreciableAmount / usefulLifeMonths;
                
                for (int i = 1; i <= usefulLifeMonths; i++)
                {
                    var periodStartDate = startDate.AddMonths(i - 1);
                    var periodEndDate = startDate.AddMonths(i).AddDays(-1);
                    
                    var schedule_item = new AssetDepreciationSchedule
                    {
                        AssetId = asset.Id,
                        PeriodNumber = i,
                        PeriodStartDate = periodStartDate,
                        PeriodEndDate = periodEndDate,
                        DepreciationAmount = monthlyDepreciation,
                        BookValueBeforeDepreciation = asset.AcquisitionCost - ((i - 1) * monthlyDepreciation),
                        BookValueAfterDepreciation = asset.AcquisitionCost - (i * monthlyDepreciation),
                        IsPosted = false,
                        CreatedAt = _dateTimeService.Now,
                        CreatedById = _currentUserService.UserId
                    };
                    
                    schedule.Add(schedule_item);
                }
            }
            // For declining balance depreciation
            else if (asset.DepreciationMethod == DepreciationMethod.DecliningBalance)
            {
                decimal rate = 2m / usefulLifeMonths; // Double declining rate
                decimal remainingValue = asset.AcquisitionCost;
                
                for (int i = 1; i <= usefulLifeMonths && remainingValue > salvageValue; i++)
                {
                    var periodStartDate = startDate.AddMonths(i - 1);
                    var periodEndDate = startDate.AddMonths(i).AddDays(-1);
                    
                    decimal depreciation = remainingValue * rate;
                    
                    // Ensure we don't depreciate below salvage value
                    if (remainingValue - depreciation < salvageValue)
                    {
                        depreciation = remainingValue - salvageValue;
                    }
                    
                    var bookValueBefore = remainingValue;
                    remainingValue -= depreciation;
                    
                    var schedule_item = new AssetDepreciationSchedule
                    {
                        AssetId = asset.Id,
                        PeriodNumber = i,
                        PeriodStartDate = periodStartDate,
                        PeriodEndDate = periodEndDate,
                        DepreciationAmount = depreciation,
                        BookValueBeforeDepreciation = bookValueBefore,
                        BookValueAfterDepreciation = remainingValue,
                        IsPosted = false,
                        CreatedAt = _dateTimeService.Now,
                        CreatedById = _currentUserService.UserId
                    };
                    
                    schedule.Add(schedule_item);
                    
                    if (remainingValue <= salvageValue)
                        break;
                }
            }
            // For sum-of-years-digits
            else if (asset.DepreciationMethod == DepreciationMethod.SumOfYearsDigits)
            {
                // Calculate sum of years digits
                int sumOfYears = 0;
                for (int i = 1; i <= asset.UsefulLifeYears; i++)
                {
                    sumOfYears += i;
                }
                
                decimal remainingValue = asset.AcquisitionCost;
                int currentYear = 1;
                int monthsInCurrentYear = 0;
                
                for (int i = 1; i <= usefulLifeMonths; i++)
                {
                    monthsInCurrentYear++;
                    if (monthsInCurrentYear > 12 || i == 1)
                    {
                        monthsInCurrentYear = 1;
                        currentYear = (i - 1) / 12 + 1;
                    }
                    
                    var periodStartDate = startDate.AddMonths(i - 1);
                    var periodEndDate = startDate.AddMonths(i).AddDays(-1);
                    
                    // Calculate the fraction for this year
                    int yearsRemaining = asset.UsefulLifeYears - currentYear + 1;
                    decimal yearFraction = (decimal)yearsRemaining / sumOfYears;
                    
                    // Calculate monthly depreciation
                    decimal yearlyDepreciation = depreciableAmount * yearFraction;
                    decimal monthlyDepreciation = yearlyDepreciation / 12;
                    
                    var bookValueBefore = remainingValue;
                    remainingValue -= monthlyDepreciation;
                    
                    // Ensure we don't depreciate below salvage value
                    if (remainingValue < salvageValue)
                    {
                        monthlyDepreciation = bookValueBefore - salvageValue;
                        remainingValue = salvageValue;
                    }
                    
                    var schedule_item = new AssetDepreciationSchedule
                    {
                        AssetId = asset.Id,
                        PeriodNumber = i,
                        PeriodStartDate = periodStartDate,
                        PeriodEndDate = periodEndDate,
                        DepreciationAmount = monthlyDepreciation,
                        BookValueBeforeDepreciation = bookValueBefore,
                        BookValueAfterDepreciation = remainingValue,
                        IsPosted = false,
                        CreatedAt = _dateTimeService.Now,
                        CreatedById = _currentUserService.UserId
                    };
                    
                    schedule.Add(schedule_item);
                    
                    if (remainingValue <= salvageValue)
                        break;
                }
            }
            
            // Save the schedule
            await _dbContext.AssetDepreciationSchedules.AddRangeAsync(schedule);
            await _dbContext.SaveChangesAsync();
            
            return schedule;
        }

        /// <summary>
        /// Retrieves depreciation schedule for an asset
        /// </summary>
        public async Task<IEnumerable<AssetDepreciationSchedule>> GetDepreciationScheduleForAssetAsync(Guid assetId)
        {
            return await _dbContext.AssetDepreciationSchedules
                .Where(s => s.AssetId == assetId)
                .OrderBy(s => s.PeriodNumber)
                .ToListAsync();
        }

        /// <summary>
        /// Calculates monthly depreciation for an asset
        /// </summary>
        public async Task<decimal> CalculateMonthlyDepreciationAsync(Guid assetId)
        {
            var now = _dateTimeService.Now;
            var schedule = await _dbContext.AssetDepreciationSchedules
                .Where(s => s.AssetId == assetId && 
                       s.PeriodStartDate <= now && 
                       s.PeriodEndDate >= now)
                .FirstOrDefaultAsync();
            
            return schedule?.DepreciationAmount ?? 0;
        }

        /// <summary>
        /// Processes monthly depreciation for all assets
        /// </summary>
        public async Task<bool> ProcessMonthlyDepreciationAsync(DateTime asOfDate)
        {
            var schedules = await _dbContext.AssetDepreciationSchedules
                .Include(s => s.Asset)
                .Where(s => 
                    s.PeriodStartDate <= asOfDate && 
                    s.PeriodEndDate >= asOfDate &&
                    !s.IsPosted &&
                    s.Asset.Status == AssetStatus.Active)
                .ToListAsync();
            
            if (!schedules.Any())
                return true; // No schedules to process
            
            foreach (var schedule in schedules)
            {
                // Update asset book value and accumulated depreciation
                var asset = schedule.Asset;
                asset.BookValue = schedule.BookValueAfterDepreciation;
                asset.AccumulatedDepreciation += schedule.DepreciationAmount;
                asset.UpdatedAt = _dateTimeService.Now;
                asset.UpdatedById = _currentUserService.UserId;
                
                // Mark schedule as posted
                schedule.IsPosted = true;
                schedule.PostedDate = _dateTimeService.Now;
                schedule.PostedById = _currentUserService.UserId;
            }
            
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Posts depreciation to the general ledger
        /// </summary>
        public async Task<bool> PostDepreciationToGLAsync(DateTime periodStart, DateTime periodEnd)
        {
            var schedules = await _dbContext.AssetDepreciationSchedules
                .Include(s => s.Asset)
                .ThenInclude(a => a.AssetCategory)
                .Where(s => 
                    s.PeriodStartDate >= periodStart && 
                    s.PeriodEndDate <= periodEnd &&
                    s.IsPosted &&
                    !s.PostedToGL)
                .ToListAsync();
            
            if (!schedules.Any())
                return true; // No schedules to post
            
            // Group by account for summarized posting
            var accountGroups = schedules
                .GroupBy(s => new { 
                    ExpenseAccount = s.Asset.AssetCategory.DepreciationExpenseAccountId,
                    AccumulatedAccount = s.Asset.AssetCategory.AccumulatedDepreciationAccountId
                })
                .ToList();
            
            foreach (var group in accountGroups)
            {
                decimal totalDepreciation = group.Sum(s => s.DepreciationAmount);
                
                // Create journal entry
                var journalEntry = new JournalEntry
                {
                    JournalNumber = await _glService.GenerateJournalNumberAsync(),
                    TransactionDate = _dateTimeService.Now,
                    Description = $"Monthly Depreciation {periodStart:MMM yyyy}",
                    Status = Domain.Enums.GeneralLedger.JournalEntryStatus.Posted,
                    TotalDebit = totalDepreciation,
                    TotalCredit = totalDepreciation,
                    CreatedAt = _dateTimeService.Now,
                    CreatedById = _currentUserService.UserId,
                    Source = "Fixed Assets"
                };
                
                // Add journal details
                var details = new List<JournalEntryDetail>
                {
                    // Debit Depreciation Expense
                    new JournalEntryDetail
                    {
                        AccountId = group.Key.ExpenseAccount,
                        Description = "Depreciation Expense",
                        DebitAmount = totalDepreciation,
                        CreditAmount = 0,
                        CreatedAt = _dateTimeService.Now
                    },
                    // Credit Accumulated Depreciation
                    new JournalEntryDetail
                    {
                        AccountId = group.Key.AccumulatedAccount,
                        Description = "Accumulated Depreciation",
                        DebitAmount = 0,
                        CreditAmount = totalDepreciation,
                        CreatedAt = _dateTimeService.Now
                    }
                };
                
                journalEntry.Details = details;
                await _glService.CreateJournalEntryAsync(journalEntry);
                
                // Mark schedules as posted to GL
                foreach (var schedule in group)
                {
                    schedule.PostedToGL = true;
                    schedule.PostedToGLDate = _dateTimeService.Now;
                }
            }
            
            await _dbContext.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Asset Maintenance Management

        /// <summary>
        /// Creates a new maintenance record
        /// </summary>
        public async Task<AssetMaintenance> CreateMaintenanceRecordAsync(AssetMaintenance maintenance)
        {
            if (maintenance == null) throw new ArgumentNullException(nameof(maintenance));
            
            // Validate asset exists
            var assetExists = await _dbContext.Assets.AnyAsync(a => a.Id == maintenance.AssetId);
            if (!assetExists)
                throw new InvalidOperationException($"Asset with ID {maintenance.AssetId} not found");
            
            // Set initial values
            maintenance.MaintenanceNumber = await GenerateMaintenanceNumberAsync();
            maintenance.Status = MaintenanceStatus.Scheduled;
            maintenance.CreatedAt = _dateTimeService.Now;
            maintenance.CreatedById = _currentUserService.UserId;
            
            await _dbContext.AssetMaintenances.AddAsync(maintenance);
            await _dbContext.SaveChangesAsync();
            return maintenance;
        }

        /// <summary>
        /// Retrieves a maintenance record by ID
        /// </summary>
        public async Task<AssetMaintenance> GetMaintenanceRecordByIdAsync(Guid id)
        {
            return await _dbContext.AssetMaintenances
                .Include(m => m.Asset)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        /// <summary>
        /// Retrieves maintenance history for an asset
        /// </summary>
        public async Task<IEnumerable<AssetMaintenance>> GetMaintenanceHistoryForAssetAsync(Guid assetId)
        {
            return await _dbContext.AssetMaintenances
                .Where(m => m.AssetId == assetId)
                .OrderByDescending(m => m.MaintenanceDate)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves scheduled maintenance for a date range
        /// </summary>
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

        /// <summary>
        /// Updates a maintenance record
        /// </summary>
        public async Task<AssetMaintenance> UpdateMaintenanceRecordAsync(AssetMaintenance maintenance)
        {
            if (maintenance == null) throw new ArgumentNullException(nameof(maintenance));
            
            var existingMaintenance = await _dbContext.AssetMaintenances.FindAsync(maintenance.Id);
            if (existingMaintenance == null)
                throw new InvalidOperationException($"Maintenance record with ID {maintenance.Id} not found");
            
            // Don't allow updates to completed or cancelled maintenance
            if (existingMaintenance.Status == MaintenanceStatus.Completed || 
                existingMaintenance.Status == MaintenanceStatus.Cancelled)
                throw new InvalidOperationException("Cannot update completed or cancelled maintenance records");
            
            // Update fields
            existingMaintenance.MaintenanceDate = maintenance.MaintenanceDate;
            existingMaintenance.MaintenanceType = maintenance.MaintenanceType;
            existingMaintenance.Description = maintenance.Description;
            existingMaintenance.VendorId = maintenance.VendorId;
            existingMaintenance.Cost = maintenance.Cost;
            existingMaintenance.Notes = maintenance.Notes;
            existingMaintenance.Status = maintenance.Status;
            existingMaintenance.UpdatedAt = _dateTimeService.Now;
            existingMaintenance.UpdatedById = _currentUserService.UserId;
            
            // Additional logic for completed maintenance
            if (maintenance.Status == MaintenanceStatus.Completed && 
                existingMaintenance.Status != MaintenanceStatus.Completed)
            {
                existingMaintenance.CompletionDate = _dateTimeService.Now;
                existingMaintenance.CompletedById = _currentUserService.UserId;
                
                // Update asset's last maintenance date
                var asset = await _dbContext.Assets.FindAsync(existingMaintenance.AssetId);
                if (asset != null)
                {
                    asset.LastMaintenanceDate = _dateTimeService.Now;
                    asset.UpdatedAt = _dateTimeService.Now;
                    asset.UpdatedById = _currentUserService.UserId;
                }
            }
            
            await _dbContext.SaveChangesAsync();
            return existingMaintenance;
        }

        /// <summary>
        /// Deletes a maintenance record if not completed
        /// </summary>
        public async Task<bool> DeleteMaintenanceRecordAsync(Guid id)
        {
            var maintenance = await _dbContext.AssetMaintenances.FindAsync(id);
            if (maintenance == null)
                return false;
            
            // Only allow deletion of scheduled or in-progress maintenance
            if (maintenance.Status == MaintenanceStatus.Completed)
                throw new InvalidOperationException("Cannot delete completed maintenance records");
            
            _dbContext.AssetMaintenances.Remove(maintenance);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Generates a unique maintenance number
        /// </summary>
        private async Task<string> GenerateMaintenanceNumberAsync()
        {
            var tenantId = await _currentUserService.GetCurrentTenantId();
            var date = _dateTimeService.Now;
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

        // Additional implementation sections for Asset Transfer, Inventory Count,
        // Asset Disposal, Asset Revaluation, and Reporting would follow the same pattern
        // but are omitted here for brevity. These would be implemented following the
        // interface definitions in IFixedAssetService.
        #region Stubs - Interface completeness

        // Asset Transfer Management - stubs to satisfy interface
        public Task<AssetTransfer> CreateAssetTransferAsync(AssetTransfer transfer)
        {
            throw new NotImplementedException();
        }

        public Task<AssetTransfer> GetAssetTransferByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<AssetTransfer>> GetTransferHistoryForAssetAsync(Guid assetId)
        {
            throw new NotImplementedException();
        }

        public Task<AssetTransfer> UpdateAssetTransferAsync(AssetTransfer transfer)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ApproveAssetTransferAsync(Guid transferId, Guid approverId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CompleteAssetTransferAsync(Guid transferId)
        {
            throw new NotImplementedException();
        }

        // Asset Inventory Management - stubs
        public Task<AssetInventoryCount> CreateInventoryCountAsync(AssetInventoryCount inventoryCount)
        {
            throw new NotImplementedException();
        }

        public Task<AssetInventoryCount> GetInventoryCountByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<AssetInventoryCount>> GetInventoryCountsByStatusAsync(InventoryCountStatus status)
        {
            throw new NotImplementedException();
        }

        public Task<AssetInventoryCount> UpdateInventoryCountAsync(AssetInventoryCount inventoryCount)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CompleteInventoryCountAsync(Guid inventoryCountId)
        {
            throw new NotImplementedException();
        }

        public Task<AssetInventoryCountItem> RecordInventoryCountItemAsync(AssetInventoryCountItem countItem)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<AssetInventoryCountItem>> GetDiscrepanciesForInventoryCountAsync(Guid inventoryCountId)
        {
            throw new NotImplementedException();
        }

        // Asset Disposal Management - stubs
        public Task<AssetDisposal> CreateAssetDisposalAsync(AssetDisposal disposal)
        {
            throw new NotImplementedException();
        }

        public Task<AssetDisposal> GetAssetDisposalByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<AssetDisposal>> GetAssetDisposalsByStatusAsync(DisposalStatus status)
        {
            throw new NotImplementedException();
        }

        public Task<AssetDisposal> UpdateAssetDisposalAsync(AssetDisposal disposal)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ApproveAssetDisposalAsync(Guid disposalId, Guid approverId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CompleteAssetDisposalAsync(Guid disposalId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PostDisposalToGLAsync(Guid disposalId)
        {
            throw new NotImplementedException();
        }

        // Asset Revaluation Management - stubs
        public Task<AssetRevaluation> CreateAssetRevaluationAsync(AssetRevaluation revaluation)
        {
            throw new NotImplementedException();
        }

        public Task<AssetRevaluation> GetAssetRevaluationByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<AssetRevaluation>> GetRevaluationHistoryForAssetAsync(Guid assetId)
        {
            throw new NotImplementedException();
        }

        public Task<AssetRevaluation> UpdateAssetRevaluationAsync(AssetRevaluation revaluation)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PostRevaluationToGLAsync(Guid revaluationId)
        {
            throw new NotImplementedException();
        }

        // Reporting - stubs
        public Task<decimal> GetTotalAssetValueAsync()
        {
            throw new NotImplementedException();
        }

        public Task<decimal> GetTotalDepreciationForPeriodAsync(DateTime periodStart, DateTime periodEnd)
        {
            throw new NotImplementedException();
        }

        public Task<IDictionary<string, decimal>> GetAssetValueByDepartmentAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IDictionary<string, decimal>> GetAssetValueByCategoryAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Asset>> GetFullyDepreciatedAssetsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Asset>> GetAssetsNearingEndOfLifeAsync(int monthsThreshold)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
