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
    public class LoanApplicationRepository : ILoanApplicationRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LoanApplicationRepository> _logger;

        public LoanApplicationRepository(
            ApplicationDbContext context,
            ILogger<LoanApplicationRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<LoanApplication>> GetAllAsync()
        {
            try
            {
                return await _context.LoanApplications
                    .Include(la => la.LoanProduct)
                    .Include(la => la.Guarantors)
                    .Include(la => la.Collaterals)
                    .Include(la => la.Documents)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all loan applications");
                throw;
            }
        }

        public async Task<LoanApplication> GetByIdAsync(string id)
        {
            try
            {
                return await _context.LoanApplications
                    .Include(la => la.LoanProduct)
                    .Include(la => la.Guarantors)
                    .Include(la => la.Collaterals)
                    .Include(la => la.Documents)
                    .FirstOrDefaultAsync(la => la.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting loan application with ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<LoanApplication>> GetByCustomerIdAsync(string customerId)
        {
            try
            {
                return await _context.LoanApplications
                    .Include(la => la.LoanProduct)
                    .Include(la => la.Guarantors)
                    .Include(la => la.Collaterals)
                    .Include(la => la.Documents)
                    .Where(la => la.CustomerId == customerId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting loan applications for customer ID: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<LoanApplication> AddAsync(LoanApplication loanApplication)
        {
            try
            {
                _context.LoanApplications.Add(loanApplication);
                await _context.SaveChangesAsync();
                return loanApplication;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding loan application");
                throw;
            }
        }

        public async Task<LoanApplication> UpdateAsync(LoanApplication loanApplication)
        {
            try
            {
                _context.Entry(loanApplication).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return loanApplication;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating loan application with ID: {Id}", loanApplication.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var loanApplication = await _context.LoanApplications.FindAsync(id);
                if (loanApplication == null)
                {
                    return false;
                }

                _context.LoanApplications.Remove(loanApplication);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting loan application with ID: {Id}", id);
                throw;
            }
        }
    }
}