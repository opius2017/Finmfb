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
    public class LoanRepaymentScheduleRepository : ILoanRepaymentScheduleRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LoanRepaymentScheduleRepository> _logger;

        public LoanRepaymentScheduleRepository(
            ApplicationDbContext context,
            ILogger<LoanRepaymentScheduleRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<LoanRepaymentSchedule>> GetAllAsync()
        {
            try
            {
                return await _context.LoanRepaymentSchedules
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all loan repayment schedules");
                throw;
            }
        }

        public async Task<LoanRepaymentSchedule> GetByIdAsync(string id)
        {
            try
            {
                return await _context.LoanRepaymentSchedules
                    .FirstOrDefaultAsync(lrs => lrs.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting loan repayment schedule with ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<LoanRepaymentSchedule>> GetByLoanIdAsync(string loanId)
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
                _logger.LogError(ex, "Error getting repayment schedules for loan ID: {LoanId}", loanId);
                throw;
            }
        }

        public async Task<LoanRepaymentSchedule> AddAsync(LoanRepaymentSchedule schedule)
        {
            try
            {
                _context.LoanRepaymentSchedules.Add(schedule);
                await _context.SaveChangesAsync();
                return schedule;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding loan repayment schedule");
                throw;
            }
        }

        public async Task<LoanRepaymentSchedule> UpdateAsync(LoanRepaymentSchedule schedule)
        {
            try
            {
                _context.Entry(schedule).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return schedule;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating loan repayment schedule with ID: {Id}", schedule.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var schedule = await _context.LoanRepaymentSchedules.FindAsync(id);
                if (schedule == null)
                {
                    return false;
                }

                _context.LoanRepaymentSchedules.Remove(schedule);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting loan repayment schedule with ID: {Id}", id);
                throw;
            }
        }
    }
}
