using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinTech.Application.Interfaces.Repositories.Loans;
using FinTech.Domain.Entities.Loans;
using FinTech.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinTech.Infrastructure.Repositories.Loans
{
    public class LoanTransactionRepository : ILoanTransactionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LoanTransactionRepository> _logger;

        public LoanTransactionRepository(
            ApplicationDbContext context,
            ILogger<LoanTransactionRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<LoanTransaction>> GetAllAsync()
        {
            try
            {
                return await _context.LoanTransactions
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all loan transactions");
                throw;
            }
        }

        public async Task<LoanTransaction> GetByIdAsync(string id)
        {
            try
            {
                return await _context.LoanTransactions
                    .FirstOrDefaultAsync(lt => lt.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting loan transaction with ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<LoanTransaction>> GetByLoanIdAsync(string loanId)
        {
            try
            {
                return await _context.LoanTransactions
                    .Where(lt => lt.LoanId == loanId)
                    .OrderBy(lt => lt.TransactionDate)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting transactions for loan ID: {LoanId}", loanId);
                throw;
            }
        }

        public async Task<LoanTransaction> AddAsync(LoanTransaction transaction)
        {
            try
            {
                _context.LoanTransactions.Add(transaction);
                await _context.SaveChangesAsync();
                return transaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding loan transaction");
                throw;
            }
        }

        public async Task<LoanTransaction> UpdateAsync(LoanTransaction transaction)
        {
            try
            {
                _context.Entry(transaction).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return transaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating loan transaction with ID: {Id}", transaction.Id);
                throw;
            }
        }
    }
}