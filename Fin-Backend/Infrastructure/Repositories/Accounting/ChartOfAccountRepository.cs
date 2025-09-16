using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Domain.Entities.Accounting;
using FinTech.Domain.Repositories.Accounting;
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
                .OrderBy(a => a.AccountCode)
                .ToListAsync(cancellationToken);
        }
        
        public async Task<IReadOnlyList<ChartOfAccount>> GetByAccountTypeAsync(
            AccountType accountType, 
            CancellationToken cancellationToken = default)
        {
            return await _context.ChartOfAccounts
                .Where(a => a.AccountType == accountType)
                .OrderBy(a => a.AccountCode)
                .ToListAsync(cancellationToken);
        }
        
        public async Task<IReadOnlyList<ChartOfAccount>> GetChildAccountsAsync(
            string parentAccountId, 
            CancellationToken cancellationToken = default)
        {
            return await _context.ChartOfAccounts
                .Where(a => a.ParentAccountId == parentAccountId)
                .OrderBy(a => a.AccountCode)
                .ToListAsync(cancellationToken);
        }
        
        public async Task<ChartOfAccount> GetByAccountCodeAsync(
            string accountCode, 
            CancellationToken cancellationToken = default)
        {
            return await _context.ChartOfAccounts
                .FirstOrDefaultAsync(a => a.AccountCode == accountCode, cancellationToken);
        }
        
        public async Task<bool> ExistsAsync(
            string accountCode, 
            CancellationToken cancellationToken = default)
        {
            return await _context.ChartOfAccounts
                .AnyAsync(a => a.AccountCode == accountCode, cancellationToken);
        }
        
        public async Task<IReadOnlyList<ChartOfAccount>> GetActiveAccountsAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.ChartOfAccounts
                .Where(a => a.IsActive)
                .OrderBy(a => a.AccountCode)
                .ToListAsync(cancellationToken);
        }
        
        public async Task<IReadOnlyList<ChartOfAccount>> SearchAccountsAsync(
            string searchTerm, 
            int? maxResults = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.ChartOfAccounts
                .Where(a => 
                    a.AccountCode.Contains(searchTerm) || 
                    a.AccountName.Contains(searchTerm) || 
                    a.Description.Contains(searchTerm))
                .OrderBy(a => a.AccountCode);
                
            if (maxResults.HasValue)
            {
                query = query.Take(maxResults.Value);
            }
            
            return await query.ToListAsync(cancellationToken);
        }
    }
}