using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Domain.Entities.Accounting;

namespace FinTech.Domain.Repositories.Accounting
{
    /// <summary>
    /// Repository interface for Chart of Accounts
    /// </summary>
    public interface IChartOfAccountRepository : IRepository<ChartOfAccount>
    {
        Task<IReadOnlyList<ChartOfAccount>> GetByClassificationAsync(
            AccountClassification classification, 
            CancellationToken cancellationToken = default);
            
        Task<IReadOnlyList<ChartOfAccount>> GetByAccountTypeAsync(
            AccountType accountType, 
            CancellationToken cancellationToken = default);
            
        Task<IReadOnlyList<ChartOfAccount>> GetChildAccountsAsync(
            string parentAccountId, 
            CancellationToken cancellationToken = default);
            
        Task<ChartOfAccount> GetByAccountCodeAsync(
            string accountCode, 
            CancellationToken cancellationToken = default);
            
        Task<bool> ExistsAsync(
            string accountCode, 
            CancellationToken cancellationToken = default);
            
        Task<IReadOnlyList<ChartOfAccount>> GetActiveAccountsAsync(
            CancellationToken cancellationToken = default);
            
        Task<IReadOnlyList<ChartOfAccount>> SearchAccountsAsync(
            string searchTerm, 
            int? maxResults = null,
            CancellationToken cancellationToken = default);
    }
}