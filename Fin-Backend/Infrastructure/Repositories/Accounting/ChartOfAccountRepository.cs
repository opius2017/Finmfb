using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Accounting;
using FinTech.Core.Domain.Repositories.Accounting;
using FinTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinTech.Infrastructure.Repositories.Accounting
{
    /// <summary>
    /// Repository implementation for Chart of Accounts
    /// </summary>
    public class ChartOfAccountRepository : Repository<ChartOfAccount>, IChartOfAccountRepository
    {
        public ChartOfAccountRepository(ApplicationDbContext context) : base(context)
        {
        }
        
        public async Task<IReadOnlyList<ChartOfAccount>> GetByClassificationAsync(
            AccountClassification classification, 
            CancellationToken cancellationToken = default)
        {
            return await _context.ChartOfAccounts
                .Where(a => a.Classification == classification)
                .OrderBy(a => a.AccountNumber)
                .ToListAsync(cancellationToken);
        }
        
        public async Task<IReadOnlyList<ChartOfAccount>> GetByTypeAsync(
            AccountType accountType, 
            CancellationToken cancellationToken = default)
        {
            return await _context.ChartOfAccounts
                .Where(a => a.AccountType == accountType)
                .OrderBy(a => a.AccountNumber)
                .ToListAsync(cancellationToken);
        }
        
        public async Task<IReadOnlyList<ChartOfAccount>> GetChildAccountsAsync(
            string parentAccountId, 
            CancellationToken cancellationToken = default)
        {
            return await _context.ChartOfAccounts
                .Where(a => a.ParentAccountId == parentAccountId)
                .OrderBy(a => a.AccountNumber)
                .ToListAsync(cancellationToken);
        }
        
        public async Task<ChartOfAccount> GetByAccountNumberAsync(
            string accountNumber, 
            CancellationToken cancellationToken = default)
        {
            return await _context.ChartOfAccounts
                .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber, cancellationToken);
        }
        
        public async Task<ChartOfAccount> GetHighestAccountNumberByPrefixAsync(
            string prefix,
            CancellationToken cancellationToken = default)
        {
            return await _context.ChartOfAccounts
                .Where(a => a.AccountNumber.StartsWith(prefix))
                .OrderByDescending(a => a.AccountNumber)
                .FirstOrDefaultAsync(cancellationToken);
        }
        
        public async Task<bool> AccountNumberExistsAsync(
            string accountNumber, 
            CancellationToken cancellationToken = default)
        {
            return await _context.ChartOfAccounts
                .AnyAsync(a => a.AccountNumber == accountNumber, cancellationToken);
        }
        
        public async Task<IReadOnlyList<ChartOfAccount>> GetActiveAccountsAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.ChartOfAccounts
                .Where(a => a.Status == FinTech.Core.Domain.Enums.AccountStatus.Active)
                .OrderBy(a => a.AccountNumber)
                .ToListAsync(cancellationToken);
        }
        
        public async Task<IReadOnlyList<ChartOfAccount>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return await ListAllAsync(cancellationToken);
        }
        
        public async Task<IReadOnlyList<ChartOfAccount>> SearchAccountsAsync(
            string searchTerm, 
            int? maxResults = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.ChartOfAccounts
                .Where(a => 
                    a.AccountNumber.Contains(searchTerm) || 
                    a.AccountName.Contains(searchTerm) || 
                    a.Description.Contains(searchTerm))
                .OrderBy(a => a.AccountNumber);
                
            if (maxResults.HasValue)
            {
                query = query.Take(maxResults.Value);
            }
            
            return await query.ToListAsync(cancellationToken);
        }
    }
}
