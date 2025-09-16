using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Domain.Entities.Accounting;
using FinTech.Domain.Repositories.Accounting;

namespace FinTech.Application.Services.Accounting
{
    public interface IFinancialAnalyticsService
    {
        Task<FinancialRatiosDto> CalculateFinancialRatiosAsync(
            string financialPeriodId,
            bool includeComparativePeriod = false,
            CancellationToken cancellationToken = default);
            
        Task<TrendAnalysisDto> GenerateTrendAnalysisAsync(
            int numberOfPeriods,
            string endPeriodId = null,
            string[] accountNumbers = null,
            string[] metrics = null,
            CancellationToken cancellationToken = default);
            
        Task<FinancialKpiDto> CalculateFinancialKpisAsync(
            string financialPeriodId,
            bool includeComparativePeriod = false,
            CancellationToken cancellationToken = default);
            
        Task<ProfitabilitySummaryDto> GenerateProfitabilitySummaryAsync(
            string financialPeriodId,
            string segmentType = null,
            CancellationToken cancellationToken = default);
    }
    
    public class FinancialAnalyticsService : IFinancialAnalyticsService
    {
        private readonly IFinancialPeriodRepository _financialPeriodRepository;
        private readonly IChartOfAccountRepository _chartOfAccountRepository;
        private readonly IGeneralLedgerService _generalLedgerService;
        private readonly ITrialBalanceService _trialBalanceService;
        private readonly IFinancialStatementService _financialStatementService;
        
        public FinancialAnalyticsService(
            IFinancialPeriodRepository financialPeriodRepository,
            IChartOfAccountRepository chartOfAccountRepository,
            IGeneralLedgerService generalLedgerService,
            ITrialBalanceService trialBalanceService,
            IFinancialStatementService financialStatementService)
        {
            _financialPeriodRepository = financialPeriodRepository ?? throw new ArgumentNullException(nameof(financialPeriodRepository));
            _chartOfAccountRepository = chartOfAccountRepository ?? throw new ArgumentNullException(nameof(chartOfAccountRepository));
            _generalLedgerService = generalLedgerService ?? throw new ArgumentNullException(nameof(generalLedgerService));
            _trialBalanceService = trialBalanceService ?? throw new ArgumentNullException(nameof(trialBalanceService));
            _financialStatementService = financialStatementService ?? throw new ArgumentNullException(nameof(financialStatementService));
        }
        
        /// <summary>
        /// Calculates key financial ratios for a specific financial period
        /// </summary>
        public async Task<FinancialRatiosDto> CalculateFinancialRatiosAsync(
            string financialPeriodId,
            bool includeComparativePeriod = false,
            CancellationToken cancellationToken = default)
        {
            // Get the financial period
            var financialPeriod = await _financialPeriodRepository.GetByIdAsync(financialPeriodId, cancellationToken);
            if (financialPeriod == null)
            {
                throw new InvalidOperationException($"Financial period with ID {financialPeriodId} not found");
            }
            
            // Get the financial statements
            var incomeStatement = await _financialStatementService.GenerateIncomeStatementAsync(
                financialPeriodId, 
                false, 
                false, 
                null, 
                cancellationToken);
                
            var balanceSheet = await _financialStatementService.GenerateBalanceSheetAsync(
                financialPeriodId, 
                false, 
                false, 
                null, 
                cancellationToken);
                
            // Initialize result
            var result = new FinancialRatiosDto
            {
                FinancialPeriodId = financialPeriodId,
                FinancialPeriodName = financialPeriod.Name,
                GeneratedAt = DateTime.UtcNow,
                StartDate = financialPeriod.StartDate,
                EndDate = financialPeriod.EndDate,
                LiquidityRatios = new LiquidityRatiosDto(),
                ProfitabilityRatios = new ProfitabilityRatiosDto(),
                SolvencyRatios = new SolvencyRatiosDto(),
                EfficiencyRatios = new EfficiencyRatiosDto(),
                ComparativePeriod = null
            };
            
            // Calculate liquidity ratios
            result.LiquidityRatios = await CalculateLiquidityRatiosAsync(balanceSheet, cancellationToken);
            
            // Calculate profitability ratios
            result.ProfitabilityRatios = CalculateProfitabilityRatios(incomeStatement, balanceSheet);
            
            // Calculate solvency ratios
            result.SolvencyRatios = CalculateSolvencyRatios(balanceSheet, incomeStatement);
            
            // Calculate efficiency ratios
            result.EfficiencyRatios = await CalculateEfficiencyRatiosAsync(
                incomeStatement, 
                balanceSheet, 
                financialPeriodId, 
                cancellationToken);
            
            // Handle comparative period if requested
            if (includeComparativePeriod)
            {
                // Get the previous financial period
                var previousPeriod = await _financialPeriodRepository.GetPreviousPeriodAsync(financialPeriodId, cancellationToken);
                if (previousPeriod != null)
                {
                    // Calculate ratios for the previous period
                    var previousRatios = await CalculateFinancialRatiosAsync(
                        previousPeriod.Id,
                        false, // Don't include comparative for the previous period
                        cancellationToken);
                        
                    // Create comparative data
                    result.ComparativePeriod = new ComparativeRatiosDto
                    {
                        FinancialPeriodId = previousPeriod.Id,
                        FinancialPeriodName = previousPeriod.Name,
                        StartDate = previousPeriod.StartDate,
                        EndDate = previousPeriod.EndDate,
                        
                        // Liquidity ratios
                        CurrentRatio = previousRatios.LiquidityRatios.CurrentRatio,
                        QuickRatio = previousRatios.LiquidityRatios.QuickRatio,
                        CashRatio = previousRatios.LiquidityRatios.CashRatio,
                        WorkingCapital = previousRatios.LiquidityRatios.WorkingCapital,
                        
                        // Profitability ratios
                        GrossProfitMargin = previousRatios.ProfitabilityRatios.GrossProfitMargin,
                        NetProfitMargin = previousRatios.ProfitabilityRatios.NetProfitMargin,
                        ReturnOnAssets = previousRatios.ProfitabilityRatios.ReturnOnAssets,
                        ReturnOnEquity = previousRatios.ProfitabilityRatios.ReturnOnEquity,
                        
                        // Solvency ratios
                        DebtToEquityRatio = previousRatios.SolvencyRatios.DebtToEquityRatio,
                        DebtToAssetsRatio = previousRatios.SolvencyRatios.DebtToAssetsRatio,
                        InterestCoverageRatio = previousRatios.SolvencyRatios.InterestCoverageRatio,
                        
                        // Efficiency ratios
                        AssetTurnoverRatio = previousRatios.EfficiencyRatios.AssetTurnoverRatio,
                        InventoryTurnoverRatio = previousRatios.EfficiencyRatios.InventoryTurnoverRatio,
                        ReceivablesTurnoverRatio = previousRatios.EfficiencyRatios.ReceivablesTurnoverRatio,
                        PayablesTurnoverRatio = previousRatios.EfficiencyRatios.PayablesTurnoverRatio,
                        
                        // Calculate changes
                        CurrentRatioChange = result.LiquidityRatios.CurrentRatio - previousRatios.LiquidityRatios.CurrentRatio,
                        QuickRatioChange = result.LiquidityRatios.QuickRatio - previousRatios.LiquidityRatios.QuickRatio,
                        CashRatioChange = result.LiquidityRatios.CashRatio - previousRatios.LiquidityRatios.CashRatio,
                        WorkingCapitalChange = result.LiquidityRatios.WorkingCapital - previousRatios.LiquidityRatios.WorkingCapital,
                        
                        GrossProfitMarginChange = result.ProfitabilityRatios.GrossProfitMargin - previousRatios.ProfitabilityRatios.GrossProfitMargin,
                        NetProfitMarginChange = result.ProfitabilityRatios.NetProfitMargin - previousRatios.ProfitabilityRatios.NetProfitMargin,
                        ReturnOnAssetsChange = result.ProfitabilityRatios.ReturnOnAssets - previousRatios.ProfitabilityRatios.ReturnOnAssets,
                        ReturnOnEquityChange = result.ProfitabilityRatios.ReturnOnEquity - previousRatios.ProfitabilityRatios.ReturnOnEquity,
                        
                        DebtToEquityRatioChange = result.SolvencyRatios.DebtToEquityRatio - previousRatios.SolvencyRatios.DebtToEquityRatio,
                        DebtToAssetsRatioChange = result.SolvencyRatios.DebtToAssetsRatio - previousRatios.SolvencyRatios.DebtToAssetsRatio,
                        InterestCoverageRatioChange = result.SolvencyRatios.InterestCoverageRatio - previousRatios.SolvencyRatios.InterestCoverageRatio,
                        
                        AssetTurnoverRatioChange = result.EfficiencyRatios.AssetTurnoverRatio - previousRatios.EfficiencyRatios.AssetTurnoverRatio,
                        InventoryTurnoverRatioChange = result.EfficiencyRatios.InventoryTurnoverRatio - previousRatios.EfficiencyRatios.InventoryTurnoverRatio,
                        ReceivablesTurnoverRatioChange = result.EfficiencyRatios.ReceivablesTurnoverRatio - previousRatios.EfficiencyRatios.ReceivablesTurnoverRatio,
                        PayablesTurnoverRatioChange = result.EfficiencyRatios.PayablesTurnoverRatio - previousRatios.EfficiencyRatios.PayablesTurnoverRatio
                    };
                }
            }
            
            return result;
        }

        /// <summary>
        /// Generates trend analysis for specified accounts or financial metrics over multiple periods
        /// </summary>
        public async Task<TrendAnalysisDto> GenerateTrendAnalysisAsync(
            int numberOfPeriods,
            string endPeriodId = null,
            string[] accountNumbers = null,
            string[] metrics = null,
            CancellationToken cancellationToken = default)
        {
            // Get the end period (current period) - if not specified, get the most recent period
            FinancialPeriod endPeriod;
            if (string.IsNullOrEmpty(endPeriodId))
            {
                endPeriod = await _financialPeriodRepository.GetCurrentActivePeriodAsync(cancellationToken);
                if (endPeriod == null)
                {
                    throw new InvalidOperationException("No active financial period found");
                }
            }
            else
            {
                endPeriod = await _financialPeriodRepository.GetByIdAsync(endPeriodId, cancellationToken);
                if (endPeriod == null)
                {
                    throw new InvalidOperationException($"Financial period with ID {endPeriodId} not found");
                }
            }
            
            // Get all the required periods (moving backwards from the end period)
            var periods = new List<FinancialPeriod> { endPeriod };
            var currentPeriod = endPeriod;
            
            for (int i = 1; i < numberOfPeriods; i++)
            {
                var previousPeriod = await _financialPeriodRepository.GetPreviousPeriodAsync(currentPeriod.Id, cancellationToken);
                if (previousPeriod == null)
                {
                    break; // No more previous periods
                }
                
                periods.Add(previousPeriod);
                currentPeriod = previousPeriod;
            }
            
            // Sort periods chronologically
            periods = periods.OrderBy(p => p.StartDate).ToList();
            
            // Initialize the result
            var result = new TrendAnalysisDto
            {
                GeneratedAt = DateTime.UtcNow,
                StartPeriodId = periods.First().Id,
                StartPeriodName = periods.First().Name,
                EndPeriodId = endPeriod.Id,
                EndPeriodName = endPeriod.Name,
                NumberOfPeriods = periods.Count,
                Periods = periods.Select(p => p.Name).ToArray(),
                AccountTrends = new List<AccountTrendDto>(),
                MetricTrends = new List<MetricTrendDto>()
            };
            
            // Process account trends if requested
            if (accountNumbers != null && accountNumbers.Length > 0)
            {
                foreach (var accountNumber in accountNumbers)
                {
                    var account = await _chartOfAccountRepository.GetByAccountNumberAsync(accountNumber, cancellationToken);
                    if (account == null)
                    {
                        continue; // Skip if account not found
                    }
                    
                    var accountTrend = new AccountTrendDto
                    {
                        AccountId = account.Id,
                        AccountNumber = account.AccountNumber,
                        AccountName = account.AccountName,
                        AccountClassification = account.Classification,
                        BalanceValues = new decimal[periods.Count]
                    };
                    
                    // Get balance for each period
                    for (int i = 0; i < periods.Count; i++)
                    {
                        var balance = await _generalLedgerService.GetAccountBalanceAsync(
                            account.Id, 
                            periods[i].Id, 
                            cancellationToken);
                            
                        accountTrend.BalanceValues[i] = balance.Balance;
                    }
                    
                    result.AccountTrends.Add(accountTrend);
                }
            }
            
            // Process metric trends if requested
            if (metrics != null && metrics.Length > 0)
            {
                foreach (var metric in metrics)
                {
                    var metricTrend = new MetricTrendDto
                    {
                        MetricName = metric,
                        MetricValues = new decimal[periods.Count]
                    };
                    
                    // Calculate metric for each period
                    for (int i = 0; i < periods.Count; i++)
                    {
                        decimal metricValue = 0;
                        
                        switch (metric.ToLowerInvariant())
                        {
                            case "netincome":
                                var incomeStatement = await _financialStatementService.GenerateIncomeStatementAsync(
                                    periods[i].Id, 
                                    false, 
                                    false, 
                                    null, 
                                    cancellationToken);
                                    
                                metricValue = incomeStatement.NetIncome;
                                break;
                                
                            case "totalassets":
                                var balanceSheet = await _financialStatementService.GenerateBalanceSheetAsync(
                                    periods[i].Id, 
                                    false, 
                                    false, 
                                    null, 
                                    cancellationToken);
                                    
                                metricValue = balanceSheet.TotalAssets;
                                break;
                                
                            case "totalliabilities":
                                var bs = await _financialStatementService.GenerateBalanceSheetAsync(
                                    periods[i].Id, 
                                    false, 
                                    false, 
                                    null, 
                                    cancellationToken);
                                    
                                metricValue = bs.Liabilities.Total;
                                break;
                                
                            case "totalequity":
                                var bsEquity = await _financialStatementService.GenerateBalanceSheetAsync(
                                    periods[i].Id, 
                                    false, 
                                    false, 
                                    null, 
                                    cancellationToken);
                                    
                                metricValue = bsEquity.Equity.Total;
                                break;
                                
                            case "grossprofit":
                                var is1 = await _financialStatementService.GenerateIncomeStatementAsync(
                                    periods[i].Id, 
                                    false, 
                                    false, 
                                    null, 
                                    cancellationToken);
                                    
                                metricValue = is1.GrossProfit;
                                break;
                                
                            case "totalrevenue":
                                var is2 = await _financialStatementService.GenerateIncomeStatementAsync(
                                    periods[i].Id, 
                                    false, 
                                    false, 
                                    null, 
                                    cancellationToken);
                                    
                                metricValue = is2.Income.Total;
                                break;
                                
                            case "totalexpenses":
                                var is3 = await _financialStatementService.GenerateIncomeStatementAsync(
                                    periods[i].Id, 
                                    false, 
                                    false, 
                                    null, 
                                    cancellationToken);
                                    
                                metricValue = is3.Expenses.Total;
                                break;
                                
                            case "currentratio":
                                var ratios = await CalculateFinancialRatiosAsync(periods[i].Id, false, cancellationToken);
                                metricValue = ratios.LiquidityRatios.CurrentRatio;
                                break;
                                
                            case "quickratio":
                                var ratios2 = await CalculateFinancialRatiosAsync(periods[i].Id, false, cancellationToken);
                                metricValue = ratios2.LiquidityRatios.QuickRatio;
                                break;
                                
                            case "roe":
                                var ratios3 = await CalculateFinancialRatiosAsync(periods[i].Id, false, cancellationToken);
                                metricValue = ratios3.ProfitabilityRatios.ReturnOnEquity;
                                break;
                                
                            case "roa":
                                var ratios4 = await CalculateFinancialRatiosAsync(periods[i].Id, false, cancellationToken);
                                metricValue = ratios4.ProfitabilityRatios.ReturnOnAssets;
                                break;
                                
                            default:
                                metricValue = 0; // Unknown metric
                                break;
                        }
                        
                        metricTrend.MetricValues[i] = metricValue;
                    }
                    
                    result.MetricTrends.Add(metricTrend);
                }
            }
            
            return result;
        }

        /// <summary>
        /// Calculates key financial KPIs for a specific financial period
        /// </summary>
        public async Task<FinancialKpiDto> CalculateFinancialKpisAsync(
            string financialPeriodId,
            bool includeComparativePeriod = false,
            CancellationToken cancellationToken = default)
        {
            // Get the financial period
            var financialPeriod = await _financialPeriodRepository.GetByIdAsync(financialPeriodId, cancellationToken);
            if (financialPeriod == null)
            {
                throw new InvalidOperationException($"Financial period with ID {financialPeriodId} not found");
            }
            
            // Get the financial statements
            var incomeStatement = await _financialStatementService.GenerateIncomeStatementAsync(
                financialPeriodId, 
                false, 
                false, 
                null, 
                cancellationToken);
                
            var balanceSheet = await _financialStatementService.GenerateBalanceSheetAsync(
                financialPeriodId, 
                false, 
                false, 
                null, 
                cancellationToken);
                
            // Get financial ratios
            var ratios = await CalculateFinancialRatiosAsync(financialPeriodId, false, cancellationToken);
            
            // Initialize result
            var result = new FinancialKpiDto
            {
                FinancialPeriodId = financialPeriodId,
                FinancialPeriodName = financialPeriod.Name,
                GeneratedAt = DateTime.UtcNow,
                StartDate = financialPeriod.StartDate,
                EndDate = financialPeriod.EndDate,
                KeyPerformanceIndicators = new List<KpiItemDto>(),
                ComparativePeriod = null
            };
            
            // Add KPIs
            result.KeyPerformanceIndicators.Add(new KpiItemDto
            {
                Name = "Net Income",
                Value = incomeStatement.NetIncome,
                Format = "Currency",
                Category = "Profitability",
                Description = "Total profit or loss for the period"
            });
            
            result.KeyPerformanceIndicators.Add(new KpiItemDto
            {
                Name = "Gross Profit Margin",
                Value = ratios.ProfitabilityRatios.GrossProfitMargin * 100, // Convert to percentage
                Format = "Percentage",
                Category = "Profitability",
                Description = "Gross profit as a percentage of revenue"
            });
            
            result.KeyPerformanceIndicators.Add(new KpiItemDto
            {
                Name = "Net Profit Margin",
                Value = ratios.ProfitabilityRatios.NetProfitMargin * 100, // Convert to percentage
                Format = "Percentage",
                Category = "Profitability",
                Description = "Net profit as a percentage of revenue"
            });
            
            result.KeyPerformanceIndicators.Add(new KpiItemDto
            {
                Name = "Return on Equity (ROE)",
                Value = ratios.ProfitabilityRatios.ReturnOnEquity * 100, // Convert to percentage
                Format = "Percentage",
                Category = "Profitability",
                Description = "Net income as a percentage of average shareholders' equity"
            });
            
            result.KeyPerformanceIndicators.Add(new KpiItemDto
            {
                Name = "Return on Assets (ROA)",
                Value = ratios.ProfitabilityRatios.ReturnOnAssets * 100, // Convert to percentage
                Format = "Percentage",
                Category = "Profitability",
                Description = "Net income as a percentage of average total assets"
            });
            
            result.KeyPerformanceIndicators.Add(new KpiItemDto
            {
                Name = "Current Ratio",
                Value = ratios.LiquidityRatios.CurrentRatio,
                Format = "Decimal",
                Category = "Liquidity",
                Description = "Current assets divided by current liabilities"
            });
            
            result.KeyPerformanceIndicators.Add(new KpiItemDto
            {
                Name = "Quick Ratio",
                Value = ratios.LiquidityRatios.QuickRatio,
                Format = "Decimal",
                Category = "Liquidity",
                Description = "Quick assets divided by current liabilities"
            });
            
            result.KeyPerformanceIndicators.Add(new KpiItemDto
            {
                Name = "Working Capital",
                Value = ratios.LiquidityRatios.WorkingCapital,
                Format = "Currency",
                Category = "Liquidity",
                Description = "Current assets minus current liabilities"
            });
            
            result.KeyPerformanceIndicators.Add(new KpiItemDto
            {
                Name = "Debt to Equity",
                Value = ratios.SolvencyRatios.DebtToEquityRatio,
                Format = "Decimal",
                Category = "Solvency",
                Description = "Total liabilities divided by total equity"
            });
            
            result.KeyPerformanceIndicators.Add(new KpiItemDto
            {
                Name = "Debt to Assets",
                Value = ratios.SolvencyRatios.DebtToAssetsRatio * 100, // Convert to percentage
                Format = "Percentage",
                Category = "Solvency",
                Description = "Total liabilities as a percentage of total assets"
            });
            
            result.KeyPerformanceIndicators.Add(new KpiItemDto
            {
                Name = "Asset Turnover",
                Value = ratios.EfficiencyRatios.AssetTurnoverRatio,
                Format = "Decimal",
                Category = "Efficiency",
                Description = "Revenue divided by average total assets"
            });
            
            result.KeyPerformanceIndicators.Add(new KpiItemDto
            {
                Name = "Total Revenue",
                Value = incomeStatement.Income.Total,
                Format = "Currency",
                Category = "Growth",
                Description = "Total revenue for the period"
            });
            
            result.KeyPerformanceIndicators.Add(new KpiItemDto
            {
                Name = "Total Assets",
                Value = balanceSheet.TotalAssets,
                Format = "Currency",
                Category = "Size",
                Description = "Total assets at the end of the period"
            });
            
            // Handle comparative period if requested
            if (includeComparativePeriod)
            {
                // Get the previous financial period
                var previousPeriod = await _financialPeriodRepository.GetPreviousPeriodAsync(financialPeriodId, cancellationToken);
                if (previousPeriod != null)
                {
                    // Calculate KPIs for the previous period
                    var previousKpis = await CalculateFinancialKpisAsync(
                        previousPeriod.Id,
                        false, // Don't include comparative for the previous period
                        cancellationToken);
                        
                    // Create comparative data
                    result.ComparativePeriod = new ComparativeKpiDto
                    {
                        FinancialPeriodId = previousPeriod.Id,
                        FinancialPeriodName = previousPeriod.Name,
                        StartDate = previousPeriod.StartDate,
                        EndDate = previousPeriod.EndDate,
                        KeyPerformanceIndicators = previousKpis.KeyPerformanceIndicators,
                        Changes = new List<KpiChangeDto>()
                    };
                    
                    // Calculate changes
                    foreach (var currentKpi in result.KeyPerformanceIndicators)
                    {
                        var previousKpi = previousKpis.KeyPerformanceIndicators
                            .FirstOrDefault(k => k.Name == currentKpi.Name);
                            
                        if (previousKpi != null)
                        {
                            decimal change = currentKpi.Value - previousKpi.Value;
                            decimal percentChange = 0;
                            
                            if (previousKpi.Value != 0)
                            {
                                percentChange = (change / Math.Abs(previousKpi.Value)) * 100;
                            }
                            
                            result.ComparativePeriod.Changes.Add(new KpiChangeDto
                            {
                                Name = currentKpi.Name,
                                PreviousValue = previousKpi.Value,
                                CurrentValue = currentKpi.Value,
                                AbsoluteChange = change,
                                PercentageChange = percentChange,
                                Format = currentKpi.Format,
                                Category = currentKpi.Category
                            });
                        }
                    }
                }
            }
            
            return result;
        }

        /// <summary>
        /// Generates a profitability summary by segment (branch, product, customer type, etc.)
        /// </summary>
        public async Task<ProfitabilitySummaryDto> GenerateProfitabilitySummaryAsync(
            string financialPeriodId,
            string segmentType = null,
            CancellationToken cancellationToken = default)
        {
            // Get the financial period
            var financialPeriod = await _financialPeriodRepository.GetByIdAsync(financialPeriodId, cancellationToken);
            if (financialPeriod == null)
            {
                throw new InvalidOperationException($"Financial period with ID {financialPeriodId} not found");
            }
            
            // Initialize result
            var result = new ProfitabilitySummaryDto
            {
                FinancialPeriodId = financialPeriodId,
                FinancialPeriodName = financialPeriod.Name,
                GeneratedAt = DateTime.UtcNow,
                StartDate = financialPeriod.StartDate,
                EndDate = financialPeriod.EndDate,
                SegmentType = segmentType ?? "Branch", // Default to branch
                Segments = new List<ProfitabilitySegmentDto>()
            };
            
            // Get the income statement for the full period
            var overallIncomeStatement = await _financialStatementService.GenerateIncomeStatementAsync(
                financialPeriodId, 
                false, 
                false, 
                null, 
                cancellationToken);
                
            // For now, we'll implement a simplified version with mock segment data
            // In a real implementation, you would need to have segment data in your chart of accounts 
            // or journal entries, and filter/aggregate based on that
            
            // For branches (example)
            if (segmentType == "Branch" || segmentType == null)
            {
                // Mock branch data - in a real implementation, you would get this from your database
                var branches = new[] 
                { 
                    "Main Branch", 
                    "North Branch", 
                    "South Branch", 
                    "East Branch", 
                    "West Branch" 
                };
                
                Random random = new Random(123); // Fixed seed for consistent demo data
                
                foreach (var branch in branches)
                {
                    // Generate mock data that's proportional to the overall results
                    decimal revenueFactor = (decimal)(0.05 + random.NextDouble() * 0.3); // 5% to 35% of total
                    decimal expenseFactor = (decimal)(0.05 + random.NextDouble() * 0.3); // 5% to 35% of total
                    
                    decimal branchRevenue = overallIncomeStatement.Income.Total * revenueFactor;
                    decimal branchExpenses = overallIncomeStatement.Expenses.Total * expenseFactor;
                    decimal branchNetIncome = branchRevenue - branchExpenses;
                    decimal branchNetProfitMargin = branchRevenue != 0 ? branchNetIncome / branchRevenue : 0;
                    
                    var segment = new ProfitabilitySegmentDto
                    {
                        SegmentName = branch,
                        SegmentCode = branch.Replace(" ", "").ToUpper(), // Simple code
                        Revenue = branchRevenue,
                        Expenses = branchExpenses,
                        NetIncome = branchNetIncome,
                        NetProfitMargin = branchNetProfitMargin,
                        ContributionToTotalRevenue = overallIncomeStatement.Income.Total != 0 
                            ? branchRevenue / overallIncomeStatement.Income.Total 
                            : 0,
                        ContributionToTotalProfit = overallIncomeStatement.NetIncome != 0 
                            ? branchNetIncome / overallIncomeStatement.NetIncome 
                            : 0
                    };
                    
                    result.Segments.Add(segment);
                }
                
                // Ensure the sum of segments matches the overall totals (adjust the last segment)
                decimal totalSegmentRevenue = result.Segments.Sum(s => s.Revenue);
                decimal totalSegmentExpenses = result.Segments.Sum(s => s.Expenses);
                
                if (result.Segments.Any())
                {
                    var lastSegment = result.Segments.Last();
                    lastSegment.Revenue += overallIncomeStatement.Income.Total - totalSegmentRevenue;
                    lastSegment.Expenses += overallIncomeStatement.Expenses.Total - totalSegmentExpenses;
                    lastSegment.NetIncome = lastSegment.Revenue - lastSegment.Expenses;
                    lastSegment.NetProfitMargin = lastSegment.Revenue != 0 ? lastSegment.NetIncome / lastSegment.Revenue : 0;
                    lastSegment.ContributionToTotalRevenue = overallIncomeStatement.Income.Total != 0 
                        ? lastSegment.Revenue / overallIncomeStatement.Income.Total 
                        : 0;
                    lastSegment.ContributionToTotalProfit = overallIncomeStatement.NetIncome != 0 
                        ? lastSegment.NetIncome / overallIncomeStatement.NetIncome 
                        : 0;
                }
            }
            
            // Calculate overall totals
            result.TotalRevenue = result.Segments.Sum(s => s.Revenue);
            result.TotalExpenses = result.Segments.Sum(s => s.Expenses);
            result.TotalNetIncome = result.Segments.Sum(s => s.NetIncome);
            result.OverallNetProfitMargin = result.TotalRevenue != 0 ? result.TotalNetIncome / result.TotalRevenue : 0;
            
            return result;
        }
        
        #region Private Helper Methods
        
        /// <summary>
        /// Calculates liquidity ratios from a balance sheet
        /// </summary>
        private async Task<LiquidityRatiosDto> CalculateLiquidityRatiosAsync(
            BalanceSheetDto balanceSheet, 
            CancellationToken cancellationToken)
        {
            // Get current assets and current liabilities
            var currentAssets = await GetCurrentAssetsAsync(balanceSheet, cancellationToken);
            var currentLiabilities = await GetCurrentLiabilitiesAsync(balanceSheet, cancellationToken);
            
            // Get quick assets (current assets - inventory)
            var inventory = await GetInventoryAsync(balanceSheet, cancellationToken);
            decimal quickAssets = currentAssets - inventory;
            
            // Get cash and cash equivalents
            var cashAndEquivalents = await GetCashAndEquivalentsAsync(balanceSheet, cancellationToken);
            
            // Calculate ratios
            decimal currentRatio = currentLiabilities != 0 ? currentAssets / currentLiabilities : 0;
            decimal quickRatio = currentLiabilities != 0 ? quickAssets / currentLiabilities : 0;
            decimal cashRatio = currentLiabilities != 0 ? cashAndEquivalents / currentLiabilities : 0;
            decimal workingCapital = currentAssets - currentLiabilities;
            
            return new LiquidityRatiosDto
            {
                CurrentRatio = currentRatio,
                QuickRatio = quickRatio,
                CashRatio = cashRatio,
                WorkingCapital = workingCapital,
                CurrentAssets = currentAssets,
                CurrentLiabilities = currentLiabilities,
                QuickAssets = quickAssets,
                CashAndEquivalents = cashAndEquivalents
            };
        }
        
        /// <summary>
        /// Calculates profitability ratios from income statement and balance sheet
        /// </summary>
        private ProfitabilityRatiosDto CalculateProfitabilityRatios(
            IncomeStatementDto incomeStatement, 
            BalanceSheetDto balanceSheet)
        {
            // Extract necessary values
            decimal revenue = incomeStatement.Income.Total;
            decimal grossProfit = incomeStatement.GrossProfit;
            decimal netIncome = incomeStatement.NetIncome;
            decimal totalAssets = balanceSheet.TotalAssets;
            decimal totalEquity = balanceSheet.Equity.Total;
            
            // Calculate ratios
            decimal grossProfitMargin = revenue != 0 ? grossProfit / revenue : 0;
            decimal netProfitMargin = revenue != 0 ? netIncome / revenue : 0;
            decimal returnOnAssets = totalAssets != 0 ? netIncome / totalAssets : 0;
            decimal returnOnEquity = totalEquity != 0 ? netIncome / totalEquity : 0;
            
            return new ProfitabilityRatiosDto
            {
                GrossProfitMargin = grossProfitMargin,
                NetProfitMargin = netProfitMargin,
                ReturnOnAssets = returnOnAssets,
                ReturnOnEquity = returnOnEquity,
                Revenue = revenue,
                GrossProfit = grossProfit,
                NetIncome = netIncome,
                TotalAssets = totalAssets,
                TotalEquity = totalEquity
            };
        }
        
        /// <summary>
        /// Calculates solvency ratios from balance sheet and income statement
        /// </summary>
        private SolvencyRatiosDto CalculateSolvencyRatios(
            BalanceSheetDto balanceSheet, 
            IncomeStatementDto incomeStatement)
        {
            // Extract necessary values
            decimal totalLiabilities = balanceSheet.Liabilities.Total;
            decimal totalAssets = balanceSheet.TotalAssets;
            decimal totalEquity = balanceSheet.Equity.Total;
            decimal netIncome = incomeStatement.NetIncome;
            
            // For interest coverage ratio, we need interest expense
            // This is a simplified approach - in a real system, you would extract the actual interest expense
            // from a specific account in the income statement
            decimal interestExpense = 0;
            var interestExpenseAccount = incomeStatement.Expenses.Accounts
                .FirstOrDefault(a => a.AccountName.Contains("Interest"));
                
            if (interestExpenseAccount != null)
            {
                interestExpense = interestExpenseAccount.Amount;
            }
            
            // Calculate ratios
            decimal debtToEquityRatio = totalEquity != 0 ? totalLiabilities / totalEquity : 0;
            decimal debtToAssetsRatio = totalAssets != 0 ? totalLiabilities / totalAssets : 0;
            decimal interestCoverageRatio = interestExpense != 0 ? (netIncome + interestExpense) / interestExpense : 0;
            
            return new SolvencyRatiosDto
            {
                DebtToEquityRatio = debtToEquityRatio,
                DebtToAssetsRatio = debtToAssetsRatio,
                InterestCoverageRatio = interestCoverageRatio,
                TotalLiabilities = totalLiabilities,
                TotalEquity = totalEquity,
                TotalAssets = totalAssets,
                InterestExpense = interestExpense
            };
        }
        
        /// <summary>
        /// Calculates efficiency ratios from income statement and balance sheet
        /// </summary>
        private async Task<EfficiencyRatiosDto> CalculateEfficiencyRatiosAsync(
            IncomeStatementDto incomeStatement, 
            BalanceSheetDto balanceSheet,
            string financialPeriodId,
            CancellationToken cancellationToken)
        {
            // Extract necessary values
            decimal revenue = incomeStatement.Income.Total;
            decimal totalAssets = balanceSheet.TotalAssets;
            
            // Get inventory
            decimal inventory = await GetInventoryAsync(balanceSheet, cancellationToken);
            
            // Get accounts receivable
            decimal accountsReceivable = await GetAccountsReceivableAsync(balanceSheet, cancellationToken);
            
            // Get accounts payable
            decimal accountsPayable = await GetAccountsPayableAsync(balanceSheet, cancellationToken);
            
            // Get cost of goods sold (simplified approach)
            decimal costOfGoodsSold = revenue - incomeStatement.GrossProfit;
            
            // Calculate ratios
            decimal assetTurnoverRatio = totalAssets != 0 ? revenue / totalAssets : 0;
            decimal inventoryTurnoverRatio = inventory != 0 ? costOfGoodsSold / inventory : 0;
            decimal receivablesTurnoverRatio = accountsReceivable != 0 ? revenue / accountsReceivable : 0;
            decimal payablesTurnoverRatio = accountsPayable != 0 ? costOfGoodsSold / accountsPayable : 0;
            
            return new EfficiencyRatiosDto
            {
                AssetTurnoverRatio = assetTurnoverRatio,
                InventoryTurnoverRatio = inventoryTurnoverRatio,
                ReceivablesTurnoverRatio = receivablesTurnoverRatio,
                PayablesTurnoverRatio = payablesTurnoverRatio,
                Revenue = revenue,
                TotalAssets = totalAssets,
                Inventory = inventory,
                AccountsReceivable = accountsReceivable,
                AccountsPayable = accountsPayable,
                CostOfGoodsSold = costOfGoodsSold
            };
        }
        
        /// <summary>
        /// Gets the total value of current assets from a balance sheet
        /// </summary>
        private async Task<decimal> GetCurrentAssetsAsync(
            BalanceSheetDto balanceSheet, 
            CancellationToken cancellationToken)
        {
            // In a real implementation, you would identify current assets by account classification
            // For now, we'll use a simple approach of looking at account names or numbers
            
            decimal currentAssets = 0;
            
            foreach (var asset in balanceSheet.Assets.Accounts)
            {
                // Assuming account numbers follow a convention where current assets are in a specific range
                // or have specific keywords in their names
                if (asset.AccountNumber.StartsWith("1") && int.Parse(asset.AccountNumber) < 1500 ||
                    asset.AccountName.Contains("Current") ||
                    asset.AccountName.Contains("Cash") ||
                    asset.AccountName.Contains("Receivable") ||
                    asset.AccountName.Contains("Inventory"))
                {
                    currentAssets += asset.Amount;
                }
            }
            
            return currentAssets;
        }
        
        /// <summary>
        /// Gets the total value of current liabilities from a balance sheet
        /// </summary>
        private async Task<decimal> GetCurrentLiabilitiesAsync(
            BalanceSheetDto balanceSheet, 
            CancellationToken cancellationToken)
        {
            // Similar approach as with current assets
            decimal currentLiabilities = 0;
            
            foreach (var liability in balanceSheet.Liabilities.Accounts)
            {
                if (liability.AccountNumber.StartsWith("2") && int.Parse(liability.AccountNumber) < 2500 ||
                    liability.AccountName.Contains("Current") ||
                    liability.AccountName.Contains("Payable") ||
                    liability.AccountName.Contains("Short-term"))
                {
                    currentLiabilities += liability.Amount;
                }
            }
            
            return currentLiabilities;
        }
        
        /// <summary>
        /// Gets the total value of inventory from a balance sheet
        /// </summary>
        private async Task<decimal> GetInventoryAsync(
            BalanceSheetDto balanceSheet, 
            CancellationToken cancellationToken)
        {
            decimal inventory = 0;
            
            foreach (var asset in balanceSheet.Assets.Accounts)
            {
                if (asset.AccountName.Contains("Inventory"))
                {
                    inventory += asset.Amount;
                }
            }
            
            return inventory;
        }
        
        /// <summary>
        /// Gets the total value of cash and cash equivalents from a balance sheet
        /// </summary>
        private async Task<decimal> GetCashAndEquivalentsAsync(
            BalanceSheetDto balanceSheet, 
            CancellationToken cancellationToken)
        {
            decimal cashAndEquivalents = 0;
            
            foreach (var asset in balanceSheet.Assets.Accounts)
            {
                if (asset.AccountName.Contains("Cash") || 
                    asset.AccountName.Contains("Bank") ||
                    asset.AccountName.Contains("Money Market"))
                {
                    cashAndEquivalents += asset.Amount;
                }
            }
            
            return cashAndEquivalents;
        }
        
        /// <summary>
        /// Gets the total value of accounts receivable from a balance sheet
        /// </summary>
        private async Task<decimal> GetAccountsReceivableAsync(
            BalanceSheetDto balanceSheet, 
            CancellationToken cancellationToken)
        {
            decimal accountsReceivable = 0;
            
            foreach (var asset in balanceSheet.Assets.Accounts)
            {
                if (asset.AccountName.Contains("Receivable"))
                {
                    accountsReceivable += asset.Amount;
                }
            }
            
            return accountsReceivable;
        }
        
        /// <summary>
        /// Gets the total value of accounts payable from a balance sheet
        /// </summary>
        private async Task<decimal> GetAccountsPayableAsync(
            BalanceSheetDto balanceSheet, 
            CancellationToken cancellationToken)
        {
            decimal accountsPayable = 0;
            
            foreach (var liability in balanceSheet.Liabilities.Accounts)
            {
                if (liability.AccountName.Contains("Payable") && !liability.AccountName.Contains("Note"))
                {
                    accountsPayable += liability.Amount;
                }
            }
            
            return accountsPayable;
        }
        
        #endregion
    }
    
    #region DTOs
    
    public class FinancialRatiosDto
    {
        public string FinancialPeriodId { get; set; }
        public string FinancialPeriodName { get; set; }
        public DateTime GeneratedAt { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public LiquidityRatiosDto LiquidityRatios { get; set; }
        public ProfitabilityRatiosDto ProfitabilityRatios { get; set; }
        public SolvencyRatiosDto SolvencyRatios { get; set; }
        public EfficiencyRatiosDto EfficiencyRatios { get; set; }
        public ComparativeRatiosDto ComparativePeriod { get; set; }
    }
    
    public class LiquidityRatiosDto
    {
        public decimal CurrentRatio { get; set; }
        public decimal QuickRatio { get; set; }
        public decimal CashRatio { get; set; }
        public decimal WorkingCapital { get; set; }
        public decimal CurrentAssets { get; set; }
        public decimal CurrentLiabilities { get; set; }
        public decimal QuickAssets { get; set; }
        public decimal CashAndEquivalents { get; set; }
    }
    
    public class ProfitabilityRatiosDto
    {
        public decimal GrossProfitMargin { get; set; }
        public decimal NetProfitMargin { get; set; }
        public decimal ReturnOnAssets { get; set; }
        public decimal ReturnOnEquity { get; set; }
        public decimal Revenue { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal NetIncome { get; set; }
        public decimal TotalAssets { get; set; }
        public decimal TotalEquity { get; set; }
    }
    
    public class SolvencyRatiosDto
    {
        public decimal DebtToEquityRatio { get; set; }
        public decimal DebtToAssetsRatio { get; set; }
        public decimal InterestCoverageRatio { get; set; }
        public decimal TotalLiabilities { get; set; }
        public decimal TotalEquity { get; set; }
        public decimal TotalAssets { get; set; }
        public decimal InterestExpense { get; set; }
    }
    
    public class EfficiencyRatiosDto
    {
        public decimal AssetTurnoverRatio { get; set; }
        public decimal InventoryTurnoverRatio { get; set; }
        public decimal ReceivablesTurnoverRatio { get; set; }
        public decimal PayablesTurnoverRatio { get; set; }
        public decimal Revenue { get; set; }
        public decimal TotalAssets { get; set; }
        public decimal Inventory { get; set; }
        public decimal AccountsReceivable { get; set; }
        public decimal AccountsPayable { get; set; }
        public decimal CostOfGoodsSold { get; set; }
    }
    
    public class ComparativeRatiosDto
    {
        public string FinancialPeriodId { get; set; }
        public string FinancialPeriodName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        // Liquidity ratios
        public decimal CurrentRatio { get; set; }
        public decimal QuickRatio { get; set; }
        public decimal CashRatio { get; set; }
        public decimal WorkingCapital { get; set; }
        
        // Profitability ratios
        public decimal GrossProfitMargin { get; set; }
        public decimal NetProfitMargin { get; set; }
        public decimal ReturnOnAssets { get; set; }
        public decimal ReturnOnEquity { get; set; }
        
        // Solvency ratios
        public decimal DebtToEquityRatio { get; set; }
        public decimal DebtToAssetsRatio { get; set; }
        public decimal InterestCoverageRatio { get; set; }
        
        // Efficiency ratios
        public decimal AssetTurnoverRatio { get; set; }
        public decimal InventoryTurnoverRatio { get; set; }
        public decimal ReceivablesTurnoverRatio { get; set; }
        public decimal PayablesTurnoverRatio { get; set; }
        
        // Changes
        public decimal CurrentRatioChange { get; set; }
        public decimal QuickRatioChange { get; set; }
        public decimal CashRatioChange { get; set; }
        public decimal WorkingCapitalChange { get; set; }
        
        public decimal GrossProfitMarginChange { get; set; }
        public decimal NetProfitMarginChange { get; set; }
        public decimal ReturnOnAssetsChange { get; set; }
        public decimal ReturnOnEquityChange { get; set; }
        
        public decimal DebtToEquityRatioChange { get; set; }
        public decimal DebtToAssetsRatioChange { get; set; }
        public decimal InterestCoverageRatioChange { get; set; }
        
        public decimal AssetTurnoverRatioChange { get; set; }
        public decimal InventoryTurnoverRatioChange { get; set; }
        public decimal ReceivablesTurnoverRatioChange { get; set; }
        public decimal PayablesTurnoverRatioChange { get; set; }
    }
    
    public class TrendAnalysisDto
    {
        public DateTime GeneratedAt { get; set; }
        public string StartPeriodId { get; set; }
        public string StartPeriodName { get; set; }
        public string EndPeriodId { get; set; }
        public string EndPeriodName { get; set; }
        public int NumberOfPeriods { get; set; }
        public string[] Periods { get; set; }
        public List<AccountTrendDto> AccountTrends { get; set; }
        public List<MetricTrendDto> MetricTrends { get; set; }
    }
    
    public class AccountTrendDto
    {
        public string AccountId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string AccountClassification { get; set; }
        public decimal[] BalanceValues { get; set; }
    }
    
    public class MetricTrendDto
    {
        public string MetricName { get; set; }
        public decimal[] MetricValues { get; set; }
    }
    
    public class FinancialKpiDto
    {
        public string FinancialPeriodId { get; set; }
        public string FinancialPeriodName { get; set; }
        public DateTime GeneratedAt { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<KpiItemDto> KeyPerformanceIndicators { get; set; }
        public ComparativeKpiDto ComparativePeriod { get; set; }
    }
    
    public class KpiItemDto
    {
        public string Name { get; set; }
        public decimal Value { get; set; }
        public string Format { get; set; } // "Currency", "Percentage", "Decimal", etc.
        public string Category { get; set; } // "Profitability", "Liquidity", "Solvency", "Efficiency", etc.
        public string Description { get; set; }
    }
    
    public class ComparativeKpiDto
    {
        public string FinancialPeriodId { get; set; }
        public string FinancialPeriodName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<KpiItemDto> KeyPerformanceIndicators { get; set; }
        public List<KpiChangeDto> Changes { get; set; }
    }
    
    public class KpiChangeDto
    {
        public string Name { get; set; }
        public decimal PreviousValue { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal AbsoluteChange { get; set; }
        public decimal PercentageChange { get; set; }
        public string Format { get; set; }
        public string Category { get; set; }
    }
    
    public class ProfitabilitySummaryDto
    {
        public string FinancialPeriodId { get; set; }
        public string FinancialPeriodName { get; set; }
        public DateTime GeneratedAt { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string SegmentType { get; set; } // "Branch", "Product", "Customer", etc.
        public List<ProfitabilitySegmentDto> Segments { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal TotalNetIncome { get; set; }
        public decimal OverallNetProfitMargin { get; set; }
    }
    
    public class ProfitabilitySegmentDto
    {
        public string SegmentName { get; set; }
        public string SegmentCode { get; set; }
        public decimal Revenue { get; set; }
        public decimal Expenses { get; set; }
        public decimal NetIncome { get; set; }
        public decimal NetProfitMargin { get; set; }
        public decimal ContributionToTotalRevenue { get; set; } // As a decimal (0.25 = 25%)
        public decimal ContributionToTotalProfit { get; set; } // As a decimal (0.25 = 25%)
    }
    
    #endregion
}