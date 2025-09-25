using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Accounting;
using FinTech.Core.Domain.Repositories.Accounting;

namespace FinTech.Core.Application.Services.Accounting
{
    public interface IBudgetingService
    {
        Task<BudgetDto> CreateBudgetAsync(
            CreateBudgetDto budgetDto,
            CancellationToken cancellationToken = default);
            
        Task<BudgetDto> GetBudgetAsync(
            string budgetId,
            CancellationToken cancellationToken = default);
            
        Task<BudgetDto> UpdateBudgetAsync(
            string budgetId,
            UpdateBudgetDto budgetDto,
            CancellationToken cancellationToken = default);
            
        Task<bool> DeleteBudgetAsync(
            string budgetId,
            CancellationToken cancellationToken = default);
            
        Task<BudgetVarianceAnalysisDto> GenerateVarianceAnalysisAsync(
            string budgetId,
            string financialPeriodId,
            bool includeDetails = true,
            CancellationToken cancellationToken = default);
            
        Task<List<BudgetDto>> GetBudgetsForPeriodAsync(
            string financialPeriodId,
            CancellationToken cancellationToken = default);
            
        Task<List<BudgetDto>> GetBudgetsForAccountAsync(
            string accountId,
            CancellationToken cancellationToken = default);
            
        Task<BudgetPerformanceDto> CalculateBudgetPerformanceAsync(
            string budgetId,
            string financialPeriodId,
            CancellationToken cancellationToken = default);
    }
    
    public class BudgetingService : IBudgetingService
    {
        private readonly IBudgetRepository _budgetRepository;
        private readonly IChartOfAccountRepository _chartOfAccountRepository;
        private readonly IFinancialPeriodRepository _financialPeriodRepository;
        private readonly IJournalEntryRepository _journalEntryRepository;
        private readonly IGeneralLedgerService _generalLedgerService;
        
        public BudgetingService(
            IBudgetRepository budgetRepository,
            IChartOfAccountRepository chartOfAccountRepository,
            IFinancialPeriodRepository financialPeriodRepository,
            IJournalEntryRepository journalEntryRepository,
            IGeneralLedgerService generalLedgerService)
        {
            _budgetRepository = budgetRepository ?? throw new ArgumentNullException(nameof(budgetRepository));
            _chartOfAccountRepository = chartOfAccountRepository ?? throw new ArgumentNullException(nameof(chartOfAccountRepository));
            _financialPeriodRepository = financialPeriodRepository ?? throw new ArgumentNullException(nameof(financialPeriodRepository));
            _journalEntryRepository = journalEntryRepository ?? throw new ArgumentNullException(nameof(journalEntryRepository));
            _generalLedgerService = generalLedgerService ?? throw new ArgumentNullException(nameof(generalLedgerService));
        }
        
        /// <summary>
        /// Creates a new budget
        /// </summary>
        public async Task<BudgetDto> CreateBudgetAsync(
            CreateBudgetDto budgetDto,
            CancellationToken cancellationToken = default)
        {
            // Validate input
            if (budgetDto == null)
            {
                throw new ArgumentNullException(nameof(budgetDto));
            }
            
            // Validate financial period
            var financialPeriod = await _financialPeriodRepository.GetByIdAsync(budgetDto.FinancialPeriodId, cancellationToken);
            if (financialPeriod == null)
            {
                throw new InvalidOperationException($"Financial period with ID {budgetDto.FinancialPeriodId} not found");
            }
            
            // Create budget entity
            var budget = new Budget
            {
                Id = Guid.NewGuid().ToString(),
                Name = budgetDto.Name,
                Description = budgetDto.Description,
                FinancialPeriodId = budgetDto.FinancialPeriodId,
                Type = budgetDto.Type,
                Status = BudgetStatus.Draft,
                CreatedBy = budgetDto.CreatedBy,
                CreatedDate = DateTime.UtcNow,
                LastModifiedBy = budgetDto.CreatedBy,
                LastModifiedDate = DateTime.UtcNow,
                BudgetItems = new List<BudgetItem>()
            };
            
            // Add budget items
            if (budgetDto.BudgetItems != null && budgetDto.BudgetItems.Any())
            {
                foreach (var itemDto in budgetDto.BudgetItems)
                {
                    // Validate the account
                    var account = await _chartOfAccountRepository.GetByIdAsync(itemDto.AccountId, cancellationToken);
                    if (account == null)
                    {
                        throw new InvalidOperationException($"Account with ID {itemDto.AccountId} not found");
                    }
                    
                    var budgetItem = new BudgetItem
                    {
                        Id = Guid.NewGuid().ToString(),
                        BudgetId = budget.Id,
                        AccountId = itemDto.AccountId,
                        Description = itemDto.Description,
                        AnnualAmount = itemDto.AnnualAmount,
                        MonthlyDistribution = itemDto.MonthlyDistribution?.ToArray() ?? DistributeAmountEvenly(itemDto.AnnualAmount),
                        Notes = itemDto.Notes
                    };
                    
                    budget.BudgetItems.Add(budgetItem);
                }
            }
            
            // Save the budget
            await _budgetRepository.AddAsync(budget, cancellationToken);
            
            // Map to DTO and return
            return MapBudgetToDto(budget);
        }
        
        /// <summary>
        /// Gets a budget by ID
        /// </summary>
        public async Task<BudgetDto> GetBudgetAsync(
            string budgetId,
            CancellationToken cancellationToken = default)
        {
            // Get the budget
            var budget = await _budgetRepository.GetByIdWithItemsAsync(budgetId, cancellationToken);
            if (budget == null)
            {
                throw new InvalidOperationException($"Budget with ID {budgetId} not found");
            }
            
            // Map to DTO and return
            return MapBudgetToDto(budget);
        }
        
        /// <summary>
        /// Updates an existing budget
        /// </summary>
        public async Task<BudgetDto> UpdateBudgetAsync(
            string budgetId,
            UpdateBudgetDto budgetDto,
            CancellationToken cancellationToken = default)
        {
            // Validate input
            if (budgetDto == null)
            {
                throw new ArgumentNullException(nameof(budgetDto));
            }
            
            // Get the budget
            var budget = await _budgetRepository.GetByIdWithItemsAsync(budgetId, cancellationToken);
            if (budget == null)
            {
                throw new InvalidOperationException($"Budget with ID {budgetId} not found");
            }
            
            // Check if budget can be updated
            if (budget.Status == BudgetStatus.Approved && !budgetDto.ForceUpdate)
            {
                throw new InvalidOperationException("Cannot update an approved budget without ForceUpdate flag");
            }
            
            // Update basic properties
            budget.Name = budgetDto.Name ?? budget.Name;
            budget.Description = budgetDto.Description ?? budget.Description;
            budget.Status = budgetDto.Status ?? budget.Status;
            budget.LastModifiedBy = budgetDto.LastModifiedBy;
            budget.LastModifiedDate = DateTime.UtcNow;
            
            // Handle budget items
            if (budgetDto.BudgetItems != null && budgetDto.BudgetItems.Any())
            {
                // Create a dictionary of existing items for easy lookup
                var existingItems = budget.BudgetItems.ToDictionary(i => i.Id);
                
                // Process each item in the update DTO
                foreach (var itemDto in budgetDto.BudgetItems)
                {
                    if (string.IsNullOrEmpty(itemDto.Id))
                    {
                        // This is a new item - add it
                        var account = await _chartOfAccountRepository.GetByIdAsync(itemDto.AccountId, cancellationToken);
                        if (account == null)
                        {
                            throw new InvalidOperationException($"Account with ID {itemDto.AccountId} not found");
                        }
                        
                        var newItem = new BudgetItem
                        {
                            Id = Guid.NewGuid().ToString(),
                            BudgetId = budget.Id,
                            AccountId = itemDto.AccountId,
                            Description = itemDto.Description,
                            AnnualAmount = itemDto.AnnualAmount,
                            MonthlyDistribution = itemDto.MonthlyDistribution?.ToArray() ?? DistributeAmountEvenly(itemDto.AnnualAmount),
                            Notes = itemDto.Notes
                        };
                        
                        budget.BudgetItems.Add(newItem);
                    }
                    else if (existingItems.TryGetValue(itemDto.Id, out var existingItem))
                    {
                        // This is an existing item - update it
                        existingItem.Description = itemDto.Description ?? existingItem.Description;
                        existingItem.AnnualAmount = itemDto.AnnualAmount;
                        
                        if (itemDto.MonthlyDistribution != null)
                        {
                            existingItem.MonthlyDistribution = itemDto.MonthlyDistribution.ToArray();
                        }
                        
                        existingItem.Notes = itemDto.Notes ?? existingItem.Notes;
                        
                        // Remove from dictionary to track what's been updated
                        existingItems.Remove(itemDto.Id);
                    }
                }
                
                // If RemoveOtherItems is true, remove any items not included in the update
                if (budgetDto.RemoveOtherItems && existingItems.Any())
                {
                    foreach (var item in existingItems.Values)
                    {
                        budget.BudgetItems.Remove(item);
                    }
                }
            }
            
            // Save the updated budget
            await _budgetRepository.UpdateAsync(budget, cancellationToken);
            
            // Map to DTO and return
            return MapBudgetToDto(budget);
        }
        
        /// <summary>
        /// Deletes a budget
        /// </summary>
        public async Task<bool> DeleteBudgetAsync(
            string budgetId,
            CancellationToken cancellationToken = default)
        {
            // Get the budget
            var budget = await _budgetRepository.GetByIdAsync(budgetId, cancellationToken);
            if (budget == null)
            {
                throw new InvalidOperationException($"Budget with ID {budgetId} not found");
            }
            
            // Check if budget can be deleted
            if (budget.Status == BudgetStatus.Approved)
            {
                throw new InvalidOperationException("Cannot delete an approved budget");
            }
            
            // Delete the budget
            await _budgetRepository.DeleteAsync(budgetId, cancellationToken);
            
            return true;
        }
        
        /// <summary>
        /// Generates a variance analysis comparing budget to actuals
        /// </summary>
        public async Task<BudgetVarianceAnalysisDto> GenerateVarianceAnalysisAsync(
            string budgetId,
            string financialPeriodId,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            // Get the budget
            var budget = await _budgetRepository.GetByIdWithItemsAsync(budgetId, cancellationToken);
            if (budget == null)
            {
                throw new InvalidOperationException($"Budget with ID {budgetId} not found");
            }
            
            // Get the financial period
            var financialPeriod = await _financialPeriodRepository.GetByIdAsync(financialPeriodId, cancellationToken);
            if (financialPeriod == null)
            {
                throw new InvalidOperationException($"Financial period with ID {financialPeriodId} not found");
            }
            
            // Check if the budget is for the requested financial period
            if (budget.FinancialPeriodId != financialPeriodId)
            {
                throw new InvalidOperationException($"Budget {budgetId} is not associated with financial period {financialPeriodId}");
            }
            
            // Initialize the result
            var result = new BudgetVarianceAnalysisDto
            {
                BudgetId = budgetId,
                BudgetName = budget.Name,
                FinancialPeriodId = financialPeriodId,
                FinancialPeriodName = financialPeriod.Name,
                GeneratedAt = DateTime.UtcNow,
                StartDate = financialPeriod.StartDate,
                EndDate = financialPeriod.EndDate,
                OverallVariance = new OverallVarianceDto(),
                AccountVariances = new List<AccountVarianceDto>()
            };
            
            // Calculate overall variance
            decimal totalBudgeted = budget.BudgetItems.Sum(i => i.AnnualAmount);
            decimal totalActual = 0;
            
            // Process each budget item
            foreach (var budgetItem in budget.BudgetItems)
            {
                // Get the account
                var account = await _chartOfAccountRepository.GetByIdAsync(budgetItem.AccountId, cancellationToken);
                if (account == null)
                {
                    continue; // Skip if account not found
                }
                
                // Get actual amount for this account
                var accountBalance = await _generalLedgerService.GetAccountBalanceAsync(
                    budgetItem.AccountId, 
                    financialPeriodId, 
                    cancellationToken);
                    
                decimal actualAmount = accountBalance.Balance;
                totalActual += actualAmount;
                
                // Calculate variance
                decimal variance = actualAmount - budgetItem.AnnualAmount;
                decimal variancePercentage = budgetItem.AnnualAmount != 0 
                    ? (variance / Math.Abs(budgetItem.AnnualAmount)) * 100 
                    : 0;
                    
                // Add to account variances if details are requested
                if (includeDetails)
                {
                    var accountVariance = new AccountVarianceDto
                    {
                        AccountId = budgetItem.AccountId,
                        AccountNumber = account.AccountNumber,
                        AccountName = account.AccountName,
                        BudgetedAmount = budgetItem.AnnualAmount,
                        ActualAmount = actualAmount,
                        Variance = variance,
                        VariancePercentage = variancePercentage,
                        IsFavorable = IsFavorableVariance(variance, account.Classification)
                    };
                    
                    result.AccountVariances.Add(accountVariance);
                }
            }
            
            // Calculate overall variance
            decimal overallVariance = totalActual - totalBudgeted;
            decimal overallVariancePercentage = totalBudgeted != 0 
                ? (overallVariance / Math.Abs(totalBudgeted)) * 100 
                : 0;
                
            result.OverallVariance = new OverallVarianceDto
            {
                TotalBudgetedAmount = totalBudgeted,
                TotalActualAmount = totalActual,
                Variance = overallVariance,
                VariancePercentage = overallVariancePercentage,
                // For overall, we consider favorable if we spent less than budgeted
                IsFavorable = overallVariance <= 0
            };
            
            return result;
        }
        
        /// <summary>
        /// Gets all budgets for a specific financial period
        /// </summary>
        public async Task<List<BudgetDto>> GetBudgetsForPeriodAsync(
            string financialPeriodId,
            CancellationToken cancellationToken = default)
        {
            // Get all budgets for the period
            var budgets = await _budgetRepository.GetBudgetsByPeriodIdAsync(financialPeriodId, cancellationToken);
            
            // Map to DTOs and return
            return budgets.Select(MapBudgetToDto).ToList();
        }
        
        /// <summary>
        /// Gets all budgets for a specific account
        /// </summary>
        public async Task<List<BudgetDto>> GetBudgetsForAccountAsync(
            string accountId,
            CancellationToken cancellationToken = default)
        {
            // Get all budgets for the account
            var budgets = await _budgetRepository.GetBudgetsByAccountIdAsync(accountId, cancellationToken);
            
            // Map to DTOs and return
            return budgets.Select(MapBudgetToDto).ToList();
        }
        
        /// <summary>
        /// Calculates budget performance metrics
        /// </summary>
        public async Task<BudgetPerformanceDto> CalculateBudgetPerformanceAsync(
            string budgetId,
            string financialPeriodId,
            CancellationToken cancellationToken = default)
        {
            // Get variance analysis
            var varianceAnalysis = await GenerateVarianceAnalysisAsync(
                budgetId, 
                financialPeriodId, 
                true, 
                cancellationToken);
                
            // Initialize the result
            var result = new BudgetPerformanceDto
            {
                BudgetId = budgetId,
                BudgetName = varianceAnalysis.BudgetName,
                FinancialPeriodId = financialPeriodId,
                FinancialPeriodName = varianceAnalysis.FinancialPeriodName,
                GeneratedAt = DateTime.UtcNow,
                OverallPerformance = new OverallPerformanceDto
                {
                    TotalBudgetedAmount = varianceAnalysis.OverallVariance.TotalBudgetedAmount,
                    TotalActualAmount = varianceAnalysis.OverallVariance.TotalActualAmount,
                    PerformanceRatio = varianceAnalysis.OverallVariance.TotalBudgetedAmount != 0
                        ? varianceAnalysis.OverallVariance.TotalActualAmount / varianceAnalysis.OverallVariance.TotalBudgetedAmount
                        : 0,
                    VariancePercentage = varianceAnalysis.OverallVariance.VariancePercentage,
                    PerformanceScore = CalculatePerformanceScore(varianceAnalysis.OverallVariance.VariancePercentage),
                    PerformanceRating = GetPerformanceRating(varianceAnalysis.OverallVariance.VariancePercentage)
                },
                AccountPerformances = new List<AccountPerformanceDto>()
            };
            
            // Calculate performance for each account
            foreach (var accountVariance in varianceAnalysis.AccountVariances)
            {
                var accountPerformance = new AccountPerformanceDto
                {
                    AccountId = accountVariance.AccountId,
                    AccountNumber = accountVariance.AccountNumber,
                    AccountName = accountVariance.AccountName,
                    BudgetedAmount = accountVariance.BudgetedAmount,
                    ActualAmount = accountVariance.ActualAmount,
                    PerformanceRatio = accountVariance.BudgetedAmount != 0
                        ? accountVariance.ActualAmount / accountVariance.BudgetedAmount
                        : 0,
                    VariancePercentage = accountVariance.VariancePercentage,
                    PerformanceScore = CalculatePerformanceScore(accountVariance.VariancePercentage),
                    PerformanceRating = GetPerformanceRating(accountVariance.VariancePercentage),
                    IsFavorable = accountVariance.IsFavorable
                };
                
                result.AccountPerformances.Add(accountPerformance);
            }
            
            return result;
        }
        
        #region Private Helper Methods
        
        /// <summary>
        /// Maps a Budget entity to a BudgetDto
        /// </summary>
        private BudgetDto MapBudgetToDto(Budget budget)
        {
            if (budget == null)
            {
                return null;
            }
            
            var dto = new BudgetDto
            {
                Id = budget.Id,
                Name = budget.Name,
                Description = budget.Description,
                FinancialPeriodId = budget.FinancialPeriodId,
                Type = budget.Type,
                Status = budget.Status,
                CreatedBy = budget.CreatedBy,
                CreatedDate = budget.CreatedDate,
                LastModifiedBy = budget.LastModifiedBy,
                LastModifiedDate = budget.LastModifiedDate,
                BudgetItems = budget.BudgetItems?.Select(item => new BudgetItemDto
                {
                    Id = item.Id,
                    BudgetId = item.BudgetId,
                    AccountId = item.AccountId,
                    Description = item.Description,
                    AnnualAmount = item.AnnualAmount,
                    MonthlyDistribution = item.MonthlyDistribution?.ToArray(),
                    Notes = item.Notes
                }).ToList() ?? new List<BudgetItemDto>()
            };
            
            return dto;
        }
        
        /// <summary>
        /// Distributes an amount evenly across 12 months
        /// </summary>
        private decimal[] DistributeAmountEvenly(decimal annualAmount)
        {
            var monthlyAmount = annualAmount / 12;
            return Enumerable.Repeat(monthlyAmount, 12).ToArray();
        }
        
        /// <summary>
        /// Determines if a variance is favorable based on account type
        /// </summary>
        private bool IsFavorableVariance(decimal variance, string accountClassification)
        {
            // For expense accounts, a negative variance is favorable (less than budgeted)
            // For revenue accounts, a positive variance is favorable (more than budgeted)
            
            switch (accountClassification.ToLower())
            {
                case "expense":
                case "liability":
                    return variance <= 0; // Spent less than budgeted is favorable
                    
                case "income":
                case "revenue":
                case "asset":
                case "equity":
                    return variance >= 0; // Earned more than budgeted is favorable
                    
                default:
                    return variance == 0; // No variance is neutral
            }
        }
        
        /// <summary>
        /// Calculates a performance score based on variance percentage
        /// </summary>
        private int CalculatePerformanceScore(decimal variancePercentage)
        {
            // Convert to absolute value
            var absVariance = Math.Abs(variancePercentage);
            
            // Score from 0-100, where 100 is perfect (no variance)
            if (absVariance <= 1)
                return 100; // Within 1% is considered perfect
            else if (absVariance <= 5)
                return 90;
            else if (absVariance <= 10)
                return 80;
            else if (absVariance <= 15)
                return 70;
            else if (absVariance <= 20)
                return 60;
            else if (absVariance <= 30)
                return 50;
            else if (absVariance <= 40)
                return 40;
            else if (absVariance <= 50)
                return 30;
            else if (absVariance <= 75)
                return 20;
            else if (absVariance <= 100)
                return 10;
            else
                return 0;
        }
        
        /// <summary>
        /// Gets a performance rating based on variance percentage
        /// </summary>
        private string GetPerformanceRating(decimal variancePercentage)
        {
            // Convert to absolute value
            var absVariance = Math.Abs(variancePercentage);
            
            if (absVariance <= 5)
                return "Excellent";
            else if (absVariance <= 10)
                return "Very Good";
            else if (absVariance <= 20)
                return "Good";
            else if (absVariance <= 30)
                return "Satisfactory";
            else if (absVariance <= 50)
                return "Needs Improvement";
            else
                return "Poor";
        }
        
        #endregion
    }
    
    #region Domain Entities
    
    /// <summary>
    /// Represents a budget for a financial period
    /// </summary>
    public class Budget
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string FinancialPeriodId { get; set; }
        public string Type { get; set; } // Operating, Capital, etc.
        public BudgetStatus Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public ICollection<BudgetItem> BudgetItems { get; set; } = new List<BudgetItem>();
    }
    
    /// <summary>
    /// Represents a line item in a budget
    /// </summary>
    public class BudgetItem
    {
        public string Id { get; set; }
        public string BudgetId { get; set; }
        public string AccountId { get; set; }
        public string Description { get; set; }
        public decimal AnnualAmount { get; set; }
        public decimal[] MonthlyDistribution { get; set; } // 12 months of budget allocation
        public string Notes { get; set; }
    }
    
    /// <summary>
    /// Budget status enum
    /// </summary>
    public enum BudgetStatus
    {
        Draft,
        Submitted,
        UnderReview,
        Approved,
        Rejected,
        Archived
    }
    
    #endregion
    
    #region DTOs
    
    public class CreateBudgetDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string FinancialPeriodId { get; set; }
        public string Type { get; set; } // Operating, Capital, etc.
        public string CreatedBy { get; set; }
        public List<CreateBudgetItemDto> BudgetItems { get; set; }
    }
    
    public class CreateBudgetItemDto
    {
        public string AccountId { get; set; }
        public string Description { get; set; }
        public decimal AnnualAmount { get; set; }
        public decimal[] MonthlyDistribution { get; set; }
        public string Notes { get; set; }
    }
    
    public class UpdateBudgetDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public BudgetStatus? Status { get; set; }
        public string LastModifiedBy { get; set; }
        public List<UpdateBudgetItemDto> BudgetItems { get; set; }
        public bool RemoveOtherItems { get; set; } = false;
        public bool ForceUpdate { get; set; } = false;
    }
    
    public class UpdateBudgetItemDto
    {
        public string Id { get; set; } // If empty, this is a new item
        public string AccountId { get; set; }
        public string Description { get; set; }
        public decimal AnnualAmount { get; set; }
        public decimal[] MonthlyDistribution { get; set; }
        public string Notes { get; set; }
    }
    
    public class BudgetDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string FinancialPeriodId { get; set; }
        public string Type { get; set; }
        public BudgetStatus Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public List<BudgetItemDto> BudgetItems { get; set; }
    }
    
    public class BudgetItemDto
    {
        public string Id { get; set; }
        public string BudgetId { get; set; }
        public string AccountId { get; set; }
        public string Description { get; set; }
        public decimal AnnualAmount { get; set; }
        public decimal[] MonthlyDistribution { get; set; }
        public string Notes { get; set; }
    }
    
    public class BudgetVarianceAnalysisDto
    {
        public string BudgetId { get; set; }
        public string BudgetName { get; set; }
        public string FinancialPeriodId { get; set; }
        public string FinancialPeriodName { get; set; }
        public DateTime GeneratedAt { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public OverallVarianceDto OverallVariance { get; set; }
        public List<AccountVarianceDto> AccountVariances { get; set; }
    }
    
    public class OverallVarianceDto
    {
        public decimal TotalBudgetedAmount { get; set; }
        public decimal TotalActualAmount { get; set; }
        public decimal Variance { get; set; }
        public decimal VariancePercentage { get; set; }
        public bool IsFavorable { get; set; }
    }
    
    public class AccountVarianceDto
    {
        public string AccountId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public decimal BudgetedAmount { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal Variance { get; set; }
        public decimal VariancePercentage { get; set; }
        public bool IsFavorable { get; set; }
    }
    
    public class BudgetPerformanceDto
    {
        public string BudgetId { get; set; }
        public string BudgetName { get; set; }
        public string FinancialPeriodId { get; set; }
        public string FinancialPeriodName { get; set; }
        public DateTime GeneratedAt { get; set; }
        public OverallPerformanceDto OverallPerformance { get; set; }
        public List<AccountPerformanceDto> AccountPerformances { get; set; }
    }
    
    public class OverallPerformanceDto
    {
        public decimal TotalBudgetedAmount { get; set; }
        public decimal TotalActualAmount { get; set; }
        public decimal PerformanceRatio { get; set; }
        public decimal VariancePercentage { get; set; }
        public int PerformanceScore { get; set; } // 0-100
        public string PerformanceRating { get; set; } // Excellent, Good, etc.
    }
    
    public class AccountPerformanceDto
    {
        public string AccountId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public decimal BudgetedAmount { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal PerformanceRatio { get; set; }
        public decimal VariancePercentage { get; set; }
        public int PerformanceScore { get; set; } // 0-100
        public string PerformanceRating { get; set; } // Excellent, Good, etc.
        public bool IsFavorable { get; set; }
    }
    
    #endregion
}
