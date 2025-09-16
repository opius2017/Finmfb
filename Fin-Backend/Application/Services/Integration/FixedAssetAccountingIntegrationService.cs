using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Application.Services.Accounting;
using FinTech.Domain.Entities.Accounting;
using FinTech.Domain.Entities.FixedAssets;
using FinTech.Domain.Repositories.Accounting;

namespace FinTech.Application.Services.Integration
{
    public interface IFixedAssetAccountingIntegrationService
    {
        Task<string> ProcessAssetAcquisitionAsync(
            Asset asset, 
            string financialPeriodId, 
            CancellationToken cancellationToken = default);
            
        Task<string> ProcessDepreciationExpenseAsync(
            IEnumerable<AssetDepreciationSchedule> depreciationSchedules, 
            DateTime depreciationDate,
            string financialPeriodId, 
            CancellationToken cancellationToken = default);
            
        Task<string> ProcessAssetDisposalAsync(
            AssetDisposal assetDisposal, 
            string financialPeriodId, 
            CancellationToken cancellationToken = default);
            
        Task<string> ProcessAssetRevaluationAsync(
            AssetRevaluation assetRevaluation, 
            string financialPeriodId, 
            CancellationToken cancellationToken = default);
    }

    public class FixedAssetAccountingIntegrationService : IFixedAssetAccountingIntegrationService
    {
        private readonly IJournalEntryService _journalEntryService;
        private readonly IChartOfAccountService _chartOfAccountService;
        private readonly IFinancialPeriodService _financialPeriodService;
        private readonly IChartOfAccountRepository _chartOfAccountRepository;

        public FixedAssetAccountingIntegrationService(
            IJournalEntryService journalEntryService,
            IChartOfAccountService chartOfAccountService,
            IFinancialPeriodService financialPeriodService,
            IChartOfAccountRepository chartOfAccountRepository)
        {
            _journalEntryService = journalEntryService ?? throw new ArgumentNullException(nameof(journalEntryService));
            _chartOfAccountService = chartOfAccountService ?? throw new ArgumentNullException(nameof(chartOfAccountService));
            _financialPeriodService = financialPeriodService ?? throw new ArgumentNullException(nameof(financialPeriodService));
            _chartOfAccountRepository = chartOfAccountRepository ?? throw new ArgumentNullException(nameof(chartOfAccountRepository));
        }

        public async Task<string> ProcessAssetAcquisitionAsync(
            Asset asset, 
            string financialPeriodId, 
            CancellationToken cancellationToken = default)
        {
            // Validate inputs
            if (asset == null)
                throw new ArgumentNullException(nameof(asset));

            if (asset.AcquisitionCost <= 0)
                throw new ArgumentException("Acquisition cost must be greater than zero", nameof(asset));

            // Get the relevant GL accounts
            var fixedAssetAccount = await _chartOfAccountRepository.GetByAccountNumberAsync("1500", cancellationToken);  // Fixed assets
            var bankAccount = await _chartOfAccountRepository.GetByAccountNumberAsync("1010", cancellationToken);  // Bank account

            if (fixedAssetAccount == null || bankAccount == null)
                throw new InvalidOperationException("Required GL accounts not found");

            // Create journal entry
            var journalEntry = new JournalEntry
            {
                Description = $"Acquisition of fixed asset: {asset.AssetName} ({asset.AssetNumber})",
                EntryDate = asset.AcquisitionDate,
                EntryType = JournalEntryType.Standard,
                FinancialPeriodId = financialPeriodId,
                IsSystemGenerated = true,
                CreatedBy = "FixedAssetIntegration",
                JournalEntryLines = new List<JournalEntryLine>
                {
                    // Debit Fixed Asset
                    new JournalEntryLine
                    {
                        AccountId = fixedAssetAccount.Id,
                        Description = $"Acquisition of {asset.AssetName} ({asset.AssetNumber})",
                        DebitAmount = asset.AcquisitionCost,
                        CreditAmount = 0,
                        CreatedBy = "FixedAssetIntegration"
                    },
                    // Credit Bank Account
                    new JournalEntryLine
                    {
                        AccountId = bankAccount.Id,
                        Description = $"Payment for {asset.AssetName} ({asset.AssetNumber})",
                        DebitAmount = 0,
                        CreditAmount = asset.AcquisitionCost,
                        CreatedBy = "FixedAssetIntegration"
                    }
                }
            };

            // Create, submit, approve, and post the journal entry
            var journalEntryId = await _journalEntryService.CreateJournalEntryAsync(journalEntry, cancellationToken);
            await _journalEntryService.SubmitForApprovalAsync(journalEntryId, "FixedAssetIntegration", cancellationToken);
            await _journalEntryService.ApproveJournalEntryAsync(journalEntryId, "FixedAssetIntegration", cancellationToken);
            await _journalEntryService.PostJournalEntryAsync(journalEntryId, "FixedAssetIntegration", cancellationToken);

            return journalEntryId;
        }

        public async Task<string> ProcessDepreciationExpenseAsync(
            IEnumerable<AssetDepreciationSchedule> depreciationSchedules, 
            DateTime depreciationDate,
            string financialPeriodId, 
            CancellationToken cancellationToken = default)
        {
            // Validate inputs
            if (depreciationSchedules == null || !depreciationSchedules.Any())
                throw new ArgumentException("Depreciation schedules cannot be null or empty", nameof(depreciationSchedules));

            // Get the relevant GL accounts
            var depreciationExpenseAccount = await _chartOfAccountRepository.GetByAccountNumberAsync("5200", cancellationToken);  // Depreciation expense
            var accumulatedDepreciationAccount = await _chartOfAccountRepository.GetByAccountNumberAsync("1590", cancellationToken);  // Accumulated depreciation

            if (depreciationExpenseAccount == null || accumulatedDepreciationAccount == null)
                throw new InvalidOperationException("Required GL accounts not found");

            // Calculate total depreciation amount
            decimal totalDepreciation = depreciationSchedules.Sum(ds => ds.DepreciationAmount);

            // Create journal entry
            var journalEntry = new JournalEntry
            {
                Description = $"Depreciation expense for period ending {depreciationDate:yyyy-MM-dd}",
                EntryDate = depreciationDate,
                EntryType = JournalEntryType.Standard,
                FinancialPeriodId = financialPeriodId,
                IsSystemGenerated = true,
                CreatedBy = "FixedAssetIntegration",
                JournalEntryLines = new List<JournalEntryLine>
                {
                    // Debit Depreciation Expense
                    new JournalEntryLine
                    {
                        AccountId = depreciationExpenseAccount.Id,
                        Description = $"Depreciation expense for period ending {depreciationDate:yyyy-MM-dd}",
                        DebitAmount = totalDepreciation,
                        CreditAmount = 0,
                        CreatedBy = "FixedAssetIntegration"
                    },
                    // Credit Accumulated Depreciation
                    new JournalEntryLine
                    {
                        AccountId = accumulatedDepreciationAccount.Id,
                        Description = $"Accumulated depreciation for period ending {depreciationDate:yyyy-MM-dd}",
                        DebitAmount = 0,
                        CreditAmount = totalDepreciation,
                        CreatedBy = "FixedAssetIntegration"
                    }
                }
            };

            // Create, submit, approve, and post the journal entry
            var journalEntryId = await _journalEntryService.CreateJournalEntryAsync(journalEntry, cancellationToken);
            await _journalEntryService.SubmitForApprovalAsync(journalEntryId, "FixedAssetIntegration", cancellationToken);
            await _journalEntryService.ApproveJournalEntryAsync(journalEntryId, "FixedAssetIntegration", cancellationToken);
            await _journalEntryService.PostJournalEntryAsync(journalEntryId, "FixedAssetIntegration", cancellationToken);

            return journalEntryId;
        }

        public async Task<string> ProcessAssetDisposalAsync(
            AssetDisposal assetDisposal, 
            string financialPeriodId, 
            CancellationToken cancellationToken = default)
        {
            // Validate inputs
            if (assetDisposal == null)
                throw new ArgumentNullException(nameof(assetDisposal));
                
            if (assetDisposal.Asset == null)
                throw new ArgumentException("Asset cannot be null", nameof(assetDisposal));

            // Get the relevant GL accounts
            var fixedAssetAccount = await _chartOfAccountRepository.GetByAccountNumberAsync("1500", cancellationToken);  // Fixed assets
            var accumulatedDepreciationAccount = await _chartOfAccountRepository.GetByAccountNumberAsync("1590", cancellationToken);  // Accumulated depreciation
            var bankAccount = await _chartOfAccountRepository.GetByAccountNumberAsync("1010", cancellationToken);  // Bank account
            var gainLossAccount = await _chartOfAccountRepository.GetByAccountNumberAsync("4900", cancellationToken);  // Gain/loss on asset disposal

            if (fixedAssetAccount == null || accumulatedDepreciationAccount == null || 
                bankAccount == null || gainLossAccount == null)
                throw new InvalidOperationException("Required GL accounts not found");

            // Calculate values for the journal entry
            decimal acquisitionCost = assetDisposal.Asset.AcquisitionCost;
            decimal accumulatedDepreciation = assetDisposal.Asset.AccumulatedDepreciation;
            decimal bookValue = acquisitionCost - accumulatedDepreciation;
            decimal disposalProceeds = assetDisposal.DisposalAmount;
            decimal gainLoss = disposalProceeds - bookValue;

            // Create journal entry
            var journalEntry = new JournalEntry
            {
                Description = $"Disposal of asset: {assetDisposal.Asset.AssetName} ({assetDisposal.Asset.AssetNumber})",
                EntryDate = assetDisposal.DisposalDate,
                EntryType = JournalEntryType.Standard,
                FinancialPeriodId = financialPeriodId,
                IsSystemGenerated = true,
                CreatedBy = "FixedAssetIntegration",
                JournalEntryLines = new List<JournalEntryLine>
                {
                    // Debit Bank Account for the disposal proceeds
                    new JournalEntryLine
                    {
                        AccountId = bankAccount.Id,
                        Description = $"Proceeds from disposal of {assetDisposal.Asset.AssetName}",
                        DebitAmount = disposalProceeds,
                        CreditAmount = 0,
                        CreatedBy = "FixedAssetIntegration"
                    },
                    // Debit Accumulated Depreciation
                    new JournalEntryLine
                    {
                        AccountId = accumulatedDepreciationAccount.Id,
                        Description = $"Reversal of accumulated depreciation for {assetDisposal.Asset.AssetName}",
                        DebitAmount = accumulatedDepreciation,
                        CreditAmount = 0,
                        CreatedBy = "FixedAssetIntegration"
                    },
                    // Credit Fixed Asset
                    new JournalEntryLine
                    {
                        AccountId = fixedAssetAccount.Id,
                        Description = $"Removal of asset {assetDisposal.Asset.AssetName} from books",
                        DebitAmount = 0,
                        CreditAmount = acquisitionCost,
                        CreatedBy = "FixedAssetIntegration"
                    }
                }
            };

            // Add gain or loss entry
            if (gainLoss > 0)
            {
                // Gain on disposal
                journalEntry.JournalEntryLines.Add(new JournalEntryLine
                {
                    AccountId = gainLossAccount.Id,
                    Description = $"Gain on disposal of {assetDisposal.Asset.AssetName}",
                    DebitAmount = 0,
                    CreditAmount = gainLoss,
                    CreatedBy = "FixedAssetIntegration"
                });
            }
            else if (gainLoss < 0)
            {
                // Loss on disposal
                journalEntry.JournalEntryLines.Add(new JournalEntryLine
                {
                    AccountId = gainLossAccount.Id,
                    Description = $"Loss on disposal of {assetDisposal.Asset.AssetName}",
                    DebitAmount = Math.Abs(gainLoss),
                    CreditAmount = 0,
                    CreatedBy = "FixedAssetIntegration"
                });
            }

            // Create, submit, approve, and post the journal entry
            var journalEntryId = await _journalEntryService.CreateJournalEntryAsync(journalEntry, cancellationToken);
            await _journalEntryService.SubmitForApprovalAsync(journalEntryId, "FixedAssetIntegration", cancellationToken);
            await _journalEntryService.ApproveJournalEntryAsync(journalEntryId, "FixedAssetIntegration", cancellationToken);
            await _journalEntryService.PostJournalEntryAsync(journalEntryId, "FixedAssetIntegration", cancellationToken);

            return journalEntryId;
        }

        public async Task<string> ProcessAssetRevaluationAsync(
            AssetRevaluation assetRevaluation, 
            string financialPeriodId, 
            CancellationToken cancellationToken = default)
        {
            // Validate inputs
            if (assetRevaluation == null)
                throw new ArgumentNullException(nameof(assetRevaluation));
                
            if (assetRevaluation.Asset == null)
                throw new ArgumentException("Asset cannot be null", nameof(assetRevaluation));

            // Get the relevant GL accounts
            var fixedAssetAccount = await _chartOfAccountRepository.GetByAccountNumberAsync("1500", cancellationToken);  // Fixed assets
            var revaluationReserveAccount = await _chartOfAccountRepository.GetByAccountNumberAsync("3100", cancellationToken);  // Revaluation reserve

            if (fixedAssetAccount == null || revaluationReserveAccount == null)
                throw new InvalidOperationException("Required GL accounts not found");

            // Calculate revaluation amount (increase or decrease)
            decimal revaluationAmount = assetRevaluation.NewValue - assetRevaluation.PreviousValue;

            // Skip if there's no change in value
            if (revaluationAmount == 0)
                throw new InvalidOperationException("Revaluation amount is zero, no journal entry needed");

            // Create journal entry
            var journalEntry = new JournalEntry
            {
                Description = $"Revaluation of asset: {assetRevaluation.Asset.AssetName} ({assetRevaluation.Asset.AssetNumber})",
                EntryDate = assetRevaluation.RevaluationDate,
                EntryType = JournalEntryType.Standard,
                FinancialPeriodId = financialPeriodId,
                IsSystemGenerated = true,
                CreatedBy = "FixedAssetIntegration",
                JournalEntryLines = new List<JournalEntryLine>()
            };

            if (revaluationAmount > 0)
            {
                // Upward revaluation
                journalEntry.JournalEntryLines.Add(new JournalEntryLine
                {
                    AccountId = fixedAssetAccount.Id,
                    Description = $"Increase in value of {assetRevaluation.Asset.AssetName}",
                    DebitAmount = revaluationAmount,
                    CreditAmount = 0,
                    CreatedBy = "FixedAssetIntegration"
                });
                
                journalEntry.JournalEntryLines.Add(new JournalEntryLine
                {
                    AccountId = revaluationReserveAccount.Id,
                    Description = $"Revaluation reserve for {assetRevaluation.Asset.AssetName}",
                    DebitAmount = 0,
                    CreditAmount = revaluationAmount,
                    CreatedBy = "FixedAssetIntegration"
                });
            }
            else
            {
                // Downward revaluation
                decimal absoluteAmount = Math.Abs(revaluationAmount);
                
                journalEntry.JournalEntryLines.Add(new JournalEntryLine
                {
                    AccountId = revaluationReserveAccount.Id,
                    Description = $"Reduction in revaluation reserve for {assetRevaluation.Asset.AssetName}",
                    DebitAmount = absoluteAmount,
                    CreditAmount = 0,
                    CreatedBy = "FixedAssetIntegration"
                });
                
                journalEntry.JournalEntryLines.Add(new JournalEntryLine
                {
                    AccountId = fixedAssetAccount.Id,
                    Description = $"Decrease in value of {assetRevaluation.Asset.AssetName}",
                    DebitAmount = 0,
                    CreditAmount = absoluteAmount,
                    CreatedBy = "FixedAssetIntegration"
                });
            }

            // Create, submit, approve, and post the journal entry
            var journalEntryId = await _journalEntryService.CreateJournalEntryAsync(journalEntry, cancellationToken);
            await _journalEntryService.SubmitForApprovalAsync(journalEntryId, "FixedAssetIntegration", cancellationToken);
            await _journalEntryService.ApproveJournalEntryAsync(journalEntryId, "FixedAssetIntegration", cancellationToken);
            await _journalEntryService.PostJournalEntryAsync(journalEntryId, "FixedAssetIntegration", cancellationToken);

            return journalEntryId;
        }
    }
}