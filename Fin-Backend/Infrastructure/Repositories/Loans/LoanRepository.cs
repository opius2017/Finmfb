using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinTech.Core.Application.Interfaces.Repositories.Loans;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinTech.Infrastructure.Repositories.Loans
{
    public class LoanRepository : ILoanRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LoanRepository> _logger;

        public LoanRepository(
            ApplicationDbContext context,
            ILogger<LoanRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<Loan>> GetAllAsync()
        {
            try
            {
                return await _context.Loans
                    .Include(l => l.Transactions)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all loans");
                throw;
            }
        }

        public async Task<Loan> GetByIdAsync(string id)
        {
            try
            {
                return await _context.Loans
                    .Include(l => l.Transactions)
                    .FirstOrDefaultAsync(l => l.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting loan with ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Loan>> GetByCustomerIdAsync(string customerId)
        {
            try
            {
                return await _context.Loans
                    .Include(l => l.Transactions)
                    .Where(l => l.CustomerId == customerId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting loans for customer ID: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<Loan> AddAsync(Loan loan)
        {
            try
            {
                _context.Loans.Add(loan);
                await _context.SaveChangesAsync();
                return loan;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding loan");
                throw;
            }
        }

        public async Task<Loan> UpdateAsync(Loan loan)
        {
            try
            {
                _context.Entry(loan).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return loan;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating loan with ID: {Id}", loan.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var loan = await _context.Loans.FindAsync(id);
                if (loan == null)
                {
                    return false;
                }

                _context.Loans.Remove(loan);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting loan with ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<LoanTransaction>> GetLoanTransactionsAsync(string loanId)
        {
            try
            {
                return await _context.LoanTransactions
                    .Where(lt => lt.LoanId == Guid.Parse(loanId))
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

        public async Task<IEnumerable<LoanRepaymentSchedule>> GetLoanRepaymentScheduleAsync(string loanId)
        {
            try
            {
                return await _context.LoanRepaymentSchedules
                    .Where(lrs => lrs.LoanId == Guid.Parse(loanId))
                    .OrderBy(lrs => lrs.DueDate)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting repayment schedule for loan ID: {LoanId}", loanId);
                throw;
            }
        }
    }
}
