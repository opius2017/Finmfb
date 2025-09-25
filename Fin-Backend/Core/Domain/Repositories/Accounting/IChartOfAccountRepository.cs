using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Accounting;

namespace FinTech.Core.Domain.Repositories.Accounting
{
    /// <summary>
    /// Repository interface for Chart of Accounts
    /// </summary>
    public interface IChartOfAccountRepository : IRepository<ChartOfAccount>
    {
        Task<IReadOnlyList<ChartOfAccount>> GetByClassificationAsync(
            AccountClassification classification, 
            CancellationToken cancellationToken = default);
            
        Task<IReadOnlyList<ChartOfAccount>> GetByTypeAsync(
            AccountType accountType, 
            CancellationToken cancellationToken = default);
            
        Task<IReadOnlyList<ChartOfAccount>> GetChildAccountsAsync(
            string parentAccountId, 
            CancellationToken cancellationToken = default);
            
        Task<ChartOfAccount> GetByAccountNumberAsync(
            string accountNumber, 
            CancellationToken cancellationToken = default);
            
        Task<ChartOfAccount> GetHighestAccountNumberByPrefixAsync(
            string prefix,
            CancellationToken cancellationToken = default);
            
        Task<bool> AccountNumberExistsAsync(
            string accountNumber, 
            CancellationToken cancellationToken = default);
            
        Task<IReadOnlyList<ChartOfAccount>> GetActiveAccountsAsync(
            CancellationToken cancellationToken = default);
            
        Task<IReadOnlyList<ChartOfAccount>> SearchAccountsAsync(
            string searchTerm, 
            int? maxResults = null,
            CancellationToken cancellationToken = default);
    }
}
