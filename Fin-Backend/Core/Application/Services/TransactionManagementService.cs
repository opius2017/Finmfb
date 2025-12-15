using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using FinTech.Core.Domain.Entities.Deposits;
using FinTech.Core.Application.DTOs.ClientPortal;
using FinTech.Core.Domain.Enums;
using FinTech.Core.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinTech.Core.Application.Services
{
    public interface ITransactionManagementService
    {
        Task<(IEnumerable<DepositTransaction> Transactions, int TotalCount)> SearchTransactionsAsync(TransactionSearchDto searchDto, Guid customerId);
        Task<IEnumerable<string>> GetTransactionCategoriesAsync(string accountNumber);
        Task<IEnumerable<string>> GetTransactionChannelsAsync(string accountNumber);
        Task<byte[]> ExportTransactionsAsync(TransactionExportDto exportDto, Guid customerId);
        Task<Dictionary<string, decimal>> GetCategoryBreakdownAsync(string accountNumber, DateTime startDate, DateTime endDate);
        Task<IEnumerable<DepositTransaction>> GetLargestTransactionsAsync(Guid customerId, int count = 5);
        Task<Dictionary<string, decimal>> GetMonthlySpendingAsync(string accountNumber, int months = 6);
    }

    public class TransactionManagementService : ITransactionManagementService
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ILogger<TransactionManagementService> _logger;

        public TransactionManagementService(IApplicationDbContext dbContext, ILogger<TransactionManagementService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<(IEnumerable<DepositTransaction> Transactions, int TotalCount)> SearchTransactionsAsync(TransactionSearchDto searchDto, Guid customerId)
        {
            try
            {
                // First, verify the account belongs to the customer
                var accountNumbers = await _dbContext.DepositAccounts
                    .Where(a => a.CustomerId == customerId)
                    .Select(a => a.AccountNumber)
                    .ToListAsync();
                
                if (!string.IsNullOrEmpty(searchDto.AccountNumber) && !accountNumbers.Contains(searchDto.AccountNumber))
                {
                    throw new UnauthorizedAccessException("Account does not belong to the customer");
                }

                // Build the query
                var query = _dbContext.DepositTransactions.AsQueryable();
                
                // Filter by account
                if (!string.IsNullOrEmpty(searchDto.AccountNumber))
                {
                    query = query.Where(t => t.AccountNumber == searchDto.AccountNumber);
                }
                else
                {
                    query = query.Where(t => t.AccountNumber != null && accountNumbers.Contains(t.AccountNumber));
                }
                
                // Apply date filters
                if (searchDto.StartDate.HasValue)
                {
                    query = query.Where(t => t.TransactionDate >= searchDto.StartDate.Value);
                }
                
                if (searchDto.EndDate.HasValue)
                {
                    var endDate = searchDto.EndDate.Value.Date.AddDays(1).AddSeconds(-1);
                    query = query.Where(t => t.TransactionDate <= endDate);
                }
                
                // Apply amount filters
                if (searchDto.MinAmount.HasValue)
                {
                    query = query.Where(t => t.Amount >= searchDto.MinAmount.Value);
                }
                
                if (searchDto.MaxAmount.HasValue)
                {
                    query = query.Where(t => t.Amount <= searchDto.MaxAmount.Value);
                }
                
                // Filter by transaction type
                if (!string.IsNullOrEmpty(searchDto.TransactionType) && Enum.TryParse<TransactionType>(searchDto.TransactionType, out var typeEnum))
                {
                    query = query.Where(t => t.TransactionType == typeEnum);
                }
                
                // Search in description, reference, beneficiary
                if (!string.IsNullOrEmpty(searchDto.SearchTerm))
                {
                    var searchTerm = searchDto.SearchTerm.ToLower();
                    query = query.Where(t => 
                        (t.Description != null && t.Description.ToLower().Contains(searchTerm)) ||
                        (t.TransactionReference != null && t.TransactionReference.ToLower().Contains(searchTerm)) ||
                        (t.ExternalReference != null && t.ExternalReference.ToLower().Contains(searchTerm)) ||
                        (t.BeneficiaryName != null && t.BeneficiaryName.ToLower().Contains(searchTerm)) ||
                        (t.BeneficiaryAccountNumber != null && t.BeneficiaryAccountNumber.Contains(searchTerm))
                    );
                }
                
                // Filter by category
                if (!string.IsNullOrEmpty(searchDto.Category))
                {
                    query = query.Where(t => t.Category == searchDto.Category);
                }
                
                // Filter by channel
                if (!string.IsNullOrEmpty(searchDto.Channel))
                {
                    query = query.Where(t => t.Channel == searchDto.Channel);
                }

                // Get total count for pagination
                var totalCount = await query.CountAsync();
                
                // Apply pagination
                var transactions = await query
                    .OrderByDescending(t => t.TransactionDate)
                    .Skip((searchDto.Page - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize)
                    .ToListAsync();
                
                return (transactions, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching transactions for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<IEnumerable<string>> GetTransactionCategoriesAsync(string accountNumber)
        {
            try
            {
                return await _dbContext.DepositTransactions
                    .Where(t => t.AccountNumber == accountNumber && !string.IsNullOrEmpty(t.Category))
                    .Select(t => t.Category!)
                    .Distinct()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transaction categories for account {AccountNumber}", accountNumber);
                throw;
            }
        }

        public async Task<IEnumerable<string>> GetTransactionChannelsAsync(string accountNumber)
        {
            try
            {
                return await _dbContext.DepositTransactions
                    .Where(t => t.AccountNumber == accountNumber && !string.IsNullOrEmpty(t.Channel))
                    .Select(t => t.Channel!)
                    .Distinct()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transaction channels for account {AccountNumber}", accountNumber);
                throw;
            }
        }

        public async Task<byte[]> ExportTransactionsAsync(TransactionExportDto exportDto, Guid customerId)
        {
            try
            {
                // Verify account belongs to customer
                var account = await _dbContext.DepositAccounts
                    .FirstOrDefaultAsync(a => a.AccountNumber == exportDto.AccountNumber && a.CustomerId == customerId);
                
                if (account == null)
                {
                    throw new UnauthorizedAccessException("Account does not belong to the customer");
                }

                // Get transactions
                var transactions = await _dbContext.DepositTransactions
                    .Where(t => t.AccountNumber == exportDto.AccountNumber &&
                           t.TransactionDate >= exportDto.StartDate &&
                           t.TransactionDate <= exportDto.EndDate)
                    .OrderByDescending(t => t.TransactionDate)
                    .ToListAsync();

                // In a real implementation, this would use a reporting service to generate the export file
                // For this example, we'll return a dummy byte array
                return new byte[100]; // Placeholder implementation
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting transactions for account {AccountNumber}", exportDto.AccountNumber);
                throw;
            }
        }

        public async Task<Dictionary<string, decimal>> GetCategoryBreakdownAsync(string accountNumber, DateTime startDate, DateTime endDate)
        {
            try
            {
                var transactions = await _dbContext.DepositTransactions
                    .Where(t => t.AccountNumber == accountNumber &&
                           t.TransactionDate >= startDate &&
                           t.TransactionDate <= endDate &&
                           !string.IsNullOrEmpty(t.Category) &&
                           (t.TransactionType == TransactionType.Withdrawal || 
                            t.TransactionType == TransactionType.ChargeDebit || 
                            t.TransactionType == TransactionType.Transfer)) // Only consider expenses
                    .ToListAsync();

                return transactions
                    .GroupBy(t => t.Category ?? "Uncategorized")
                    .ToDictionary(
                        g => g.Key,
                        g => g.Sum(t => t.Amount)
                    );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category breakdown for account {AccountNumber}", accountNumber);
                throw;
            }
        }

        public async Task<IEnumerable<DepositTransaction>> GetLargestTransactionsAsync(Guid customerId, int count = 5)
        {
            try
            {
                var accountNumbers = await _dbContext.DepositAccounts
                    .Where(a => a.CustomerId == customerId)
                    .Select(a => a.AccountNumber)
                    .ToListAsync();

                return await _dbContext.DepositTransactions
                    .Where(t => accountNumbers.Contains(t.AccountNumber))
                    .OrderByDescending(t => t.Amount)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving largest transactions for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<Dictionary<string, decimal>> GetMonthlySpendingAsync(string accountNumber, int months = 6)
        {
            try
            {
                var startDate = DateTime.UtcNow.AddMonths(-months).Date.AddDays(-(DateTime.UtcNow.Day - 1));
                var endDate = DateTime.UtcNow;

                var transactions = await _dbContext.DepositTransactions
                    .Where(t => t.AccountNumber == accountNumber &&
                           t.TransactionDate >= startDate &&
                           t.TransactionDate <= endDate &&
                           (t.TransactionType == TransactionType.Withdrawal || 
                            t.TransactionType == TransactionType.ChargeDebit || 
                            t.TransactionType == TransactionType.Transfer)) // Only consider expenses
                    .ToListAsync();

                return transactions
                    .GroupBy(t => new { Year = t.TransactionDate.Year, Month = t.TransactionDate.Month })
                    .OrderBy(g => g.Key.Year)
                    .ThenBy(g => g.Key.Month)
                    .ToDictionary(
                        g => $"{g.Key.Year}-{g.Key.Month:D2}",
                        g => g.Sum(t => t.Amount)
                    );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving monthly spending for account {AccountNumber}", accountNumber);
                throw;
            }
        }
    }
}
