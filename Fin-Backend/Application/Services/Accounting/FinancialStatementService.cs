using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Domain.Entities.Accounting;
using FinTech.Domain.Repositories.Accounting;

namespace FinTech.Application.Services.Accounting
{
    public interface IFinancialStatementService
    {
        Task<IncomeStatementDto> GenerateIncomeStatementAsync(
            string financialPeriodId, 
            bool includeZeroBalances = false, 
            bool includeComparativePeriod = false,
            string currencyCode = null,
            CancellationToken cancellationToken = default);
            
        Task<BalanceSheetDto> GenerateBalanceSheetAsync(
            string financialPeriodId, 
            bool includeZeroBalances = false, 
            bool includeComparativePeriod = false,
            string currencyCode = null,
            CancellationToken cancellationToken = default);
            
        Task<CashFlowStatementDto> GenerateCashFlowStatementAsync(
            string financialPeriodId, 
            bool includeZeroBalances = false, 
            bool includeComparativePeriod = false,
            string currencyCode = null,
            CancellationToken cancellationToken = default);
            
        Task<EquityChangeStatementDto> GenerateEquityChangeStatementAsync(
            string financialPeriodId, 
            bool includeZeroBalances = false, 
            bool includeComparativePeriod = false,
            string currencyCode = null,
            CancellationToken cancellationToken = default);
    }

    public class FinancialStatementService : IFinancialStatementService
    {
        private readonly IChartOfAccountRepository _chartOfAccountRepository;
        private readonly IFinancialPeriodRepository _financialPeriodRepository;
        private readonly IJournalEntryRepository _journalEntryRepository;
        private readonly IGeneralLedgerService _generalLedgerService;
        private readonly ITrialBalanceService _trialBalanceService;
        
        public FinancialStatementService(
            IChartOfAccountRepository chartOfAccountRepository,
            IFinancialPeriodRepository financialPeriodRepository,
            IJournalEntryRepository journalEntryRepository,
            IGeneralLedgerService generalLedgerService,
            ITrialBalanceService trialBalanceService)
        {
            _chartOfAccountRepository = chartOfAccountRepository ?? throw new ArgumentNullException(nameof(chartOfAccountRepository));
            _financialPeriodRepository = financialPeriodRepository ?? throw new ArgumentNullException(nameof(financialPeriodRepository));
            _journalEntryRepository = journalEntryRepository ?? throw new ArgumentNullException(nameof(journalEntryRepository));
            _generalLedgerService = generalLedgerService ?? throw new ArgumentNullException(nameof(generalLedgerService));
            _trialBalanceService = trialBalanceService ?? throw new ArgumentNullException(nameof(trialBalanceService));
        }

        /// <summary>
        /// Generates an income statement (profit and loss) for a specific financial period
        /// </summary>
        public async Task<IncomeStatementDto> GenerateIncomeStatementAsync(
            string financialPeriodId, 
            bool includeZeroBalances = false, 
            bool includeComparativePeriod = false,
            string currencyCode = null,
            CancellationToken cancellationToken = default)
        {
            // Get the financial period
            var financialPeriod = await _financialPeriodRepository.GetByIdAsync(financialPeriodId, cancellationToken);
            if (financialPeriod == null)
            {
                throw new InvalidOperationException($"Financial period with ID {financialPeriodId} not found");
            }
            
            // Get the trial balance for this period
            var trialBalance = await _trialBalanceService.GenerateTrialBalanceByFinancialPeriodAsync(
                financialPeriodId, 
                true, // Include zero balances for the calculation
                currencyCode,
                cancellationToken);
            
            // Get income and expense accounts from the trial balance
            var incomeAccounts = trialBalance.Accounts
                .Where(a => a.Classification == "Income")
                .ToList();
                
            var expenseAccounts = trialBalance.Accounts
                .Where(a => a.Classification == "Expense")
                .ToList();
            
            // Calculate totals
            decimal totalIncome = incomeAccounts.Sum(a => a.CreditBalance - a.DebitBalance);
            decimal totalExpenses = expenseAccounts.Sum(a => a.DebitBalance - a.CreditBalance);
            decimal netIncome = totalIncome - totalExpenses;
            
            // Prepare income statement sections
            var incomeSection = new FinancialStatementSectionDto
            {
                Title = "Income",
                Accounts = incomeAccounts
                    .Where(a => includeZeroBalances || (a.CreditBalance != 0 || a.DebitBalance != 0))
                    .Select(a => new FinancialStatementAccountDto
                    {
                        AccountId = a.AccountId,
                        AccountNumber = a.AccountNumber,
                        AccountName = a.AccountName,
                        Amount = a.CreditBalance - a.DebitBalance, // Net income amount
                        IsCredit = true, // Income is normally credit
                        CurrencyCode = a.CurrencyCode,
                        SubAccounts = new List<FinancialStatementAccountDto>()
                    })
                    .OrderBy(a => a.AccountNumber)
                    .ToList(),
                Total = totalIncome
            };
            
            var expenseSection = new FinancialStatementSectionDto
            {
                Title = "Expenses",
                Accounts = expenseAccounts
                    .Where(a => includeZeroBalances || (a.CreditBalance != 0 || a.DebitBalance != 0))
                    .Select(a => new FinancialStatementAccountDto
                    {
                        AccountId = a.AccountId,
                        AccountNumber = a.AccountNumber,
                        AccountName = a.AccountName,
                        Amount = a.DebitBalance - a.CreditBalance, // Net expense amount
                        IsCredit = false, // Expense is normally debit
                        CurrencyCode = a.CurrencyCode,
                        SubAccounts = new List<FinancialStatementAccountDto>()
                    })
                    .OrderBy(a => a.AccountNumber)
                    .ToList(),
                Total = totalExpenses
            };
            
            // Create the income statement
            var result = new IncomeStatementDto
            {
                FinancialPeriodId = financialPeriodId,
                FinancialPeriodName = financialPeriod.Name,
                StartDate = financialPeriod.StartDate,
                EndDate = financialPeriod.EndDate,
                GeneratedAt = DateTime.UtcNow,
                CurrencyCode = currencyCode ?? "NGN",
                Income = incomeSection,
                Expenses = expenseSection,
                GrossProfit = totalIncome,
                TotalExpenses = totalExpenses,
                NetIncome = netIncome
            };
            
            // Handle comparative period if requested
            if (includeComparativePeriod)
            {
                // Get the previous financial period
                var previousPeriod = await _financialPeriodRepository.GetPreviousPeriodAsync(financialPeriodId, cancellationToken);
                if (previousPeriod != null)
                {
                    // Generate income statement for the previous period
                    var previousIncomeStatement = await GenerateIncomeStatementAsync(
                        previousPeriod.Id,
                        includeZeroBalances,
                        false, // Don't include comparative for the previous period
                        currencyCode,
                        cancellationToken);
                    
                    result.ComparativePeriod = new ComparativePeriodDto
                    {
                        FinancialPeriodId = previousPeriod.Id,
                        FinancialPeriodName = previousPeriod.Name,
                        StartDate = previousPeriod.StartDate,
                        EndDate = previousPeriod.EndDate,
                        TotalIncome = previousIncomeStatement.Income.Total,
                        TotalExpenses = previousIncomeStatement.Expenses.Total,
                        NetIncome = previousIncomeStatement.NetIncome,
                        ChangeAmount = netIncome - previousIncomeStatement.NetIncome,
                        ChangePercentage = previousIncomeStatement.NetIncome != 0 
                            ? (netIncome - previousIncomeStatement.NetIncome) / Math.Abs(previousIncomeStatement.NetIncome) * 100
                            : 0
                    };
                }
            }
            
            return result;
        }

        /// <summary>
        /// Generates a balance sheet for a specific financial period
        /// </summary>
        public async Task<BalanceSheetDto> GenerateBalanceSheetAsync(
            string financialPeriodId, 
            bool includeZeroBalances = false, 
            bool includeComparativePeriod = false,
            string currencyCode = null,
            CancellationToken cancellationToken = default)
        {
            // Get the financial period
            var financialPeriod = await _financialPeriodRepository.GetByIdAsync(financialPeriodId, cancellationToken);
            if (financialPeriod == null)
            {
                throw new InvalidOperationException($"Financial period with ID {financialPeriodId} not found");
            }
            
            // Get the trial balance for this period
            var trialBalance = await _trialBalanceService.GenerateTrialBalanceByFinancialPeriodAsync(
                financialPeriodId, 
                true, // Include zero balances for the calculation
                currencyCode,
                cancellationToken);
            
            // Get asset, liability, and equity accounts from the trial balance
            var assetAccounts = trialBalance.Accounts
                .Where(a => a.Classification == "Asset")
                .ToList();
                
            var liabilityAccounts = trialBalance.Accounts
                .Where(a => a.Classification == "Liability")
                .ToList();
                
            var equityAccounts = trialBalance.Accounts
                .Where(a => a.Classification == "Equity")
                .ToList();
            
            // Calculate totals
            decimal totalAssets = assetAccounts.Sum(a => a.DebitBalance - a.CreditBalance);
            decimal totalLiabilities = liabilityAccounts.Sum(a => a.CreditBalance - a.DebitBalance);
            decimal totalEquity = equityAccounts.Sum(a => a.CreditBalance - a.DebitBalance);
            
            // Get income statement for this period to include net income
            var incomeStatement = await GenerateIncomeStatementAsync(
                financialPeriodId,
                false,
                false,
                currencyCode,
                cancellationToken);
            
            // Add net income to equity
            totalEquity += incomeStatement.NetIncome;
            
            // Prepare balance sheet sections
            var assetSection = new FinancialStatementSectionDto
            {
                Title = "Assets",
                Accounts = assetAccounts
                    .Where(a => includeZeroBalances || (a.CreditBalance != 0 || a.DebitBalance != 0))
                    .Select(a => new FinancialStatementAccountDto
                    {
                        AccountId = a.AccountId,
                        AccountNumber = a.AccountNumber,
                        AccountName = a.AccountName,
                        Amount = a.DebitBalance - a.CreditBalance, // Net asset amount
                        IsCredit = false, // Asset is normally debit
                        CurrencyCode = a.CurrencyCode,
                        SubAccounts = new List<FinancialStatementAccountDto>()
                    })
                    .OrderBy(a => a.AccountNumber)
                    .ToList(),
                Total = totalAssets
            };
            
            var liabilitySection = new FinancialStatementSectionDto
            {
                Title = "Liabilities",
                Accounts = liabilityAccounts
                    .Where(a => includeZeroBalances || (a.CreditBalance != 0 || a.DebitBalance != 0))
                    .Select(a => new FinancialStatementAccountDto
                    {
                        AccountId = a.AccountId,
                        AccountNumber = a.AccountNumber,
                        AccountName = a.AccountName,
                        Amount = a.CreditBalance - a.DebitBalance, // Net liability amount
                        IsCredit = true, // Liability is normally credit
                        CurrencyCode = a.CurrencyCode,
                        SubAccounts = new List<FinancialStatementAccountDto>()
                    })
                    .OrderBy(a => a.AccountNumber)
                    .ToList(),
                Total = totalLiabilities
            };
            
            var equitySection = new FinancialStatementSectionDto
            {
                Title = "Equity",
                Accounts = equityAccounts
                    .Where(a => includeZeroBalances || (a.CreditBalance != 0 || a.DebitBalance != 0))
                    .Select(a => new FinancialStatementAccountDto
                    {
                        AccountId = a.AccountId,
                        AccountNumber = a.AccountNumber,
                        AccountName = a.AccountName,
                        Amount = a.CreditBalance - a.DebitBalance, // Net equity amount
                        IsCredit = true, // Equity is normally credit
                        CurrencyCode = a.CurrencyCode,
                        SubAccounts = new List<FinancialStatementAccountDto>()
                    })
                    .OrderBy(a => a.AccountNumber)
                    .ToList(),
                Total = totalEquity - incomeStatement.NetIncome // Exclude net income for now
            };
            
            // Add net income as a separate "account" in the equity section
            equitySection.Accounts.Add(new FinancialStatementAccountDto
            {
                AccountId = "net_income", // Special ID for net income
                AccountNumber = "9999", // Use a high number to ensure it appears at the end
                AccountName = "Net Income for the Period",
                Amount = incomeStatement.NetIncome,
                IsCredit = incomeStatement.NetIncome > 0, // Credit if positive, debit if negative
                CurrencyCode = currencyCode ?? "NGN",
                SubAccounts = new List<FinancialStatementAccountDto>()
            });
            
            // Create the balance sheet
            var result = new BalanceSheetDto
            {
                FinancialPeriodId = financialPeriodId,
                FinancialPeriodName = financialPeriod.Name,
                AsOfDate = financialPeriod.EndDate,
                GeneratedAt = DateTime.UtcNow,
                CurrencyCode = currencyCode ?? "NGN",
                Assets = assetSection,
                Liabilities = liabilitySection,
                Equity = equitySection,
                TotalAssets = totalAssets,
                TotalLiabilitiesAndEquity = totalLiabilities + totalEquity,
                IsBalanced = Math.Abs(totalAssets - (totalLiabilities + totalEquity)) < 0.01m // Allow small rounding differences
            };
            
            // Handle comparative period if requested
            if (includeComparativePeriod)
            {
                // Get the previous financial period
                var previousPeriod = await _financialPeriodRepository.GetPreviousPeriodAsync(financialPeriodId, cancellationToken);
                if (previousPeriod != null)
                {
                    // Generate balance sheet for the previous period
                    var previousBalanceSheet = await GenerateBalanceSheetAsync(
                        previousPeriod.Id,
                        includeZeroBalances,
                        false, // Don't include comparative for the previous period
                        currencyCode,
                        cancellationToken);
                    
                    result.ComparativePeriod = new ComparativePeriodDto
                    {
                        FinancialPeriodId = previousPeriod.Id,
                        FinancialPeriodName = previousPeriod.Name,
                        StartDate = previousPeriod.StartDate,
                        EndDate = previousPeriod.EndDate,
                        TotalAssets = previousBalanceSheet.TotalAssets,
                        TotalLiabilities = previousBalanceSheet.Liabilities.Total,
                        TotalEquity = previousBalanceSheet.Equity.Total,
                        ChangeInAssets = totalAssets - previousBalanceSheet.TotalAssets,
                        ChangeInLiabilities = totalLiabilities - previousBalanceSheet.Liabilities.Total,
                        ChangeInEquity = totalEquity - previousBalanceSheet.Equity.Total,
                        ChangePercentageAssets = previousBalanceSheet.TotalAssets != 0 
                            ? (totalAssets - previousBalanceSheet.TotalAssets) / Math.Abs(previousBalanceSheet.TotalAssets) * 100
                            : 0,
                        ChangePercentageLiabilities = previousBalanceSheet.Liabilities.Total != 0 
                            ? (totalLiabilities - previousBalanceSheet.Liabilities.Total) / Math.Abs(previousBalanceSheet.Liabilities.Total) * 100
                            : 0,
                        ChangePercentageEquity = previousBalanceSheet.Equity.Total != 0 
                            ? (totalEquity - previousBalanceSheet.Equity.Total) / Math.Abs(previousBalanceSheet.Equity.Total) * 100
                            : 0
                    };
                }
            }
            
            return result;
        }

        /// <summary>
        /// Generates a cash flow statement for a specific financial period
        /// </summary>
        public async Task<CashFlowStatementDto> GenerateCashFlowStatementAsync(
            string financialPeriodId, 
            bool includeZeroBalances = false, 
            bool includeComparativePeriod = false,
            string currencyCode = null,
            CancellationToken cancellationToken = default)
        {
            // Get the financial period
            var financialPeriod = await _financialPeriodRepository.GetByIdAsync(financialPeriodId, cancellationToken);
            if (financialPeriod == null)
            {
                throw new InvalidOperationException($"Financial period with ID {financialPeriodId} not found");
            }
            
            // Get the previous financial period
            var previousPeriod = await _financialPeriodRepository.GetPreviousPeriodAsync(financialPeriodId, cancellationToken);
            if (previousPeriod == null)
            {
                throw new InvalidOperationException($"Cannot generate cash flow statement without a previous period");
            }
            
            // Get the income statement for this period
            var incomeStatement = await GenerateIncomeStatementAsync(
                financialPeriodId,
                false,
                false,
                currencyCode,
                cancellationToken);
            
            // Get balance sheets for current and previous periods
            var currentBalanceSheet = await GenerateBalanceSheetAsync(
                financialPeriodId,
                true, // Include zero balances for the calculation
                false,
                currencyCode,
                cancellationToken);
                
            var previousBalanceSheet = await GenerateBalanceSheetAsync(
                previousPeriod.Id,
                true, // Include zero balances for the calculation
                false,
                currencyCode,
                cancellationToken);
            
            // Identify cash and cash equivalent accounts
            // In a real system, we would have a way to tag these accounts or use a specific account type
            var cashAccounts = currentBalanceSheet.Assets.Accounts
                .Where(a => a.AccountName.Contains("Cash") || a.AccountName.Contains("Bank"))
                .ToList();
                
            // Calculate net cash balance change
            decimal currentCashBalance = cashAccounts.Sum(a => a.Amount);
            
            // Find the same accounts in the previous period
            decimal previousCashBalance = 0;
            foreach (var cashAccount in cashAccounts)
            {
                var previousAccount = previousBalanceSheet.Assets.Accounts
                    .FirstOrDefault(a => a.AccountNumber == cashAccount.AccountNumber);
                
                if (previousAccount != null)
                {
                    previousCashBalance += previousAccount.Amount;
                }
            }
            
            decimal netCashChange = currentCashBalance - previousCashBalance;
            
            // Extract operating activities (simplified - in a real system, this would be more sophisticated)
            // Operating activities typically include net income and changes in working capital
            decimal operatingCashFlow = incomeStatement.NetIncome;
            
            // Add back non-cash expenses (simplified - would need to identify these properly)
            decimal nonCashExpenses = 0; // Placeholder for depreciation, amortization, etc.
            
            // Calculate changes in working capital (simplified)
            decimal changeInWorkingCapital = 0;
            
            // For a simplified approach, we'll assume a portion of the asset and liability changes 
            // that aren't cash are related to working capital
            var currentAssets = currentBalanceSheet.Assets.Accounts
                .Where(a => !a.AccountName.Contains("Cash") && !a.AccountName.Contains("Bank") && !a.AccountName.Contains("Fixed") && !a.AccountName.Contains("Long"))
                .Sum(a => a.Amount);
                
            var previousAssets = previousBalanceSheet.Assets.Accounts
                .Where(a => !a.AccountName.Contains("Cash") && !a.AccountName.Contains("Bank") && !a.AccountName.Contains("Fixed") && !a.AccountName.Contains("Long"))
                .Sum(a => a.Amount);
                
            var currentLiabilities = currentBalanceSheet.Liabilities.Accounts
                .Where(a => !a.AccountName.Contains("Long"))
                .Sum(a => a.Amount);
                
            var previousLiabilities = previousBalanceSheet.Liabilities.Accounts
                .Where(a => !a.AccountName.Contains("Long"))
                .Sum(a => a.Amount);
            
            // Changes in working capital - note that increases in assets decrease cash flow,
            // while increases in liabilities increase cash flow
            changeInWorkingCapital = (previousAssets - currentAssets) + (currentLiabilities - previousLiabilities);
            
            // Total operating cash flow
            decimal netCashFromOperatingActivities = operatingCashFlow + nonCashExpenses + changeInWorkingCapital;
            
            // Extract investing activities (simplified)
            // Investing activities typically include purchases and sales of fixed assets and investments
            var currentFixedAssets = currentBalanceSheet.Assets.Accounts
                .Where(a => a.AccountName.Contains("Fixed") || a.AccountName.Contains("Long"))
                .Sum(a => a.Amount);
                
            var previousFixedAssets = previousBalanceSheet.Assets.Accounts
                .Where(a => a.AccountName.Contains("Fixed") || a.AccountName.Contains("Long"))
                .Sum(a => a.Amount);
            
            // Net cash used in investing activities - increases in fixed assets represent cash outflows
            decimal netCashFromInvestingActivities = previousFixedAssets - currentFixedAssets;
            
            // Extract financing activities (simplified)
            // Financing activities typically include changes in debt and equity
            var currentLongTermLiabilities = currentBalanceSheet.Liabilities.Accounts
                .Where(a => a.AccountName.Contains("Long"))
                .Sum(a => a.Amount);
                
            var previousLongTermLiabilities = previousBalanceSheet.Liabilities.Accounts
                .Where(a => a.AccountName.Contains("Long"))
                .Sum(a => a.Amount);
                
            // Equity changes excluding net income (which is already accounted for in operating activities)
            var currentEquity = currentBalanceSheet.Equity.Total - incomeStatement.NetIncome;
            var previousEquity = previousBalanceSheet.Equity.Total;
            
            // Net cash from financing activities
            decimal netCashFromFinancingActivities = (currentLongTermLiabilities - previousLongTermLiabilities) + (currentEquity - previousEquity);
            
            // Prepare the cash flow statement
            var result = new CashFlowStatementDto
            {
                FinancialPeriodId = financialPeriodId,
                FinancialPeriodName = financialPeriod.Name,
                StartDate = financialPeriod.StartDate,
                EndDate = financialPeriod.EndDate,
                GeneratedAt = DateTime.UtcNow,
                CurrencyCode = currencyCode ?? "NGN",
                
                // Operating activities
                OperatingActivities = new CashFlowSectionDto
                {
                    Title = "Operating Activities",
                    Items = new List<CashFlowItemDto>
                    {
                        new CashFlowItemDto
                        {
                            Description = "Net Income",
                            Amount = incomeStatement.NetIncome
                        },
                        new CashFlowItemDto
                        {
                            Description = "Adjustments for Non-cash Items",
                            Amount = nonCashExpenses
                        },
                        new CashFlowItemDto
                        {
                            Description = "Changes in Working Capital",
                            Amount = changeInWorkingCapital
                        }
                    },
                    Total = netCashFromOperatingActivities
                },
                
                // Investing activities
                InvestingActivities = new CashFlowSectionDto
                {
                    Title = "Investing Activities",
                    Items = new List<CashFlowItemDto>
                    {
                        new CashFlowItemDto
                        {
                            Description = "Net Change in Fixed Assets",
                            Amount = previousFixedAssets - currentFixedAssets
                        }
                    },
                    Total = netCashFromInvestingActivities
                },
                
                // Financing activities
                FinancingActivities = new CashFlowSectionDto
                {
                    Title = "Financing Activities",
                    Items = new List<CashFlowItemDto>
                    {
                        new CashFlowItemDto
                        {
                            Description = "Net Change in Long-term Liabilities",
                            Amount = currentLongTermLiabilities - previousLongTermLiabilities
                        },
                        new CashFlowItemDto
                        {
                            Description = "Net Change in Equity",
                            Amount = currentEquity - previousEquity
                        }
                    },
                    Total = netCashFromFinancingActivities
                },
                
                // Summary
                NetCashFromOperatingActivities = netCashFromOperatingActivities,
                NetCashFromInvestingActivities = netCashFromInvestingActivities,
                NetCashFromFinancingActivities = netCashFromFinancingActivities,
                NetChangeInCash = netCashChange,
                CashAtBeginningOfPeriod = previousCashBalance,
                CashAtEndOfPeriod = currentCashBalance
            };
            
            // Validate that our calculations match the actual cash change
            decimal calculatedNetChange = netCashFromOperatingActivities + netCashFromInvestingActivities + netCashFromFinancingActivities;
            result.IsReconciled = Math.Abs(calculatedNetChange - netCashChange) < 0.01m; // Allow small rounding differences
            
            // Handle comparative period if requested
            if (includeComparativePeriod && previousPeriod != null)
            {
                // Get the period before the previous period
                var prePreviousPeriod = await _financialPeriodRepository.GetPreviousPeriodAsync(previousPeriod.Id, cancellationToken);
                if (prePreviousPeriod != null)
                {
                    // Generate cash flow statement for the previous period
                    var previousCashFlow = await GenerateCashFlowStatementAsync(
                        previousPeriod.Id,
                        includeZeroBalances,
                        false, // Don't include comparative for the previous period
                        currencyCode,
                        cancellationToken);
                    
                    result.ComparativePeriod = new ComparativePeriodDto
                    {
                        FinancialPeriodId = previousPeriod.Id,
                        FinancialPeriodName = previousPeriod.Name,
                        StartDate = previousPeriod.StartDate,
                        EndDate = previousPeriod.EndDate,
                        OperatingCashFlow = previousCashFlow.NetCashFromOperatingActivities,
                        InvestingCashFlow = previousCashFlow.NetCashFromInvestingActivities,
                        FinancingCashFlow = previousCashFlow.NetCashFromFinancingActivities,
                        NetChangeInCash = previousCashFlow.NetChangeInCash,
                        ChangeInOperatingCashFlow = netCashFromOperatingActivities - previousCashFlow.NetCashFromOperatingActivities,
                        ChangeInInvestingCashFlow = netCashFromInvestingActivities - previousCashFlow.NetCashFromInvestingActivities,
                        ChangeInFinancingCashFlow = netCashFromFinancingActivities - previousCashFlow.NetCashFromFinancingActivities,
                        ChangePercentageOperating = previousCashFlow.NetCashFromOperatingActivities != 0 
                            ? (netCashFromOperatingActivities - previousCashFlow.NetCashFromOperatingActivities) / Math.Abs(previousCashFlow.NetCashFromOperatingActivities) * 100
                            : 0,
                        ChangePercentageInvesting = previousCashFlow.NetCashFromInvestingActivities != 0 
                            ? (netCashFromInvestingActivities - previousCashFlow.NetCashFromInvestingActivities) / Math.Abs(previousCashFlow.NetCashFromInvestingActivities) * 100
                            : 0,
                        ChangePercentageFinancing = previousCashFlow.NetCashFromFinancingActivities != 0 
                            ? (netCashFromFinancingActivities - previousCashFlow.NetCashFromFinancingActivities) / Math.Abs(previousCashFlow.NetCashFromFinancingActivities) * 100
                            : 0
                    };
                }
            }
            
            return result;
        }

        /// <summary>
        /// Generates a statement of changes in equity for a specific financial period
        /// </summary>
        public async Task<EquityChangeStatementDto> GenerateEquityChangeStatementAsync(
            string financialPeriodId, 
            bool includeZeroBalances = false, 
            bool includeComparativePeriod = false,
            string currencyCode = null,
            CancellationToken cancellationToken = default)
        {
            // Get the financial period
            var financialPeriod = await _financialPeriodRepository.GetByIdAsync(financialPeriodId, cancellationToken);
            if (financialPeriod == null)
            {
                throw new InvalidOperationException($"Financial period with ID {financialPeriodId} not found");
            }
            
            // Get the previous financial period
            var previousPeriod = await _financialPeriodRepository.GetPreviousPeriodAsync(financialPeriodId, cancellationToken);
            if (previousPeriod == null)
            {
                throw new InvalidOperationException($"Cannot generate equity change statement without a previous period");
            }
            
            // Get the income statement for this period
            var incomeStatement = await GenerateIncomeStatementAsync(
                financialPeriodId,
                false,
                false,
                currencyCode,
                cancellationToken);
            
            // Get balance sheets for current and previous periods
            var currentBalanceSheet = await GenerateBalanceSheetAsync(
                financialPeriodId,
                true, // Include zero balances for the calculation
                false,
                currencyCode,
                cancellationToken);
                
            var previousBalanceSheet = await GenerateBalanceSheetAsync(
                previousPeriod.Id,
                true, // Include zero balances for the calculation
                false,
                currencyCode,
                cancellationToken);
            
            // Get equity accounts for current and previous periods
            var currentEquityAccounts = currentBalanceSheet.Equity.Accounts
                .Where(a => !a.AccountName.Contains("Net Income")) // Exclude net income
                .ToList();
                
            var previousEquityAccounts = previousBalanceSheet.Equity.Accounts
                .Where(a => !a.AccountName.Contains("Net Income")) // Exclude net income
                .ToList();
            
            // Create equity components
            var equityComponents = new List<EquityComponentDto>();
            
            // Add each equity account as a component
            foreach (var currentAccount in currentEquityAccounts)
            {
                // Find the corresponding account in the previous period
                var previousAccount = previousEquityAccounts
                    .FirstOrDefault(a => a.AccountNumber == currentAccount.AccountNumber);
                
                // Calculate the change
                decimal beginningBalance = previousAccount?.Amount ?? 0;
                decimal endingBalance = currentAccount.Amount;
                decimal change = endingBalance - beginningBalance;
                
                // Create the equity component
                equityComponents.Add(new EquityComponentDto
                {
                    ComponentName = currentAccount.AccountName,
                    AccountId = currentAccount.AccountId,
                    AccountNumber = currentAccount.AccountNumber,
                    BeginningBalance = beginningBalance,
                    Change = change,
                    EndingBalance = endingBalance
                });
            }
            
            // Add any equity accounts that were in the previous period but not in the current period
            foreach (var previousAccount in previousEquityAccounts)
            {
                if (!currentEquityAccounts.Any(a => a.AccountNumber == previousAccount.AccountNumber))
                {
                    equityComponents.Add(new EquityComponentDto
                    {
                        ComponentName = previousAccount.AccountName,
                        AccountId = previousAccount.AccountId,
                        AccountNumber = previousAccount.AccountNumber,
                        BeginningBalance = previousAccount.Amount,
                        Change = -previousAccount.Amount, // Account no longer exists, so the change is the negative of the beginning balance
                        EndingBalance = 0
                    });
                }
            }
            
            // Add net income as a separate component
            equityComponents.Add(new EquityComponentDto
            {
                ComponentName = "Net Income for the Period",
                AccountId = "net_income", // Special ID for net income
                AccountNumber = "9999", // Use a high number to ensure it appears at the end
                BeginningBalance = 0, // Net income always starts at 0 for a new period
                Change = incomeStatement.NetIncome,
                EndingBalance = incomeStatement.NetIncome
            });
            
            // Calculate totals
            decimal totalBeginningBalance = equityComponents.Sum(c => c.BeginningBalance);
            decimal totalChange = equityComponents.Sum(c => c.Change);
            decimal totalEndingBalance = equityComponents.Sum(c => c.EndingBalance);
            
            // Create the equity change statement
            var result = new EquityChangeStatementDto
            {
                FinancialPeriodId = financialPeriodId,
                FinancialPeriodName = financialPeriod.Name,
                StartDate = financialPeriod.StartDate,
                EndDate = financialPeriod.EndDate,
                GeneratedAt = DateTime.UtcNow,
                CurrencyCode = currencyCode ?? "NGN",
                Components = equityComponents.OrderBy(c => c.AccountNumber).ToList(),
                TotalBeginningBalance = totalBeginningBalance,
                TotalChange = totalChange,
                TotalEndingBalance = totalEndingBalance
            };
            
            // Handle comparative period if requested
            if (includeComparativePeriod && previousPeriod != null)
            {
                // Get the period before the previous period
                var prePreviousPeriod = await _financialPeriodRepository.GetPreviousPeriodAsync(previousPeriod.Id, cancellationToken);
                if (prePreviousPeriod != null)
                {
                    // Generate equity change statement for the previous period
                    var previousEquityChange = await GenerateEquityChangeStatementAsync(
                        previousPeriod.Id,
                        includeZeroBalances,
                        false, // Don't include comparative for the previous period
                        currencyCode,
                        cancellationToken);
                    
                    result.ComparativePeriod = new ComparativePeriodDto
                    {
                        FinancialPeriodId = previousPeriod.Id,
                        FinancialPeriodName = previousPeriod.Name,
                        StartDate = previousPeriod.StartDate,
                        EndDate = previousPeriod.EndDate,
                        BeginningEquity = previousEquityChange.TotalBeginningBalance,
                        EndingEquity = previousEquityChange.TotalEndingBalance,
                        ChangeInEquity = previousEquityChange.TotalChange,
                        ChangeAmount = totalChange - previousEquityChange.TotalChange,
                        ChangePercentage = previousEquityChange.TotalChange != 0 
                            ? (totalChange - previousEquityChange.TotalChange) / Math.Abs(previousEquityChange.TotalChange) * 100
                            : 0
                    };
                }
            }
            
            return result;
        }
    }

    // DTOs
    public class FinancialStatementSectionDto
    {
        public string Title { get; set; }
        public List<FinancialStatementAccountDto> Accounts { get; set; }
        public decimal Total { get; set; }
    }

    public class FinancialStatementAccountDto
    {
        public string AccountId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public decimal Amount { get; set; }
        public bool IsCredit { get; set; }
        public string CurrencyCode { get; set; }
        public List<FinancialStatementAccountDto> SubAccounts { get; set; }
    }

    public class ComparativePeriodDto
    {
        public string FinancialPeriodId { get; set; }
        public string FinancialPeriodName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        // For Income Statement
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetIncome { get; set; }
        public decimal ChangeAmount { get; set; }
        public decimal ChangePercentage { get; set; }
        
        // For Balance Sheet
        public decimal TotalAssets { get; set; }
        public decimal TotalLiabilities { get; set; }
        public decimal TotalEquity { get; set; }
        public decimal ChangeInAssets { get; set; }
        public decimal ChangeInLiabilities { get; set; }
        public decimal ChangeInEquity { get; set; }
        public decimal ChangePercentageAssets { get; set; }
        public decimal ChangePercentageLiabilities { get; set; }
        public decimal ChangePercentageEquity { get; set; }
        
        // For Cash Flow Statement
        public decimal OperatingCashFlow { get; set; }
        public decimal InvestingCashFlow { get; set; }
        public decimal FinancingCashFlow { get; set; }
        public decimal NetChangeInCash { get; set; }
        public decimal ChangeInOperatingCashFlow { get; set; }
        public decimal ChangeInInvestingCashFlow { get; set; }
        public decimal ChangeInFinancingCashFlow { get; set; }
        public decimal ChangePercentageOperating { get; set; }
        public decimal ChangePercentageInvesting { get; set; }
        public decimal ChangePercentageFinancing { get; set; }
        
        // For Equity Change Statement
        public decimal BeginningEquity { get; set; }
        public decimal EndingEquity { get; set; }
    }

    public class IncomeStatementDto
    {
        public string FinancialPeriodId { get; set; }
        public string FinancialPeriodName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string CurrencyCode { get; set; }
        public FinancialStatementSectionDto Income { get; set; }
        public FinancialStatementSectionDto Expenses { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetIncome { get; set; }
        public ComparativePeriodDto ComparativePeriod { get; set; }
    }

    public class BalanceSheetDto
    {
        public string FinancialPeriodId { get; set; }
        public string FinancialPeriodName { get; set; }
        public DateTime AsOfDate { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string CurrencyCode { get; set; }
        public FinancialStatementSectionDto Assets { get; set; }
        public FinancialStatementSectionDto Liabilities { get; set; }
        public FinancialStatementSectionDto Equity { get; set; }
        public decimal TotalAssets { get; set; }
        public decimal TotalLiabilitiesAndEquity { get; set; }
        public bool IsBalanced { get; set; }
        public ComparativePeriodDto ComparativePeriod { get; set; }
    }

    public class CashFlowStatementDto
    {
        public string FinancialPeriodId { get; set; }
        public string FinancialPeriodName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string CurrencyCode { get; set; }
        public CashFlowSectionDto OperatingActivities { get; set; }
        public CashFlowSectionDto InvestingActivities { get; set; }
        public CashFlowSectionDto FinancingActivities { get; set; }
        public decimal NetCashFromOperatingActivities { get; set; }
        public decimal NetCashFromInvestingActivities { get; set; }
        public decimal NetCashFromFinancingActivities { get; set; }
        public decimal NetChangeInCash { get; set; }
        public decimal CashAtBeginningOfPeriod { get; set; }
        public decimal CashAtEndOfPeriod { get; set; }
        public bool IsReconciled { get; set; }
        public ComparativePeriodDto ComparativePeriod { get; set; }
    }

    public class CashFlowSectionDto
    {
        public string Title { get; set; }
        public List<CashFlowItemDto> Items { get; set; }
        public decimal Total { get; set; }
    }

    public class CashFlowItemDto
    {
        public string Description { get; set; }
        public decimal Amount { get; set; }
    }

    public class EquityChangeStatementDto
    {
        public string FinancialPeriodId { get; set; }
        public string FinancialPeriodName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string CurrencyCode { get; set; }
        public List<EquityComponentDto> Components { get; set; }
        public decimal TotalBeginningBalance { get; set; }
        public decimal TotalChange { get; set; }
        public decimal TotalEndingBalance { get; set; }
        public ComparativePeriodDto ComparativePeriod { get; set; }
    }

    public class EquityComponentDto
    {
        public string ComponentName { get; set; }
        public string AccountId { get; set; }
        public string AccountNumber { get; set; }
        public decimal BeginningBalance { get; set; }
        public decimal Change { get; set; }
        public decimal EndingBalance { get; set; }
    }
}