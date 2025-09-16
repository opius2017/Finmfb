using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Domain.Entities.Accounting;

namespace FinTech.Core.Application.Interfaces.Services
{
    /// <summary>
    /// Service for managing the Chart of Accounts
    /// </summary>
    public interface IChartOfAccountService
    {
        Task<ChartOfAccount> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<ChartOfAccount> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ChartOfAccount>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ChartOfAccount>> GetByTypeAsync(AccountType accountType, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ChartOfAccount>> GetByClassificationAsync(AccountClassification classification, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ChartOfAccount>> GetActiveAccountsAsync(CancellationToken cancellationToken = default);
        Task<string> CreateAccountAsync(ChartOfAccount account, CancellationToken cancellationToken = default);
        Task UpdateAccountAsync(ChartOfAccount account, CancellationToken cancellationToken = default);
        Task ActivateAccountAsync(string id, string modifiedBy, CancellationToken cancellationToken = default);
        Task DeactivateAccountAsync(string id, string modifiedBy, CancellationToken cancellationToken = default);
        Task<bool> AccountNumberExistsAsync(string accountNumber, CancellationToken cancellationToken = default);
        Task<string> GenerateAccountNumberAsync(AccountType accountType, AccountClassification classification, CancellationToken cancellationToken = default);
    }
}