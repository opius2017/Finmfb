using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Domain.Entities.Accounting;
using FinTech.Domain.Repositories.Accounting;

namespace FinTech.Application.Services.Accounting
{
    public interface IFixedAssetService
    {
        Task<FixedAssetDto> CreateFixedAssetAsync(
            CreateFixedAssetDto assetDto,
            CancellationToken cancellationToken = default);
            
        Task<FixedAssetDto> GetFixedAssetAsync(
            string assetId,
            CancellationToken cancellationToken = default);
            
        Task<FixedAssetDto> UpdateFixedAssetAsync(
            string assetId,
            UpdateFixedAssetDto assetDto,
            CancellationToken cancellationToken = default);
            
        Task<bool> DeleteFixedAssetAsync(
            string assetId,
            CancellationToken cancellationToken = default);
            
        Task<List<FixedAssetDto>> GetAllFixedAssetsAsync(
            CancellationToken cancellationToken = default);
            
        Task<FixedAssetDto> CalculateDepreciationAsync(
            string assetId,
            DateTime asOfDate,
            CancellationToken cancellationToken = default);
            
        Task<DepreciationScheduleDto> GenerateDepreciationScheduleAsync(
            string assetId,
            CancellationToken cancellationToken = default);
            
        Task<bool> RecordDepreciationAsync(
            string assetId,
            string financialPeriodId,
            CancellationToken cancellationToken = default);
            
        Task<FixedAssetDisposalDto> RecordAssetDisposalAsync(
            string assetId,
            AssetDisposalDto disposalDto,
            CancellationToken cancellationToken = default);
            
        Task<FixedAssetReportDto> GenerateFixedAssetReportAsync(
            string financialPeriodId,
            CancellationToken cancellationToken = default);
    }
    
    public class FixedAssetService : IFixedAssetService
    {
        private readonly IFixedAssetRepository _fixedAssetRepository;
        private readonly IChartOfAccountRepository _chartOfAccountRepository;
        private readonly IFinancialPeriodRepository _financialPeriodRepository;
        private readonly IJournalEntryService _journalEntryService;
        
        public FixedAssetService(
            IFixedAssetRepository fixedAssetRepository,
            IChartOfAccountRepository chartOfAccountRepository,
            IFinancialPeriodRepository financialPeriodRepository,
            IJournalEntryService journalEntryService)
        {
            _fixedAssetRepository = fixedAssetRepository ?? throw new ArgumentNullException(nameof(fixedAssetRepository));
            _chartOfAccountRepository = chartOfAccountRepository ?? throw new ArgumentNullException(nameof(chartOfAccountRepository));
            _financialPeriodRepository = financialPeriodRepository ?? throw new ArgumentNullException(nameof(financialPeriodRepository));
            _journalEntryService = journalEntryService ?? throw new ArgumentNullException(nameof(journalEntryService));
        }
        
        /// <summary>
        /// Creates a new fixed asset
        /// </summary>
        public async Task<FixedAssetDto> CreateFixedAssetAsync(
            CreateFixedAssetDto assetDto,
            CancellationToken cancellationToken = default)
        {
            // Validate input
            if (assetDto == null)
            {
                throw new ArgumentNullException(nameof(assetDto));
            }
            
            // Validate asset account
            var assetAccount = await _chartOfAccountRepository.GetByIdAsync(assetDto.AssetAccountId, cancellationToken);
            if (assetAccount == null)
            {
                throw new InvalidOperationException($"Asset account with ID {assetDto.AssetAccountId} not found");
            }
            
            // Validate accumulated depreciation account
            var accDepreciationAccount = await _chartOfAccountRepository.GetByIdAsync(assetDto.AccumulatedDepreciationAccountId, cancellationToken);
            if (accDepreciationAccount == null)
            {
                throw new InvalidOperationException($"Accumulated depreciation account with ID {assetDto.AccumulatedDepreciationAccountId} not found");
            }
            
            // Validate depreciation expense account
            var depreciationExpenseAccount = await _chartOfAccountRepository.GetByIdAsync(assetDto.DepreciationExpenseAccountId, cancellationToken);
            if (depreciationExpenseAccount == null)
            {
                throw new InvalidOperationException($"Depreciation expense account with ID {assetDto.DepreciationExpenseAccountId} not found");
            }
            
            // Validate disposal gain/loss account if provided
            if (!string.IsNullOrEmpty(assetDto.DisposalGainLossAccountId))
            {
                var disposalAccount = await _chartOfAccountRepository.GetByIdAsync(assetDto.DisposalGainLossAccountId, cancellationToken);
                if (disposalAccount == null)
                {
                    throw new InvalidOperationException($"Disposal gain/loss account with ID {assetDto.DisposalGainLossAccountId} not found");
                }
            }
            
            // Calculate residual value if not provided
            decimal residualValue = assetDto.ResidualValue;
            if (residualValue < 0)
            {
                residualValue = 0; // Ensure residual value is not negative
            }
            
            // Create fixed asset entity
            var fixedAsset = new FixedAsset
            {
                Id = Guid.NewGuid().ToString(),
                AssetNumber = assetDto.AssetNumber ?? GenerateAssetNumber(),
                AssetName = assetDto.AssetName,
                Description = assetDto.Description,
                AssetCategory = assetDto.AssetCategory,
                AssetAccountId = assetDto.AssetAccountId,
                AccumulatedDepreciationAccountId = assetDto.AccumulatedDepreciationAccountId,
                DepreciationExpenseAccountId = assetDto.DepreciationExpenseAccountId,
                DisposalGainLossAccountId = assetDto.DisposalGainLossAccountId,
                PurchaseDate = assetDto.PurchaseDate,
                InServiceDate = assetDto.InServiceDate,
                PurchaseCost = assetDto.PurchaseCost,
                ResidualValue = residualValue,
                DepreciableAmount = assetDto.PurchaseCost - residualValue,
                UsefulLifeYears = assetDto.UsefulLifeYears,
                DepreciationMethod = assetDto.DepreciationMethod,
                CurrentBookValue = assetDto.PurchaseCost,
                AccumulatedDepreciation = 0,
                LastDepreciationDate = null,
                Status = FixedAssetStatus.Active,
                Location = assetDto.Location,
                AssetTag = assetDto.AssetTag,
                SerialNumber = assetDto.SerialNumber,
                Manufacturer = assetDto.Manufacturer,
                Model = assetDto.Model,
                WarrantyExpiryDate = assetDto.WarrantyExpiryDate,
                Notes = assetDto.Notes,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = assetDto.CreatedBy,
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = assetDto.CreatedBy,
                DisposalDate = null,
                DisposalType = null,
                DisposalAmount = null,
                DisposalGainLoss = null
            };
            
            // Save the fixed asset
            await _fixedAssetRepository.AddAsync(fixedAsset, cancellationToken);
            
            // Map to DTO and return
            return MapFixedAssetToDto(fixedAsset);
        }
        
        /// <summary>
        /// Gets a fixed asset by ID
        /// </summary>
        public async Task<FixedAssetDto> GetFixedAssetAsync(
            string assetId,
            CancellationToken cancellationToken = default)
        {
            // Get the fixed asset
            var fixedAsset = await _fixedAssetRepository.GetByIdAsync(assetId, cancellationToken);
            if (fixedAsset == null)
            {
                throw new InvalidOperationException($"Fixed asset with ID {assetId} not found");
            }
            
            // Map to DTO and return
            return MapFixedAssetToDto(fixedAsset);
        }
        
        /// <summary>
        /// Updates an existing fixed asset
        /// </summary>
        public async Task<FixedAssetDto> UpdateFixedAssetAsync(
            string assetId,
            UpdateFixedAssetDto assetDto,
            CancellationToken cancellationToken = default)
        {
            // Validate input
            if (assetDto == null)
            {
                throw new ArgumentNullException(nameof(assetDto));
            }
            
            // Get the fixed asset
            var fixedAsset = await _fixedAssetRepository.GetByIdAsync(assetId, cancellationToken);
            if (fixedAsset == null)
            {
                throw new InvalidOperationException($"Fixed asset with ID {assetId} not found");
            }
            
            // Check if asset can be updated
            if (fixedAsset.Status == FixedAssetStatus.Disposed && !assetDto.ForceUpdate)
            {
                throw new InvalidOperationException("Cannot update a disposed asset without ForceUpdate flag");
            }
            
            // Update properties that can be changed
            fixedAsset.AssetName = assetDto.AssetName ?? fixedAsset.AssetName;
            fixedAsset.Description = assetDto.Description ?? fixedAsset.Description;
            fixedAsset.AssetCategory = assetDto.AssetCategory ?? fixedAsset.AssetCategory;
            fixedAsset.Location = assetDto.Location ?? fixedAsset.Location;
            fixedAsset.Notes = assetDto.Notes ?? fixedAsset.Notes;
            fixedAsset.LastModifiedDate = DateTime.UtcNow;
            fixedAsset.LastModifiedBy = assetDto.LastModifiedBy;
            
            // Update properties that require validation
            if (!string.IsNullOrEmpty(assetDto.AssetAccountId))
            {
                var assetAccount = await _chartOfAccountRepository.GetByIdAsync(assetDto.AssetAccountId, cancellationToken);
                if (assetAccount == null)
                {
                    throw new InvalidOperationException($"Asset account with ID {assetDto.AssetAccountId} not found");
                }
                fixedAsset.AssetAccountId = assetDto.AssetAccountId;
            }
            
            if (!string.IsNullOrEmpty(assetDto.AccumulatedDepreciationAccountId))
            {
                var accDepreciationAccount = await _chartOfAccountRepository.GetByIdAsync(assetDto.AccumulatedDepreciationAccountId, cancellationToken);
                if (accDepreciationAccount == null)
                {
                    throw new InvalidOperationException($"Accumulated depreciation account with ID {assetDto.AccumulatedDepreciationAccountId} not found");
                }
                fixedAsset.AccumulatedDepreciationAccountId = assetDto.AccumulatedDepreciationAccountId;
            }
            
            if (!string.IsNullOrEmpty(assetDto.DepreciationExpenseAccountId))
            {
                var depreciationExpenseAccount = await _chartOfAccountRepository.GetByIdAsync(assetDto.DepreciationExpenseAccountId, cancellationToken);
                if (depreciationExpenseAccount == null)
                {
                    throw new InvalidOperationException($"Depreciation expense account with ID {assetDto.DepreciationExpenseAccountId} not found");
                }
                fixedAsset.DepreciationExpenseAccountId = assetDto.DepreciationExpenseAccountId;
            }
            
            if (!string.IsNullOrEmpty(assetDto.DisposalGainLossAccountId))
            {
                var disposalAccount = await _chartOfAccountRepository.GetByIdAsync(assetDto.DisposalGainLossAccountId, cancellationToken);
                if (disposalAccount == null)
                {
                    throw new InvalidOperationException($"Disposal gain/loss account with ID {assetDto.DisposalGainLossAccountId} not found");
                }
                fixedAsset.DisposalGainLossAccountId = assetDto.DisposalGainLossAccountId;
            }
            
            // Save the updated fixed asset
            await _fixedAssetRepository.UpdateAsync(fixedAsset, cancellationToken);
            
            // Map to DTO and return
            return MapFixedAssetToDto(fixedAsset);
        }
        
        /// <summary>
        /// Deletes a fixed asset
        /// </summary>
        public async Task<bool> DeleteFixedAssetAsync(
            string assetId,
            CancellationToken cancellationToken = default)
        {
            // Get the fixed asset
            var fixedAsset = await _fixedAssetRepository.GetByIdAsync(assetId, cancellationToken);
            if (fixedAsset == null)
            {
                throw new InvalidOperationException($"Fixed asset with ID {assetId} not found");
            }
            
            // Check if asset can be deleted
            if (fixedAsset.Status != FixedAssetStatus.Draft)
            {
                throw new InvalidOperationException("Only assets in Draft status can be deleted");
            }
            
            if (fixedAsset.AccumulatedDepreciation > 0)
            {
                throw new InvalidOperationException("Cannot delete an asset with recorded depreciation");
            }
            
            // Delete the fixed asset
            await _fixedAssetRepository.DeleteAsync(assetId, cancellationToken);
            
            return true;
        }
        
        /// <summary>
        /// Gets all fixed assets
        /// </summary>
        public async Task<List<FixedAssetDto>> GetAllFixedAssetsAsync(
            CancellationToken cancellationToken = default)
        {
            // Get all fixed assets
            var fixedAssets = await _fixedAssetRepository.GetAllAsync(cancellationToken);
            
            // Map to DTOs and return
            return fixedAssets.Select(MapFixedAssetToDto).ToList();
        }
        
        /// <summary>
        /// Calculates depreciation for a fixed asset as of a specific date
        /// </summary>
        public async Task<FixedAssetDto> CalculateDepreciationAsync(
            string assetId,
            DateTime asOfDate,
            CancellationToken cancellationToken = default)
        {
            // Get the fixed asset
            var fixedAsset = await _fixedAssetRepository.GetByIdAsync(assetId, cancellationToken);
            if (fixedAsset == null)
            {
                throw new InvalidOperationException($"Fixed asset with ID {assetId} not found");
            }
            
            // Check if asset is active
            if (fixedAsset.Status != FixedAssetStatus.Active)
            {
                throw new InvalidOperationException($"Cannot calculate depreciation for asset with status {fixedAsset.Status}");
            }
            
            // Calculate depreciation
            var totalDepreciation = CalculateDepreciation(fixedAsset, asOfDate);
            
            // Update the asset with calculated depreciation
            fixedAsset.AccumulatedDepreciation = totalDepreciation;
            fixedAsset.CurrentBookValue = fixedAsset.PurchaseCost - totalDepreciation;
            
            // Don't persist changes - this is just a calculation
            
            // Map to DTO and return
            return MapFixedAssetToDto(fixedAsset);
        }
        
        /// <summary>
        /// Generates a depreciation schedule for a fixed asset
        /// </summary>
        public async Task<DepreciationScheduleDto> GenerateDepreciationScheduleAsync(
            string assetId,
            CancellationToken cancellationToken = default)
        {
            // Get the fixed asset
            var fixedAsset = await _fixedAssetRepository.GetByIdAsync(assetId, cancellationToken);
            if (fixedAsset == null)
            {
                throw new InvalidOperationException($"Fixed asset with ID {assetId} not found");
            }
            
            // Initialize the result
            var result = new DepreciationScheduleDto
            {
                AssetId = fixedAsset.Id,
                AssetNumber = fixedAsset.AssetNumber,
                AssetName = fixedAsset.AssetName,
                PurchaseCost = fixedAsset.PurchaseCost,
                ResidualValue = fixedAsset.ResidualValue,
                DepreciableAmount = fixedAsset.DepreciableAmount,
                UsefulLifeYears = fixedAsset.UsefulLifeYears,
                DepreciationMethod = fixedAsset.DepreciationMethod,
                StartDate = fixedAsset.InServiceDate,
                EndDate = fixedAsset.InServiceDate.AddYears(fixedAsset.UsefulLifeYears),
                GeneratedAt = DateTime.UtcNow,
                ScheduleItems = new List<DepreciationScheduleItemDto>()
            };
            
            // Calculate depreciation for each period
            var startDate = fixedAsset.InServiceDate;
            var periodEndDate = startDate;
            decimal accumulatedDepreciation = 0;
            decimal remainingValue = fixedAsset.PurchaseCost;
            
            for (int year = 1; year <= fixedAsset.UsefulLifeYears; year++)
            {
                periodEndDate = startDate.AddYears(year);
                
                // Calculate depreciation amount for this period
                decimal depreciationAmount = 0;
                
                switch (fixedAsset.DepreciationMethod)
                {
                    case DepreciationMethod.StraightLine:
                        depreciationAmount = fixedAsset.DepreciableAmount / fixedAsset.UsefulLifeYears;
                        break;
                        
                    case DepreciationMethod.DoubleDecliningBalance:
                        var straightLineRate = 1.0m / fixedAsset.UsefulLifeYears;
                        var doubleRate = straightLineRate * 2;
                        depreciationAmount = remainingValue * doubleRate;
                        
                        // Switch to straight-line if it yields higher depreciation
                        var remainingYears = fixedAsset.UsefulLifeYears - year + 1;
                        var remainingDepreciable = remainingValue - fixedAsset.ResidualValue;
                        var straightLineAmount = remainingDepreciable / remainingYears;
                        
                        if (straightLineAmount > depreciationAmount)
                        {
                            depreciationAmount = straightLineAmount;
                        }
                        
                        // Ensure we don't depreciate below residual value
                        if (remainingValue - depreciationAmount < fixedAsset.ResidualValue)
                        {
                            depreciationAmount = remainingValue - fixedAsset.ResidualValue;
                        }
                        break;
                        
                    case DepreciationMethod.SumOfYearsDigits:
                        var sumOfYears = fixedAsset.UsefulLifeYears * (fixedAsset.UsefulLifeYears + 1) / 2;
                        var factor = (fixedAsset.UsefulLifeYears - year + 1) / (decimal)sumOfYears;
                        depreciationAmount = fixedAsset.DepreciableAmount * factor;
                        break;
                        
                    case DepreciationMethod.Units:
                        // For units of production, this would require tracking actual usage
                        // Using a simplified approach here
                        depreciationAmount = fixedAsset.DepreciableAmount / fixedAsset.UsefulLifeYears;
                        break;
                }
                
                // Round to 2 decimal places
                depreciationAmount = Math.Round(depreciationAmount, 2);
                
                // Update accumulated values
                accumulatedDepreciation += depreciationAmount;
                remainingValue -= depreciationAmount;
                
                // Ensure we don't depreciate below residual value
                if (remainingValue < fixedAsset.ResidualValue)
                {
                    depreciationAmount -= (fixedAsset.ResidualValue - remainingValue);
                    depreciationAmount = Math.Max(0, depreciationAmount);
                    accumulatedDepreciation = fixedAsset.PurchaseCost - fixedAsset.ResidualValue;
                    remainingValue = fixedAsset.ResidualValue;
                }
                
                // Add schedule item
                var scheduleItem = new DepreciationScheduleItemDto
                {
                    Year = year,
                    PeriodStartDate = startDate.AddYears(year - 1),
                    PeriodEndDate = periodEndDate,
                    DepreciationAmount = depreciationAmount,
                    AccumulatedDepreciation = accumulatedDepreciation,
                    BookValueEndOfPeriod = remainingValue
                };
                
                result.ScheduleItems.Add(scheduleItem);
                
                // Stop if we've fully depreciated to residual value
                if (remainingValue <= fixedAsset.ResidualValue)
                {
                    break;
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Records depreciation for a fixed asset for the specified financial period
        /// </summary>
        public async Task<bool> RecordDepreciationAsync(
            string assetId,
            string financialPeriodId,
            CancellationToken cancellationToken = default)
        {
            // Get the fixed asset
            var fixedAsset = await _fixedAssetRepository.GetByIdAsync(assetId, cancellationToken);
            if (fixedAsset == null)
            {
                throw new InvalidOperationException($"Fixed asset with ID {assetId} not found");
            }
            
            // Get the financial period
            var financialPeriod = await _financialPeriodRepository.GetByIdAsync(financialPeriodId, cancellationToken);
            if (financialPeriod == null)
            {
                throw new InvalidOperationException($"Financial period with ID {financialPeriodId} not found");
            }
            
            // Check if asset is active
            if (fixedAsset.Status != FixedAssetStatus.Active)
            {
                throw new InvalidOperationException($"Cannot record depreciation for asset with status {fixedAsset.Status}");
            }
            
            // Check if depreciation has already been recorded for this period
            if (fixedAsset.LastDepreciationDate.HasValue && 
                fixedAsset.LastDepreciationDate.Value >= financialPeriod.EndDate)
            {
                throw new InvalidOperationException($"Depreciation has already been recorded for this period");
            }
            
            // Calculate depreciation amount for the period
            decimal depreciationAmount = CalculateDepreciationForPeriod(fixedAsset, financialPeriod);
            
            // Don't record zero depreciation
            if (depreciationAmount <= 0)
            {
                return false;
            }
            
            // Record the journal entry
            var journalDto = new CreateJournalEntryDto
            {
                FinancialPeriodId = financialPeriodId,
                TransactionDate = financialPeriod.EndDate,
                Reference = $"Depreciation for {fixedAsset.AssetNumber}",
                Description = $"Depreciation expense for {fixedAsset.AssetName} for period ending {financialPeriod.EndDate.ToShortDateString()}",
                Source = "Fixed Assets",
                CreatedBy = "System",
                JournalEntryItems = new List<CreateJournalEntryItemDto>
                {
                    // Debit Depreciation Expense
                    new CreateJournalEntryItemDto
                    {
                        AccountId = fixedAsset.DepreciationExpenseAccountId,
                        Description = $"Depreciation expense for {fixedAsset.AssetName}",
                        DebitAmount = depreciationAmount,
                        CreditAmount = 0
                    },
                    // Credit Accumulated Depreciation
                    new CreateJournalEntryItemDto
                    {
                        AccountId = fixedAsset.AccumulatedDepreciationAccountId,
                        Description = $"Accumulated depreciation for {fixedAsset.AssetName}",
                        DebitAmount = 0,
                        CreditAmount = depreciationAmount
                    }
                }
            };
            
            await _journalEntryService.CreateJournalEntryAsync(journalDto, cancellationToken);
            
            // Update the fixed asset
            fixedAsset.AccumulatedDepreciation += depreciationAmount;
            fixedAsset.CurrentBookValue -= depreciationAmount;
            fixedAsset.LastDepreciationDate = financialPeriod.EndDate;
            
            // Save the updated fixed asset
            await _fixedAssetRepository.UpdateAsync(fixedAsset, cancellationToken);
            
            return true;
        }
        
        /// <summary>
        /// Records the disposal of a fixed asset
        /// </summary>
        public async Task<FixedAssetDisposalDto> RecordAssetDisposalAsync(
            string assetId,
            AssetDisposalDto disposalDto,
            CancellationToken cancellationToken = default)
        {
            // Validate input
            if (disposalDto == null)
            {
                throw new ArgumentNullException(nameof(disposalDto));
            }
            
            // Get the fixed asset
            var fixedAsset = await _fixedAssetRepository.GetByIdAsync(assetId, cancellationToken);
            if (fixedAsset == null)
            {
                throw new InvalidOperationException($"Fixed asset with ID {assetId} not found");
            }
            
            // Get the financial period
            var financialPeriod = await _financialPeriodRepository.GetByIdAsync(disposalDto.FinancialPeriodId, cancellationToken);
            if (financialPeriod == null)
            {
                throw new InvalidOperationException($"Financial period with ID {disposalDto.FinancialPeriodId} not found");
            }
            
            // Check if asset is active
            if (fixedAsset.Status != FixedAssetStatus.Active)
            {
                throw new InvalidOperationException($"Cannot dispose of asset with status {fixedAsset.Status}");
            }
            
            // Check if disposal gain/loss account is set
            if (string.IsNullOrEmpty(fixedAsset.DisposalGainLossAccountId) && 
                string.IsNullOrEmpty(disposalDto.DisposalGainLossAccountId))
            {
                throw new InvalidOperationException("Disposal gain/loss account must be specified");
            }
            
            // Use provided disposal gain/loss account or default to asset's account
            string disposalGainLossAccountId = disposalDto.DisposalGainLossAccountId ?? fixedAsset.DisposalGainLossAccountId;
            
            // Check if cash account is valid
            if (!string.IsNullOrEmpty(disposalDto.CashAccountId))
            {
                var cashAccount = await _chartOfAccountRepository.GetByIdAsync(disposalDto.CashAccountId, cancellationToken);
                if (cashAccount == null)
                {
                    throw new InvalidOperationException($"Cash account with ID {disposalDto.CashAccountId} not found");
                }
            }
            
            // Calculate final depreciation up to disposal date if needed
            if (disposalDto.RecordFinalDepreciation && 
                (!fixedAsset.LastDepreciationDate.HasValue || 
                 fixedAsset.LastDepreciationDate.Value < disposalDto.DisposalDate))
            {
                // Calculate final depreciation
                decimal finalDepreciation = CalculateDepreciationToDate(fixedAsset, disposalDto.DisposalDate);
                
                if (finalDepreciation > 0)
                {
                    // Record final depreciation journal entry
                    var depreciationDto = new CreateJournalEntryDto
                    {
                        FinancialPeriodId = disposalDto.FinancialPeriodId,
                        TransactionDate = disposalDto.DisposalDate,
                        Reference = $"Final Depreciation for {fixedAsset.AssetNumber}",
                        Description = $"Final depreciation expense for {fixedAsset.AssetName} before disposal",
                        Source = "Fixed Assets",
                        CreatedBy = disposalDto.DisposedBy,
                        JournalEntryItems = new List<CreateJournalEntryItemDto>
                        {
                            // Debit Depreciation Expense
                            new CreateJournalEntryItemDto
                            {
                                AccountId = fixedAsset.DepreciationExpenseAccountId,
                                Description = $"Final depreciation expense for {fixedAsset.AssetName}",
                                DebitAmount = finalDepreciation,
                                CreditAmount = 0
                            },
                            // Credit Accumulated Depreciation
                            new CreateJournalEntryItemDto
                            {
                                AccountId = fixedAsset.AccumulatedDepreciationAccountId,
                                Description = $"Final accumulated depreciation for {fixedAsset.AssetName}",
                                DebitAmount = 0,
                                CreditAmount = finalDepreciation
                            }
                        }
                    };
                    
                    await _journalEntryService.CreateJournalEntryAsync(depreciationDto, cancellationToken);
                    
                    // Update accumulated depreciation
                    fixedAsset.AccumulatedDepreciation += finalDepreciation;
                    fixedAsset.CurrentBookValue -= finalDepreciation;
                }
            }
            
            // Calculate gain or loss on disposal
            decimal netBookValue = fixedAsset.CurrentBookValue;
            decimal disposalAmount = disposalDto.DisposalAmount;
            decimal gainLoss = disposalAmount - netBookValue;
            
            // Create disposal journal entry
            var journalItems = new List<CreateJournalEntryItemDto>();
            
            // Cash/Receivable (if applicable)
            if (disposalAmount > 0)
            {
                journalItems.Add(new CreateJournalEntryItemDto
                {
                    AccountId = disposalDto.CashAccountId,
                    Description = $"Proceeds from disposal of {fixedAsset.AssetName}",
                    DebitAmount = disposalAmount,
                    CreditAmount = 0
                });
            }
            
            // Accumulated Depreciation
            if (fixedAsset.AccumulatedDepreciation > 0)
            {
                journalItems.Add(new CreateJournalEntryItemDto
                {
                    AccountId = fixedAsset.AccumulatedDepreciationAccountId,
                    Description = $"Removal of accumulated depreciation for {fixedAsset.AssetName}",
                    DebitAmount = fixedAsset.AccumulatedDepreciation,
                    CreditAmount = 0
                });
            }
            
            // Gain/Loss
            if (gainLoss != 0)
            {
                journalItems.Add(new CreateJournalEntryItemDto
                {
                    AccountId = disposalGainLossAccountId,
                    Description = $"{(gainLoss > 0 ? "Gain" : "Loss")} on disposal of {fixedAsset.AssetName}",
                    DebitAmount = gainLoss < 0 ? Math.Abs(gainLoss) : 0,
                    CreditAmount = gainLoss > 0 ? gainLoss : 0
                });
            }
            
            // Asset
            journalItems.Add(new CreateJournalEntryItemDto
            {
                AccountId = fixedAsset.AssetAccountId,
                Description = $"Removal of asset {fixedAsset.AssetName} due to disposal",
                DebitAmount = 0,
                CreditAmount = fixedAsset.PurchaseCost
            });
            
            // Create the journal entry
            var journalDto = new CreateJournalEntryDto
            {
                FinancialPeriodId = disposalDto.FinancialPeriodId,
                TransactionDate = disposalDto.DisposalDate,
                Reference = $"Disposal of {fixedAsset.AssetNumber}",
                Description = $"Disposal of {fixedAsset.AssetName} via {disposalDto.DisposalType}",
                Source = "Fixed Assets",
                CreatedBy = disposalDto.DisposedBy,
                JournalEntryItems = journalItems
            };
            
            await _journalEntryService.CreateJournalEntryAsync(journalDto, cancellationToken);
            
            // Update the fixed asset
            fixedAsset.Status = FixedAssetStatus.Disposed;
            fixedAsset.DisposalDate = disposalDto.DisposalDate;
            fixedAsset.DisposalType = disposalDto.DisposalType;
            fixedAsset.DisposalAmount = disposalDto.DisposalAmount;
            fixedAsset.DisposalGainLoss = gainLoss;
            fixedAsset.LastModifiedDate = DateTime.UtcNow;
            fixedAsset.LastModifiedBy = disposalDto.DisposedBy;
            
            // Save the updated fixed asset
            await _fixedAssetRepository.UpdateAsync(fixedAsset, cancellationToken);
            
            // Return disposal result
            return new FixedAssetDisposalDto
            {
                AssetId = fixedAsset.Id,
                AssetNumber = fixedAsset.AssetNumber,
                AssetName = fixedAsset.AssetName,
                DisposalDate = disposalDto.DisposalDate,
                DisposalType = disposalDto.DisposalType,
                DisposalAmount = disposalDto.DisposalAmount,
                NetBookValueAtDisposal = netBookValue,
                GainLossOnDisposal = gainLoss,
                DisposedBy = disposalDto.DisposedBy,
                Notes = disposalDto.Notes
            };
        }
        
        /// <summary>
        /// Generates a fixed asset report for a specific financial period
        /// </summary>
        public async Task<FixedAssetReportDto> GenerateFixedAssetReportAsync(
            string financialPeriodId,
            CancellationToken cancellationToken = default)
        {
            // Get the financial period
            var financialPeriod = await _financialPeriodRepository.GetByIdAsync(financialPeriodId, cancellationToken);
            if (financialPeriod == null)
            {
                throw new InvalidOperationException($"Financial period with ID {financialPeriodId} not found");
            }
            
            // Get all fixed assets
            var fixedAssets = await _fixedAssetRepository.GetAllAsync(cancellationToken);
            
            // Initialize the result
            var result = new FixedAssetReportDto
            {
                FinancialPeriodId = financialPeriodId,
                FinancialPeriodName = financialPeriod.Name,
                GeneratedAt = DateTime.UtcNow,
                AssetsByCategory = new Dictionary<string, AssetCategoryReportDto>(),
                AssetDetails = new List<AssetReportDetailDto>(),
                TotalPurchaseCost = 0,
                TotalAccumulatedDepreciation = 0,
                TotalNetBookValue = 0,
                TotalDisposedAssetsCount = 0,
                TotalDisposalProceeds = 0,
                TotalGainLoss = 0
            };
            
            // Process each fixed asset
            foreach (var asset in fixedAssets)
            {
                // Create asset detail
                var assetDetail = new AssetReportDetailDto
                {
                    AssetId = asset.Id,
                    AssetNumber = asset.AssetNumber,
                    AssetName = asset.AssetName,
                    AssetCategory = asset.AssetCategory,
                    PurchaseDate = asset.PurchaseDate,
                    InServiceDate = asset.InServiceDate,
                    PurchaseCost = asset.PurchaseCost,
                    AccumulatedDepreciation = asset.AccumulatedDepreciation,
                    NetBookValue = asset.CurrentBookValue,
                    Status = asset.Status.ToString(),
                    Location = asset.Location,
                    UsefulLifeYears = asset.UsefulLifeYears,
                    DepreciationMethod = asset.DepreciationMethod.ToString()
                };
                
                // Add disposal information if applicable
                if (asset.Status == FixedAssetStatus.Disposed && 
                    asset.DisposalDate.HasValue && 
                    asset.DisposalDate.Value <= financialPeriod.EndDate)
                {
                    assetDetail.DisposalDate = asset.DisposalDate;
                    assetDetail.DisposalType = asset.DisposalType;
                    assetDetail.DisposalAmount = asset.DisposalAmount ?? 0;
                    assetDetail.GainLossOnDisposal = asset.DisposalGainLoss ?? 0;
                    
                    // Update totals for disposed assets
                    result.TotalDisposedAssetsCount++;
                    result.TotalDisposalProceeds += asset.DisposalAmount ?? 0;
                    result.TotalGainLoss += asset.DisposalGainLoss ?? 0;
                }
                
                // Add to asset details
                result.AssetDetails.Add(assetDetail);
                
                // Update category totals
                if (!result.AssetsByCategory.TryGetValue(asset.AssetCategory, out var categoryReport))
                {
                    categoryReport = new AssetCategoryReportDto
                    {
                        CategoryName = asset.AssetCategory,
                        AssetCount = 0,
                        TotalPurchaseCost = 0,
                        TotalAccumulatedDepreciation = 0,
                        TotalNetBookValue = 0
                    };
                    result.AssetsByCategory[asset.AssetCategory] = categoryReport;
                }
                
                categoryReport.AssetCount++;
                categoryReport.TotalPurchaseCost += asset.PurchaseCost;
                categoryReport.TotalAccumulatedDepreciation += asset.AccumulatedDepreciation;
                categoryReport.TotalNetBookValue += asset.CurrentBookValue;
                
                // Update overall totals
                result.TotalPurchaseCost += asset.PurchaseCost;
                result.TotalAccumulatedDepreciation += asset.AccumulatedDepreciation;
                result.TotalNetBookValue += asset.CurrentBookValue;
            }
            
            return result;
        }
        
        #region Private Helper Methods
        
        /// <summary>
        /// Maps a FixedAsset entity to a FixedAssetDto
        /// </summary>
        private FixedAssetDto MapFixedAssetToDto(FixedAsset fixedAsset)
        {
            if (fixedAsset == null)
            {
                return null;
            }
            
            return new FixedAssetDto
            {
                Id = fixedAsset.Id,
                AssetNumber = fixedAsset.AssetNumber,
                AssetName = fixedAsset.AssetName,
                Description = fixedAsset.Description,
                AssetCategory = fixedAsset.AssetCategory,
                AssetAccountId = fixedAsset.AssetAccountId,
                AccumulatedDepreciationAccountId = fixedAsset.AccumulatedDepreciationAccountId,
                DepreciationExpenseAccountId = fixedAsset.DepreciationExpenseAccountId,
                DisposalGainLossAccountId = fixedAsset.DisposalGainLossAccountId,
                PurchaseDate = fixedAsset.PurchaseDate,
                InServiceDate = fixedAsset.InServiceDate,
                PurchaseCost = fixedAsset.PurchaseCost,
                ResidualValue = fixedAsset.ResidualValue,
                DepreciableAmount = fixedAsset.DepreciableAmount,
                UsefulLifeYears = fixedAsset.UsefulLifeYears,
                DepreciationMethod = fixedAsset.DepreciationMethod,
                CurrentBookValue = fixedAsset.CurrentBookValue,
                AccumulatedDepreciation = fixedAsset.AccumulatedDepreciation,
                LastDepreciationDate = fixedAsset.LastDepreciationDate,
                Status = fixedAsset.Status,
                Location = fixedAsset.Location,
                AssetTag = fixedAsset.AssetTag,
                SerialNumber = fixedAsset.SerialNumber,
                Manufacturer = fixedAsset.Manufacturer,
                Model = fixedAsset.Model,
                WarrantyExpiryDate = fixedAsset.WarrantyExpiryDate,
                Notes = fixedAsset.Notes,
                CreatedDate = fixedAsset.CreatedDate,
                CreatedBy = fixedAsset.CreatedBy,
                LastModifiedDate = fixedAsset.LastModifiedDate,
                LastModifiedBy = fixedAsset.LastModifiedBy,
                DisposalDate = fixedAsset.DisposalDate,
                DisposalType = fixedAsset.DisposalType,
                DisposalAmount = fixedAsset.DisposalAmount,
                DisposalGainLoss = fixedAsset.DisposalGainLoss
            };
        }
        
        /// <summary>
        /// Generates a unique asset number
        /// </summary>
        private string GenerateAssetNumber()
        {
            // Simple asset number generator - in a real system, this might use a sequence
            // or follow a specific format based on company policy
            return $"FA-{DateTime.UtcNow.ToString("yyyyMMdd")}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }
        
        /// <summary>
        /// Calculates total depreciation for an asset as of a specific date
        /// </summary>
        private decimal CalculateDepreciation(FixedAsset asset, DateTime asOfDate)
        {
            // If the asset hasn't been placed in service yet, no depreciation
            if (asset.InServiceDate > asOfDate)
            {
                return 0;
            }
            
            // If the asset is fully depreciated, return the depreciable amount
            if (asset.CurrentBookValue <= asset.ResidualValue)
            {
                return asset.DepreciableAmount;
            }
            
            // Calculate time period for depreciation
            var timeInService = asOfDate - asset.InServiceDate;
            var yearsInService = (decimal)timeInService.TotalDays / 365.25m;
            
            // Cap at useful life
            yearsInService = Math.Min(yearsInService, asset.UsefulLifeYears);
            
            // Calculate depreciation based on method
            decimal depreciation = 0;
            
            switch (asset.DepreciationMethod)
            {
                case DepreciationMethod.StraightLine:
                    // Simple straight-line depreciation
                    depreciation = asset.DepreciableAmount * (yearsInService / asset.UsefulLifeYears);
                    break;
                    
                case DepreciationMethod.DoubleDecliningBalance:
                    // This is more complex and would need simulation of each period
                    // Simplified approximation for calculation purposes
                    var annualRate = 2 / asset.UsefulLifeYears;
                    var remainingValue = asset.PurchaseCost;
                    var fullYears = (int)Math.Floor(yearsInService);
                    
                    // Calculate for full years
                    for (int i = 0; i < fullYears; i++)
                    {
                        var yearDepreciation = remainingValue * annualRate;
                        
                        // Switch to straight-line if it yields higher depreciation
                        var remainingLifeYears = asset.UsefulLifeYears - i;
                        var straightLineAmount = (remainingValue - asset.ResidualValue) / remainingLifeYears;
                        
                        if (straightLineAmount > yearDepreciation)
                        {
                            yearDepreciation = straightLineAmount;
                        }
                        
                        depreciation += yearDepreciation;
                        remainingValue -= yearDepreciation;
                        
                        // Don't depreciate below residual value
                        if (remainingValue <= asset.ResidualValue)
                        {
                            remainingValue = asset.ResidualValue;
                            break;
                        }
                    }
                    
                    // Add partial year if needed
                    var partialYear = yearsInService - fullYears;
                    if (partialYear > 0 && remainingValue > asset.ResidualValue)
                    {
                        var partialYearDepreciation = remainingValue * annualRate * partialYear;
                        
                        // Switch to straight-line if needed
                        var remainingLifeYears = asset.UsefulLifeYears - fullYears;
                        var straightLineAmount = (remainingValue - asset.ResidualValue) / remainingLifeYears * partialYear;
                        
                        if (straightLineAmount > partialYearDepreciation)
                        {
                            partialYearDepreciation = straightLineAmount;
                        }
                        
                        // Don't depreciate below residual value
                        if (remainingValue - partialYearDepreciation < asset.ResidualValue)
                        {
                            partialYearDepreciation = remainingValue - asset.ResidualValue;
                        }
                        
                        depreciation += partialYearDepreciation;
                    }
                    break;
                    
                case DepreciationMethod.SumOfYearsDigits:
                    // Sum of years digits calculation
                    var sumOfYears = asset.UsefulLifeYears * (asset.UsefulLifeYears + 1) / 2;
                    var fullYearsSYD = (int)Math.Floor(yearsInService);
                    
                    // Calculate for full years
                    for (int i = 1; i <= fullYearsSYD; i++)
                    {
                        var factor = (asset.UsefulLifeYears - i + 1) / (decimal)sumOfYears;
                        depreciation += asset.DepreciableAmount * factor;
                    }
                    
                    // Add partial year if needed
                    var partialYearSYD = yearsInService - fullYearsSYD;
                    if (partialYearSYD > 0)
                    {
                        var factor = (asset.UsefulLifeYears - fullYearsSYD) / (decimal)sumOfYears;
                        depreciation += asset.DepreciableAmount * factor * partialYearSYD;
                    }
                    break;
                    
                case DepreciationMethod.Units:
                    // For units of production, this would require tracking actual usage
                    // Using a simplified approach here (similar to straight-line)
                    depreciation = asset.DepreciableAmount * (yearsInService / asset.UsefulLifeYears);
                    break;
            }
            
            // Ensure depreciation doesn't exceed depreciable amount
            depreciation = Math.Min(depreciation, asset.DepreciableAmount);
            
            return Math.Round(depreciation, 2);
        }
        
        /// <summary>
        /// Calculates depreciation for a specific financial period
        /// </summary>
        private decimal CalculateDepreciationForPeriod(FixedAsset asset, FinancialPeriod period)
        {
            // If the asset hasn't been placed in service yet, no depreciation
            if (asset.InServiceDate > period.EndDate)
            {
                return 0;
            }
            
            // If the asset is fully depreciated, no more depreciation
            if (asset.CurrentBookValue <= asset.ResidualValue)
            {
                return 0;
            }
            
            // Calculate total depreciation up to period end date
            var totalDepreciationToEndDate = CalculateDepreciation(asset, period.EndDate);
            
            // If we already have recorded depreciation, subtract it
            decimal periodDepreciation = totalDepreciationToEndDate - asset.AccumulatedDepreciation;
            
            // Ensure non-negative
            periodDepreciation = Math.Max(0, periodDepreciation);
            
            // Ensure we don't depreciate below residual value
            if (asset.CurrentBookValue - periodDepreciation < asset.ResidualValue)
            {
                periodDepreciation = asset.CurrentBookValue - asset.ResidualValue;
            }
            
            return Math.Round(periodDepreciation, 2);
        }
        
        /// <summary>
        /// Calculates depreciation from last depreciation date to a specific date
        /// </summary>
        private decimal CalculateDepreciationToDate(FixedAsset asset, DateTime toDate)
        {
            // If the asset is fully depreciated, no more depreciation
            if (asset.CurrentBookValue <= asset.ResidualValue)
            {
                return 0;
            }
            
            // If we don't have a last depreciation date, use in-service date
            var fromDate = asset.LastDepreciationDate ?? asset.InServiceDate;
            
            // If from date is after to date, no depreciation
            if (fromDate >= toDate)
            {
                return 0;
            }
            
            // Calculate total depreciation up to toDate
            var totalDepreciationToDate = CalculateDepreciation(asset, toDate);
            
            // Calculate additional depreciation needed
            decimal additionalDepreciation = totalDepreciationToDate - asset.AccumulatedDepreciation;
            
            // Ensure non-negative
            additionalDepreciation = Math.Max(0, additionalDepreciation);
            
            // Ensure we don't depreciate below residual value
            if (asset.CurrentBookValue - additionalDepreciation < asset.ResidualValue)
            {
                additionalDepreciation = asset.CurrentBookValue - asset.ResidualValue;
            }
            
            return Math.Round(additionalDepreciation, 2);
        }
        
        #endregion
    }
    
    #region Domain Entities
    
    /// <summary>
    /// Represents a fixed asset
    /// </summary>
    public class FixedAsset
    {
        public string Id { get; set; }
        public string AssetNumber { get; set; }
        public string AssetName { get; set; }
        public string Description { get; set; }
        public string AssetCategory { get; set; }
        public string AssetAccountId { get; set; }
        public string AccumulatedDepreciationAccountId { get; set; }
        public string DepreciationExpenseAccountId { get; set; }
        public string DisposalGainLossAccountId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime InServiceDate { get; set; }
        public decimal PurchaseCost { get; set; }
        public decimal ResidualValue { get; set; }
        public decimal DepreciableAmount { get; set; }
        public int UsefulLifeYears { get; set; }
        public DepreciationMethod DepreciationMethod { get; set; }
        public decimal CurrentBookValue { get; set; }
        public decimal AccumulatedDepreciation { get; set; }
        public DateTime? LastDepreciationDate { get; set; }
        public FixedAssetStatus Status { get; set; }
        public string Location { get; set; }
        public string AssetTag { get; set; }
        public string SerialNumber { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public DateTime? WarrantyExpiryDate { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime? DisposalDate { get; set; }
        public string DisposalType { get; set; }
        public decimal? DisposalAmount { get; set; }
        public decimal? DisposalGainLoss { get; set; }
    }
    
    /// <summary>
    /// Depreciation methods enum
    /// </summary>
    public enum DepreciationMethod
    {
        StraightLine,
        DoubleDecliningBalance,
        SumOfYearsDigits,
        Units
    }
    
    /// <summary>
    /// Fixed asset status enum
    /// </summary>
    public enum FixedAssetStatus
    {
        Draft,
        Active,
        Inactive,
        UnderMaintenance,
        Disposed
    }
    
    #endregion
    
    #region DTOs
    
    public class CreateFixedAssetDto
    {
        public string AssetNumber { get; set; }
        public string AssetName { get; set; }
        public string Description { get; set; }
        public string AssetCategory { get; set; }
        public string AssetAccountId { get; set; }
        public string AccumulatedDepreciationAccountId { get; set; }
        public string DepreciationExpenseAccountId { get; set; }
        public string DisposalGainLossAccountId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime InServiceDate { get; set; }
        public decimal PurchaseCost { get; set; }
        public decimal ResidualValue { get; set; }
        public int UsefulLifeYears { get; set; }
        public DepreciationMethod DepreciationMethod { get; set; }
        public string Location { get; set; }
        public string AssetTag { get; set; }
        public string SerialNumber { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public DateTime? WarrantyExpiryDate { get; set; }
        public string Notes { get; set; }
        public string CreatedBy { get; set; }
    }
    
    public class UpdateFixedAssetDto
    {
        public string AssetName { get; set; }
        public string Description { get; set; }
        public string AssetCategory { get; set; }
        public string AssetAccountId { get; set; }
        public string AccumulatedDepreciationAccountId { get; set; }
        public string DepreciationExpenseAccountId { get; set; }
        public string DisposalGainLossAccountId { get; set; }
        public string Location { get; set; }
        public string Notes { get; set; }
        public string LastModifiedBy { get; set; }
        public bool ForceUpdate { get; set; } = false;
    }
    
    public class FixedAssetDto
    {
        public string Id { get; set; }
        public string AssetNumber { get; set; }
        public string AssetName { get; set; }
        public string Description { get; set; }
        public string AssetCategory { get; set; }
        public string AssetAccountId { get; set; }
        public string AccumulatedDepreciationAccountId { get; set; }
        public string DepreciationExpenseAccountId { get; set; }
        public string DisposalGainLossAccountId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime InServiceDate { get; set; }
        public decimal PurchaseCost { get; set; }
        public decimal ResidualValue { get; set; }
        public decimal DepreciableAmount { get; set; }
        public int UsefulLifeYears { get; set; }
        public DepreciationMethod DepreciationMethod { get; set; }
        public decimal CurrentBookValue { get; set; }
        public decimal AccumulatedDepreciation { get; set; }
        public DateTime? LastDepreciationDate { get; set; }
        public FixedAssetStatus Status { get; set; }
        public string Location { get; set; }
        public string AssetTag { get; set; }
        public string SerialNumber { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public DateTime? WarrantyExpiryDate { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime? DisposalDate { get; set; }
        public string DisposalType { get; set; }
        public decimal? DisposalAmount { get; set; }
        public decimal? DisposalGainLoss { get; set; }
    }
    
    public class DepreciationScheduleDto
    {
        public string AssetId { get; set; }
        public string AssetNumber { get; set; }
        public string AssetName { get; set; }
        public decimal PurchaseCost { get; set; }
        public decimal ResidualValue { get; set; }
        public decimal DepreciableAmount { get; set; }
        public int UsefulLifeYears { get; set; }
        public DepreciationMethod DepreciationMethod { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime GeneratedAt { get; set; }
        public List<DepreciationScheduleItemDto> ScheduleItems { get; set; }
    }
    
    public class DepreciationScheduleItemDto
    {
        public int Year { get; set; }
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public decimal DepreciationAmount { get; set; }
        public decimal AccumulatedDepreciation { get; set; }
        public decimal BookValueEndOfPeriod { get; set; }
    }
    
    public class AssetDisposalDto
    {
        public string FinancialPeriodId { get; set; }
        public DateTime DisposalDate { get; set; }
        public string DisposalType { get; set; } // Sale, Scrapped, Donated, etc.
        public decimal DisposalAmount { get; set; }
        public string CashAccountId { get; set; }
        public string DisposalGainLossAccountId { get; set; }
        public bool RecordFinalDepreciation { get; set; } = true;
        public string DisposedBy { get; set; }
        public string Notes { get; set; }
    }
    
    public class FixedAssetDisposalDto
    {
        public string AssetId { get; set; }
        public string AssetNumber { get; set; }
        public string AssetName { get; set; }
        public DateTime DisposalDate { get; set; }
        public string DisposalType { get; set; }
        public decimal DisposalAmount { get; set; }
        public decimal NetBookValueAtDisposal { get; set; }
        public decimal GainLossOnDisposal { get; set; }
        public string DisposedBy { get; set; }
        public string Notes { get; set; }
    }
    
    public class FixedAssetReportDto
    {
        public string FinancialPeriodId { get; set; }
        public string FinancialPeriodName { get; set; }
        public DateTime GeneratedAt { get; set; }
        public Dictionary<string, AssetCategoryReportDto> AssetsByCategory { get; set; }
        public List<AssetReportDetailDto> AssetDetails { get; set; }
        public decimal TotalPurchaseCost { get; set; }
        public decimal TotalAccumulatedDepreciation { get; set; }
        public decimal TotalNetBookValue { get; set; }
        public int TotalDisposedAssetsCount { get; set; }
        public decimal TotalDisposalProceeds { get; set; }
        public decimal TotalGainLoss { get; set; }
    }
    
    public class AssetCategoryReportDto
    {
        public string CategoryName { get; set; }
        public int AssetCount { get; set; }
        public decimal TotalPurchaseCost { get; set; }
        public decimal TotalAccumulatedDepreciation { get; set; }
        public decimal TotalNetBookValue { get; set; }
    }
    
    public class AssetReportDetailDto
    {
        public string AssetId { get; set; }
        public string AssetNumber { get; set; }
        public string AssetName { get; set; }
        public string AssetCategory { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime InServiceDate { get; set; }
        public decimal PurchaseCost { get; set; }
        public decimal AccumulatedDepreciation { get; set; }
        public decimal NetBookValue { get; set; }
        public string Status { get; set; }
        public string Location { get; set; }
        public int UsefulLifeYears { get; set; }
        public string DepreciationMethod { get; set; }
        public DateTime? DisposalDate { get; set; }
        public string DisposalType { get; set; }
        public decimal DisposalAmount { get; set; }
        public decimal GainLossOnDisposal { get; set; }
    }
    
    #endregion
}