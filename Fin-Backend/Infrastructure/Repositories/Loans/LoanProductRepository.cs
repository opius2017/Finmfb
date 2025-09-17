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
    public class LoanProductRepository : ILoanProductRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LoanProductRepository> _logger;

        public LoanProductRepository(
            ApplicationDbContext context,
            ILogger<LoanProductRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<LoanProduct>> GetAllAsync()
        {
            try
            {
                return await _context.LoanProducts
                    .Include(lp => lp.Fees)
                    .Include(lp => lp.RequiredDocuments)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all loan products");
                throw;
            }
        }

        public async Task<LoanProduct> GetByIdAsync(string id)
        {
            try
            {
                return await _context.LoanProducts
                    .Include(lp => lp.Fees)
                    .Include(lp => lp.RequiredDocuments)
                    .FirstOrDefaultAsync(lp => lp.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting loan product with ID: {Id}", id);
                throw;
            }
        }

        public async Task<LoanProduct> AddAsync(LoanProduct loanProduct)
        {
            try
            {
                _context.LoanProducts.Add(loanProduct);
                await _context.SaveChangesAsync();
                return loanProduct;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding loan product");
                throw;
            }
        }

        public async Task<LoanProduct> UpdateAsync(LoanProduct loanProduct)
        {
            try
            {
                _context.Entry(loanProduct).State = EntityState.Modified;
                
                // Handle related collections
                if (loanProduct.Fees != null)
                {
                    foreach (var fee in loanProduct.Fees)
                    {
                        if (string.IsNullOrEmpty(fee.Id))
                        {
                            _context.Entry(fee).State = EntityState.Added;
                        }
                        else
                        {
                            _context.Entry(fee).State = EntityState.Modified;
                        }
                    }
                }
                
                if (loanProduct.RequiredDocuments != null)
                {
                    foreach (var doc in loanProduct.RequiredDocuments)
                    {
                        if (string.IsNullOrEmpty(doc.Id))
                        {
                            _context.Entry(doc).State = EntityState.Added;
                        }
                        else
                        {
                            _context.Entry(doc).State = EntityState.Modified;
                        }
                    }
                }
                
                await _context.SaveChangesAsync();
                return loanProduct;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating loan product with ID: {Id}", loanProduct.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var loanProduct = await _context.LoanProducts.FindAsync(id);
                if (loanProduct == null)
                {
                    return false;
                }

                _context.LoanProducts.Remove(loanProduct);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting loan product with ID: {Id}", id);
                throw;
            }
        }
    }
}