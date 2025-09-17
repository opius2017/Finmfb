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
    public class LoanCollectionRepository : ILoanCollectionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LoanCollectionRepository> _logger;

        public LoanCollectionRepository(
            ApplicationDbContext context,
            ILogger<LoanCollectionRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<LoanCollection>> GetAllAsync()
        {
            try
            {
                return await _context.LoanCollections
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all loan collections");
                throw;
            }
        }

        public async Task<LoanCollection> GetByIdAsync(string id)
        {
            try
            {
                return await _context.LoanCollections
                    .FirstOrDefaultAsync(lc => lc.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting loan collection with ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<LoanCollection>> GetByLoanIdAsync(string loanId)
        {
            try
            {
                return await _context.LoanCollections
                    .Where(lc => lc.LoanId == loanId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting collections for loan ID: {LoanId}", loanId);
                throw;
            }
        }

        public async Task<IEnumerable<LoanCollection>> GetOverdueCollectionsAsync(int daysOverdue)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-daysOverdue);
                
                return await _context.LoanCollections
                    .Where(lc => lc.Status == CollectionStatus.Pending && 
                                lc.DueDate < cutoffDate)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting overdue collections for {DaysOverdue} days", daysOverdue);
                throw;
            }
        }

        public async Task<LoanCollection> AddAsync(LoanCollection collection)
        {
            try
            {
                _context.LoanCollections.Add(collection);
                await _context.SaveChangesAsync();
                return collection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding loan collection");
                throw;
            }
        }

        public async Task<LoanCollection> UpdateAsync(LoanCollection collection)
        {
            try
            {
                _context.Entry(collection).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return collection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating loan collection with ID: {Id}", collection.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var collection = await _context.LoanCollections.FindAsync(id);
                if (collection == null)
                {
                    return false;
                }

                _context.LoanCollections.Remove(collection);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting loan collection with ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<LoanCollection>> GetByAgentIdAsync(string agentId)
        {
            try
            {
                return await _context.LoanCollections
                    .Where(lc => lc.CollectionAgentId == agentId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting collections for agent ID: {AgentId}", agentId);
                throw;
            }
        }

        public async Task<IEnumerable<LoanCollection>> GetCollectionsByStatusAsync(CollectionStatus status)
        {
            try
            {
                return await _context.LoanCollections
                    .Where(lc => lc.Status == status)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting collections with status: {Status}", status);
                throw;
            }
        }

        public async Task<IEnumerable<LoanCollection>> GetCollectionsInDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _context.LoanCollections
                    .Where(lc => lc.DueDate >= startDate && lc.DueDate <= endDate)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting collections in date range: {StartDate} to {EndDate}", 
                    startDate, endDate);
                throw;
            }
        }
    }
}