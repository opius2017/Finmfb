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
    public class LoanGuarantorRepository : ILoanGuarantorRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LoanGuarantorRepository> _logger;

        public LoanGuarantorRepository(
            ApplicationDbContext context,
            ILogger<LoanGuarantorRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<LoanGuarantor>> GetAllAsync()
        {
            try
            {
                return await _context.LoanGuarantors
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all loan guarantors");
                throw;
            }
        }

        public async Task<LoanGuarantor> GetByIdAsync(string id)
        {
            try
            {
                return await _context.LoanGuarantors
                    .FirstOrDefaultAsync(lg => lg.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting loan guarantor with ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<LoanGuarantor>> GetByLoanIdAsync(string loanId)
        {
            try
            {
                return await _context.LoanGuarantors
                    .Where(lg => lg.LoanId == loanId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting guarantors for loan ID: {LoanId}", loanId);
                throw;
            }
        }

        public async Task<IEnumerable<LoanGuarantor>> GetByGuarantorIdAsync(string guarantorId)
        {
            try
            {
                return await _context.LoanGuarantors
                    .Where(lg => lg.GuarantorCustomerId == guarantorId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting loans guaranteed by customer ID: {GuarantorId}", guarantorId);
                throw;
            }
        }

        public async Task<LoanGuarantor> AddAsync(LoanGuarantor guarantor)
        {
            try
            {
                _context.LoanGuarantors.Add(guarantor);
                await _context.SaveChangesAsync();
                return guarantor;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding loan guarantor");
                throw;
            }
        }

        public async Task<LoanGuarantor> UpdateAsync(LoanGuarantor guarantor)
        {
            try
            {
                _context.Entry(guarantor).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return guarantor;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating loan guarantor with ID: {Id}", guarantor.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var guarantor = await _context.LoanGuarantors.FindAsync(id);
                if (guarantor == null)
                {
                    return false;
                }

                _context.LoanGuarantors.Remove(guarantor);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting loan guarantor with ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<LoanGuarantor>> GetApprovedGuarantorsAsync()
        {
            try
            {
                return await _context.LoanGuarantors
                    .Where(lg => lg.IsApproved)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting approved guarantors");
                throw;
            }
        }

        public async Task<IEnumerable<LoanGuarantor>> GetPendingGuarantorsAsync()
        {
            try
            {
                return await _context.LoanGuarantors
                    .Where(lg => !lg.IsApproved)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending guarantors");
                throw;
            }
        }
    }
}
