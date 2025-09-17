using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Application.Interfaces.Repositories.Loans;
using FinTech.Domain.Entities.Loans;
using FinTech.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinTech.Infrastructure.Repositories.Loans
{
    public class LoanCreditCheckRepository : ILoanCreditCheckRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LoanCreditCheckRepository> _logger;

        public LoanCreditCheckRepository(
            ApplicationDbContext context,
            ILogger<LoanCreditCheckRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<LoanCreditCheck>> GetAllAsync()
        {
            try
            {
                return await _context.LoanCreditChecks
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all loan credit checks");
                throw;
            }
        }

        public async Task<LoanCreditCheck> GetByIdAsync(string id)
        {
            try
            {
                return await _context.LoanCreditChecks
                    .FirstOrDefaultAsync(lcc => lcc.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting loan credit check with ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<LoanCreditCheck>> GetByLoanIdAsync(string loanId)
        {
            try
            {
                return await _context.LoanCreditChecks
                    .Where(lcc => lcc.LoanId == loanId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting credit checks for loan ID: {LoanId}", loanId);
                throw;
            }
        }

        public async Task<IEnumerable<LoanCreditCheck>> GetByCustomerIdAsync(string customerId)
        {
            try
            {
                return await _context.LoanCreditChecks
                    .Where(lcc => lcc.CustomerId == customerId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting credit checks for customer ID: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<LoanCreditCheck> AddAsync(LoanCreditCheck creditCheck)
        {
            try
            {
                _context.LoanCreditChecks.Add(creditCheck);
                await _context.SaveChangesAsync();
                return creditCheck;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding loan credit check");
                throw;
            }
        }

        public async Task<LoanCreditCheck> UpdateAsync(LoanCreditCheck creditCheck)
        {
            try
            {
                _context.Entry(creditCheck).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return creditCheck;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating loan credit check with ID: {Id}", creditCheck.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var creditCheck = await _context.LoanCreditChecks.FindAsync(id);
                if (creditCheck == null)
                {
                    return false;
                }

                _context.LoanCreditChecks.Remove(creditCheck);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting loan credit check with ID: {Id}", id);
                throw;
            }
        }

        public async Task<LoanCreditCheck> GetLatestCreditCheckForCustomerAsync(string customerId)
        {
            try
            {
                return await _context.LoanCreditChecks
                    .Where(lcc => lcc.CustomerId == customerId)
                    .OrderByDescending(lcc => lcc.CheckDate)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest credit check for customer ID: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<IEnumerable<LoanCreditCheck>> GetCreditChecksByStatusAsync(CreditCheckStatus status)
        {
            try
            {
                return await _context.LoanCreditChecks
                    .Where(lcc => lcc.Status == status)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting credit checks with status: {Status}", status);
                throw;
            }
        }
    }
}