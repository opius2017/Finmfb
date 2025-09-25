using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FinTech.Core.Application.Interfaces.Services;
using FinTech.Core.Application.Services.Integration;
using System.Collections.Generic;

namespace FinTech.Infrastructure.Services.Integration
{
    public class FixedAssetAccountingIntegrationService : IFixedAssetAccountingIntegrationService
    {
        private readonly ILogger<FixedAssetAccountingIntegrationService> _logger;
        private readonly IJournalEntryService _journalEntryService;
        private readonly IChartOfAccountService _chartOfAccountService;

        public FixedAssetAccountingIntegrationService(
            ILogger<FixedAssetAccountingIntegrationService> logger,
            IJournalEntryService journalEntryService,
            IChartOfAccountService chartOfAccountService)
        {
            _logger = logger;
            _journalEntryService = journalEntryService;
            _chartOfAccountService = chartOfAccountService;
        }

        public async Task ProcessAssetAcquisitionAsync(
            int assetId, 
            decimal acquisitionCost, 
            decimal taxAmount, 
            string assetCategory, 
            string reference, 
            string description)
        {
            try
            {
                decimal totalCost = acquisitionCost + taxAmount;
                
                _logger.LogInformation("Processing asset acquisition for asset {AssetId} with cost {AcquisitionCost} in category {AssetCategory}", 
                    assetId, acquisitionCost, assetCategory);
                
                // Get the accounts from chart of accounts
                var fixedAssetAccountId = await _chartOfAccountService.GetFixedAssetAccountIdAsync(assetCategory);
                var taxPayableAccountId = await _chartOfAccountService.GetTaxPayableAccountIdAsync();
                var cashAccountId = await _chartOfAccountService.GetCashAccountIdAsync();
                
                // Create journal entry lines
                var journalLines = new List<JournalEntryLineDto>
                {
                    new JournalEntryLineDto
                    {
                        AccountId = fixedAssetAccountId,
                        Description = $"Asset acquisition for asset {assetId} in category {assetCategory}",
                        DebitAmount = acquisitionCost,
                        CreditAmount = 0
                    },
                    new JournalEntryLineDto
                    {
                        AccountId = cashAccountId,
                        Description = $"Cash payment for asset {assetId}",
                        DebitAmount = 0,
                        CreditAmount = totalCost
                    }
                };
                
                // Add tax amount if applicable
                if (taxAmount > 0)
                {
                    journalLines.Add(new JournalEntryLineDto
                    {
                        AccountId = taxPayableAccountId,
                        Description = $"Tax on asset acquisition for asset {assetId}",
                        DebitAmount = taxAmount,
                        CreditAmount = 0
                    });
                }
                
                // Create the journal entry
                await _journalEntryService.CreateJournalEntryAsync(
                    new JournalEntryDto
                    {
                        TransactionDate = DateTime.UtcNow,
                        Reference = reference,
                        Description = description,
                        Source = "FixedAssets",
                        Lines = journalLines
                    });
                
                _logger.LogInformation("Successfully processed asset acquisition for asset {AssetId}", assetId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing asset acquisition for asset {AssetId}", assetId);
                throw;
            }
        }

        public async Task ProcessAssetDepreciationAsync(int assetId, decimal depreciationAmount, string period, string reference, string description)
        {
            try
            {
                _logger.LogInformation("Processing asset depreciation for asset {AssetId} with amount {DepreciationAmount} for period {Period}", 
                    assetId, depreciationAmount, period);
                
                // Get asset category for the asset (in a real implementation this would be fetched from the asset repository)
                string assetCategory = await GetAssetCategoryAsync(assetId);
                
                // Get the accounts from chart of accounts
                var accumulatedDepreciationAccountId = await _chartOfAccountService.GetAccumulatedDepreciationAccountIdAsync(assetCategory);
                var depreciationExpenseAccountId = await _chartOfAccountService.GetDepreciationExpenseAccountIdAsync(assetCategory);
                
                // Create journal entry lines
                var journalLines = new List<JournalEntryLineDto>
                {
                    new JournalEntryLineDto
                    {
                        AccountId = depreciationExpenseAccountId,
                        Description = $"Depreciation expense for asset {assetId} for period {period}",
                        DebitAmount = depreciationAmount,
                        CreditAmount = 0
                    },
                    new JournalEntryLineDto
                    {
                        AccountId = accumulatedDepreciationAccountId,
                        Description = $"Accumulated depreciation for asset {assetId} for period {period}",
                        DebitAmount = 0,
                        CreditAmount = depreciationAmount
                    }
                };
                
                // Create the journal entry
                await _journalEntryService.CreateJournalEntryAsync(
                    new JournalEntryDto
                    {
                        TransactionDate = DateTime.UtcNow,
                        Reference = reference,
                        Description = description,
                        Source = "FixedAssets",
                        Lines = journalLines
                    });
                
                _logger.LogInformation("Successfully processed asset depreciation for asset {AssetId}", assetId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing asset depreciation for asset {AssetId}", assetId);
                throw;
            }
        }

        public async Task ProcessAssetDisposalAsync(
            int assetId, 
            decimal disposalProceeds, 
            decimal netBookValue, 
            decimal gainLoss, 
            string reference, 
            string description)
        {
            try
            {
                _logger.LogInformation("Processing asset disposal for asset {AssetId} with proceeds {DisposalProceeds} and NBV {NetBookValue}", 
                    assetId, disposalProceeds, netBookValue);
                
                // Get asset category for the asset
                string assetCategory = await GetAssetCategoryAsync(assetId);
                
                // Get the accounts from chart of accounts
                var fixedAssetAccountId = await _chartOfAccountService.GetFixedAssetAccountIdAsync(assetCategory);
                var accumulatedDepreciationAccountId = await _chartOfAccountService.GetAccumulatedDepreciationAccountIdAsync(assetCategory);
                var disposalGainAccountId = await _chartOfAccountService.GetAssetDisposalGainAccountIdAsync();
                var disposalLossAccountId = await _chartOfAccountService.GetAssetDisposalLossAccountIdAsync();
                var cashAccountId = await _chartOfAccountService.GetCashAccountIdAsync();
                
                // Calculate accumulated depreciation (acquisition cost - net book value)
                decimal acquisitionCost = await GetAssetAcquisitionCostAsync(assetId);
                decimal accumulatedDepreciation = acquisitionCost - netBookValue;
                
                // Create journal entry lines
                var journalLines = new List<JournalEntryLineDto>
                {
                    // Debit Cash for proceeds received
                    new JournalEntryLineDto
                    {
                        AccountId = cashAccountId,
                        Description = $"Proceeds from disposal of asset {assetId}",
                        DebitAmount = disposalProceeds,
                        CreditAmount = 0
                    },
                    
                    // Debit Accumulated Depreciation to remove it
                    new JournalEntryLineDto
                    {
                        AccountId = accumulatedDepreciationAccountId,
                        Description = $"Removal of accumulated depreciation for asset {assetId}",
                        DebitAmount = accumulatedDepreciation,
                        CreditAmount = 0
                    },
                    
                    // Credit Fixed Asset to remove the asset
                    new JournalEntryLineDto
                    {
                        AccountId = fixedAssetAccountId,
                        Description = $"Removal of asset {assetId} from fixed assets",
                        DebitAmount = 0,
                        CreditAmount = acquisitionCost
                    }
                };
                
                // Add gain or loss on disposal
                if (gainLoss > 0)
                {
                    // Gain on disposal
                    journalLines.Add(new JournalEntryLineDto
                    {
                        AccountId = disposalGainAccountId,
                        Description = $"Gain on disposal of asset {assetId}",
                        DebitAmount = 0,
                        CreditAmount = gainLoss
                    });
                }
                else if (gainLoss < 0)
                {
                    // Loss on disposal (use absolute value for the amount)
                    journalLines.Add(new JournalEntryLineDto
                    {
                        AccountId = disposalLossAccountId,
                        Description = $"Loss on disposal of asset {assetId}",
                        DebitAmount = Math.Abs(gainLoss),
                        CreditAmount = 0
                    });
                }
                
                // Create the journal entry
                await _journalEntryService.CreateJournalEntryAsync(
                    new JournalEntryDto
                    {
                        TransactionDate = DateTime.UtcNow,
                        Reference = reference,
                        Description = description,
                        Source = "FixedAssets",
                        Lines = journalLines
                    });
                
                _logger.LogInformation("Successfully processed asset disposal for asset {AssetId}", assetId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing asset disposal for asset {AssetId}", assetId);
                throw;
            }
        }

        public async Task ProcessAssetRevaluationAsync(int assetId, decimal revaluationAmount, string reference, string description)
        {
            try
            {
                _logger.LogInformation("Processing asset revaluation for asset {AssetId} with amount {RevaluationAmount}", 
                    assetId, revaluationAmount);
                
                // Get asset category for the asset
                string assetCategory = await GetAssetCategoryAsync(assetId);
                
                // Get the accounts from chart of accounts
                var fixedAssetAccountId = await _chartOfAccountService.GetFixedAssetAccountIdAsync(assetCategory);
                var revaluationReserveAccountId = await _chartOfAccountService.GetAssetRevaluationReserveAccountIdAsync();
                
                // Create journal entry lines
                var journalLines = new List<JournalEntryLineDto>();
                
                if (revaluationAmount > 0)
                {
                    // Upward revaluation
                    journalLines.Add(new JournalEntryLineDto
                    {
                        AccountId = fixedAssetAccountId,
                        Description = $"Upward revaluation of asset {assetId}",
                        DebitAmount = revaluationAmount,
                        CreditAmount = 0
                    });
                    
                    journalLines.Add(new JournalEntryLineDto
                    {
                        AccountId = revaluationReserveAccountId,
                        Description = $"Increase in revaluation reserve for asset {assetId}",
                        DebitAmount = 0,
                        CreditAmount = revaluationAmount
                    });
                }
                else if (revaluationAmount < 0)
                {
                    // Downward revaluation (use absolute value for the amount)
                    decimal absoluteAmount = Math.Abs(revaluationAmount);
                    
                    journalLines.Add(new JournalEntryLineDto
                    {
                        AccountId = revaluationReserveAccountId,
                        Description = $"Decrease in revaluation reserve for asset {assetId}",
                        DebitAmount = absoluteAmount,
                        CreditAmount = 0
                    });
                    
                    journalLines.Add(new JournalEntryLineDto
                    {
                        AccountId = fixedAssetAccountId,
                        Description = $"Downward revaluation of asset {assetId}",
                        DebitAmount = 0,
                        CreditAmount = absoluteAmount
                    });
                }
                
                if (journalLines.Count > 0)
                {
                    // Create the journal entry
                    await _journalEntryService.CreateJournalEntryAsync(
                        new JournalEntryDto
                        {
                            TransactionDate = DateTime.UtcNow,
                            Reference = reference,
                            Description = description,
                            Source = "FixedAssets",
                            Lines = journalLines
                        });
                    
                    _logger.LogInformation("Successfully processed asset revaluation for asset {AssetId}", assetId);
                }
                else
                {
                    _logger.LogInformation("No journal entry needed for zero revaluation amount for asset {AssetId}", assetId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing asset revaluation for asset {AssetId}", assetId);
                throw;
            }
        }

        public async Task ProcessAssetImpairmentAsync(int assetId, decimal impairmentAmount, string reference, string description)
        {
            try
            {
                _logger.LogInformation("Processing asset impairment for asset {AssetId} with amount {ImpairmentAmount}", 
                    assetId, impairmentAmount);
                
                // Get asset category for the asset
                string assetCategory = await GetAssetCategoryAsync(assetId);
                
                // Get the accounts from chart of accounts
                var fixedAssetAccountId = await _chartOfAccountService.GetFixedAssetAccountIdAsync(assetCategory);
                var impairmentAccountId = await _chartOfAccountService.GetAssetImpairmentAccountIdAsync();
                
                // Create journal entry lines
                var journalLines = new List<JournalEntryLineDto>
                {
                    new JournalEntryLineDto
                    {
                        AccountId = impairmentAccountId,
                        Description = $"Impairment expense for asset {assetId}",
                        DebitAmount = impairmentAmount,
                        CreditAmount = 0
                    },
                    new JournalEntryLineDto
                    {
                        AccountId = fixedAssetAccountId,
                        Description = $"Reduction in carrying value due to impairment for asset {assetId}",
                        DebitAmount = 0,
                        CreditAmount = impairmentAmount
                    }
                };
                
                // Create the journal entry
                await _journalEntryService.CreateJournalEntryAsync(
                    new JournalEntryDto
                    {
                        TransactionDate = DateTime.UtcNow,
                        Reference = reference,
                        Description = description,
                        Source = "FixedAssets",
                        Lines = journalLines
                    });
                
                _logger.LogInformation("Successfully processed asset impairment for asset {AssetId}", assetId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing asset impairment for asset {AssetId}", assetId);
                throw;
            }
        }

        // Helper methods
        private async Task<string> GetAssetCategoryAsync(int assetId)
        {
            // In a real implementation, this would fetch the asset category from the asset repository
            // For now, we'll return a placeholder
            return "Equipment";
        }

        private async Task<decimal> GetAssetAcquisitionCostAsync(int assetId)
        {
            // In a real implementation, this would fetch the acquisition cost from the asset repository
            // For now, we'll return a placeholder
            return 10000m;
        }
    }
}
