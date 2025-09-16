using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Application.Services.Accounting;

namespace FinTech.Domain.Repositories.Accounting
{
    /// <summary>
    /// Repository interface for Budget entity
    /// </summary>
    public interface IBudgetRepository
    {
        /// <summary>
        /// Gets a budget by ID
        /// </summary>
        Task<Budget> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a budget by ID with its budget items
        /// </summary>
        Task<Budget> GetByIdWithItemsAsync(string id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all budgets for a specific financial period
        /// </summary>
        Task<IEnumerable<Budget>> GetBudgetsByPeriodIdAsync(string financialPeriodId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all budgets that include a specific account
        /// </summary>
        Task<IEnumerable<Budget>> GetBudgetsByAccountIdAsync(string accountId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Adds a new budget
        /// </summary>
        Task<Budget> AddAsync(Budget budget, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Updates an existing budget
        /// </summary>
        Task<Budget> UpdateAsync(Budget budget, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Deletes a budget
        /// </summary>
        Task DeleteAsync(string id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all budgets
        /// </summary>
        Task<IEnumerable<Budget>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}