using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Deposits;
using FinTech.Core.Domain.Entities.ClientPortal;
using FinTech.Core.Application.Common.Interfaces;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using FinTech.Core.Domain.Enums;

namespace FinTech.Core.Application.Services
{
    public interface IAccountOverviewService
    {
        Task<IEnumerable<DepositAccount>> GetClientAccountsAsync(string customerId);
        Task<DepositAccount> GetAccountDetailsAsync(string accountNumber);
        Task<decimal> GetAccountBalanceAsync(string accountNumber);
        Task<IEnumerable<DepositTransaction>> GetAccountTransactionsAsync(string accountNumber, DateTime startDate, DateTime endDate, int page = 1, int pageSize = 20);
        Task<byte[]> GenerateAccountStatementAsync(string accountNumber, DateTime startDate, DateTime endDate, string format = "pdf");
        Task<AccountSummary> GetAccountSummaryAsync(string customerId);
        Task<IEnumerable<DepositTransaction>> GetRecentTransactionsAsync(string customerId, int count = 5);
        Task<IEnumerable<AccountActivity>> GetAccountActivityAsync(string accountNumber, int days = 30);
    }

    public class AccountOverviewService : IAccountOverviewService
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ILogger<AccountOverviewService> _logger;

        public AccountOverviewService(IApplicationDbContext dbContext, ILogger<AccountOverviewService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<IEnumerable<DepositAccount>> GetClientAccountsAsync(string customerId)
        {
            try
            {
                return await _dbContext.DepositAccounts
                    .Where(a => a.CustomerId == customerId && a.Status == AccountStatus.Active)
                    .Include(a => a.Product)
                    .OrderBy(a => a.Product.ProductName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving accounts for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<DepositAccount> GetAccountDetailsAsync(string accountNumber)
        {
            try
            {
                var account = await _dbContext.DepositAccounts
                    .Include(a => a.Product)
                    .Include(a => a.Customer)
                    .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);

                if (account == null)
                {
                    throw new KeyNotFoundException($"Account with number {accountNumber} not found.");
                }

                return account;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving account details for account number {AccountNumber}", accountNumber);
                throw;
            }
        }

        public async Task<decimal> GetAccountBalanceAsync(string accountNumber)
        {
            try
            {
                var account = await _dbContext.DepositAccounts
                    .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);

                if (account == null)
                {
                    throw new KeyNotFoundException($"Account with number {accountNumber} not found.");
                }

                return account.CurrentBalance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving balance for account number {AccountNumber}", accountNumber);
                throw;
            }
        }

        public async Task<IEnumerable<DepositTransaction>> GetAccountTransactionsAsync(string accountNumber, DateTime startDate, DateTime endDate, int page = 1, int pageSize = 20)
        {
            try
            {
                // Ensure endDate includes the entire day
                endDate = endDate.Date.AddDays(1).AddSeconds(-1);
                
                return await _dbContext.DepositTransactions
                    .Where(t => t.AccountNumber == accountNumber && 
                           t.TransactionDate >= startDate && 
                           t.TransactionDate <= endDate)
                    .OrderByDescending(t => t.TransactionDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transactions for account number {AccountNumber}", accountNumber);
                throw;
            }
        }

        public async Task<byte[]> GenerateAccountStatementAsync(string accountNumber, DateTime startDate, DateTime endDate, string format = "pdf")
        {
            try
            {
                var account = await GetAccountDetailsAsync(accountNumber);
                var transactions = await GetAccountTransactionsAsync(accountNumber, startDate, endDate, 1, 1000); // Get up to 1000 transactions for the statement

                // This would call a reporting service to generate the actual PDF/Excel
                // For now, we'll return a dummy byte array
                return new byte[100]; // Placeholder implementation
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating statement for account number {AccountNumber}", accountNumber);
                throw;
            }
        }

        public async Task<AccountSummary> GetAccountSummaryAsync(string customerId)
        {
            try
            {
                var accounts = await GetClientAccountsAsync(customerId);
                
                var summary = new AccountSummary
                {
                    TotalBalance = accounts.Sum(a => a.CurrentBalance),
                    TotalSavings = accounts
                        // FinTech Best Practice: Convert enum to string for comparison
                        .Where(a => a.Product.ProductType.ToString() == "SavingsAccount")
                        .Sum(a => a.CurrentBalance),
                    TotalCurrent = accounts
                        .Where(a => a.Product.ProductType.ToString() == "CurrentAccount")
                        .Sum(a => a.CurrentBalance),
                    TotalFixed = accounts
                        .Where(a => a.Product.ProductType.ToString() == "FixedDeposit" || a.Product.ProductType.ToString() == "FixedDeposit")
                        .Sum(a => a.CurrentBalance),
                    AccountCount = accounts.Count(),
                    LastUpdateTime = DateTime.UtcNow
                };
                
                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving account summary for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<IEnumerable<DepositTransaction>> GetRecentTransactionsAsync(string customerId, int count = 5)
        {
            try
            {
                // Get all customer account numbers
                var accountNumbers = await _dbContext.DepositAccounts
                    .Where(a => a.CustomerId == customerId)
                    .Select(a => a.AccountNumber)
                    .ToListAsync();
                
                if (!accountNumbers.Any())
                {
                    return new List<DepositTransaction>();
                }
                
                // Get recent transactions for all accounts
                return await _dbContext.DepositTransactions
                    .Where(t => accountNumbers.Contains(t.AccountNumber))
                    .OrderByDescending(t => t.TransactionDate)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent transactions for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<IEnumerable<AccountActivity>> GetAccountActivityAsync(string accountNumber, int days = 30)
        {
            try
            {
                var startDate = DateTime.UtcNow.AddDays(-days);
                var transactions = await _dbContext.DepositTransactions
                    .Where(t => t.AccountNumber == accountNumber && t.TransactionDate >= startDate)
                    .OrderByDescending(t => t.TransactionDate)
                    .ToListAsync();
                
                // Group transactions by day and calculate daily totals
                return transactions
                    .GroupBy(t => t.TransactionDate.Date)
                    .Select(g => new AccountActivity
                    {
                        Date = g.Key,
                        TotalCredits = g.Where(t => t.TransactionType == TransactionType.Credit).Sum(t => t.Amount),
                        TotalDebits = g.Where(t => t.TransactionType == TransactionType.Debit).Sum(t => t.Amount),
                        TransactionCount = g.Count()
                    })

                    .OrderByDescending(a => a.Date)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving account activity for account number {AccountNumber}", accountNumber);
                throw;
            }
        }
    }

    public class AccountSummary
    {
        public decimal TotalBalance { get; set; }
        public decimal TotalSavings { get; set; }
        public decimal TotalCurrent { get; set; }
        public decimal TotalFixed { get; set; }
        public int AccountCount { get; set; }
        public DateTime LastUpdateTime { get; set; }
    }

    public class AccountActivity
    {
        public DateTime Date { get; set; }
        public decimal TotalCredits { get; set; }
        public decimal TotalDebits { get; set; }
        public int TransactionCount { get; set; }
    }
}
