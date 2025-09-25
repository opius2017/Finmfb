using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.Interfaces.Repositories.Loans;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinTech.Infrastructure.Repositories.Loans
{
    public class LoanFeeRepository : ILoanFeeRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LoanFeeRepository> _logger;

        public LoanFeeRepository(
            ApplicationDbContext context,
            ILogger<LoanFeeRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<LoanFee>> GetAllAsync()
        {
            try
            {
                return await _context.LoanFees
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all loan fees");
                throw;
            }
        }

        public async Task<LoanFee> GetByIdAsync(string id)
        {
            try
            {
                return await _context.LoanFees
                    .FirstOrDefaultAsync(lf => lf.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting loan fee with ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<LoanFee>> GetByLoanIdAsync(string loanId)
        {
            try
            {
                return await _context.LoanFees
                    .Where(lf => lf.LoanId == loanId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting fees for loan ID: {LoanId}", loanId);
                throw;
            }
        }

        public async Task<IEnumerable<LoanFee>> GetByLoanProductIdAsync(string loanProductId)
        {
            try
            {
                return await _context.LoanFees
                    .Where(lf => lf.LoanProductId == loanProductId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting fees for loan product ID: {LoanProductId}", loanProductId);
                throw;
            }
        }

        public async Task<LoanFee> AddAsync(LoanFee fee)
        {
            try
            {
                _context.LoanFees.Add(fee);
                await _context.SaveChangesAsync();
                return fee;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding loan fee");
                throw;
            }
        }

        public async Task<LoanFee> UpdateAsync(LoanFee fee)
        {
            try
            {
                _context.Entry(fee).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return fee;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating loan fee with ID: {Id}", fee.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var fee = await _context.LoanFees.FindAsync(id);
                if (fee == null)
                {
                    return false;
                }

                _context.LoanFees.Remove(fee);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting loan fee with ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<LoanFee>> GetByFeeTypeAsync(string feeType)
        {
            try
            {
                return await _context.LoanFees
                    .Where(lf => lf.FeeType == feeType)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting fees by type: {FeeType}", feeType);
                throw;
            }
        }
    }
}
