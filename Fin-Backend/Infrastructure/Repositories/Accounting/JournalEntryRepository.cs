using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Domain.Entities.Accounting;
using FinTech.Domain.Repositories.Accounting;
using FinTech.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinTech.Infrastructure.Repositories.Accounting
{
    /// <summary>
    /// Repository implementation for Journal Entries
    /// </summary>
    public class JournalEntryRepository : Repository<JournalEntry>, IJournalEntryRepository
    {
        public JournalEntryRepository(ApplicationDbContext context) : base(context)
        {
        }
        
        public async Task<JournalEntry> GetByJournalNumberAsync(
            string journalNumber, 
            CancellationToken cancellationToken = default)
        {
            return await _context.JournalEntries
                .Include(j => j.JournalEntryLines)
                    .ThenInclude(l => l.Account)
                .FirstOrDefaultAsync(j => j.JournalEntryNumber == journalNumber, cancellationToken);
        }
        
        public async Task<IReadOnlyList<JournalEntry>> GetByStatusAsync(
            JournalEntryStatus status, 
            CancellationToken cancellationToken = default)
        {
            return await _context.JournalEntries
                .Where(j => j.Status == status)
                .OrderByDescending(j => j.EntryDate)
                .ToListAsync(cancellationToken);
        }
        
        public async Task<IReadOnlyList<JournalEntry>> GetByDateRangeAsync(
            DateTime startDate, 
            DateTime endDate, 
            CancellationToken cancellationToken = default)
        {
            return await _context.JournalEntries
                .Where(j => j.EntryDate >= startDate && j.EntryDate <= endDate)
                .OrderByDescending(j => j.EntryDate)
                .ToListAsync(cancellationToken);
        }
        
        public async Task<IReadOnlyList<JournalEntry>> GetByFinancialPeriodAsync(
            string financialPeriodId, 
            CancellationToken cancellationToken = default)
        {
            return await _context.JournalEntries
                .Where(j => j.FinancialPeriodId == financialPeriodId)
                .OrderByDescending(j => j.EntryDate)
                .ToListAsync(cancellationToken);
        }
        
        public async Task<IReadOnlyList<JournalEntry>> GetByAccountIdAsync(
            string accountId, 
            CancellationToken cancellationToken = default)
        {
            return await _context.JournalEntries
                .Include(j => j.JournalEntryLines)
                .Where(j => j.JournalEntryLines.Any(l => l.AccountId == accountId))
                .OrderByDescending(j => j.EntryDate)
                .ToListAsync(cancellationToken);
        }
        
        public async Task<IReadOnlyList<JournalEntry>> GetPendingApprovalsAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.JournalEntries
                .Where(j => j.Status == JournalEntryStatus.Pending)
                .OrderByDescending(j => j.EntryDate)
                .ToListAsync(cancellationToken);
        }
        
        public async Task<string> GenerateJournalNumberAsync(
            JournalEntryType entryType,
            CancellationToken cancellationToken = default)
        {
            // Get the current year and month
            var currentDate = DateTime.UtcNow;
            var year = currentDate.Year;
            var month = currentDate.Month;
            
            // Create a prefix based on the journal entry type
            string prefix;
            switch (entryType)
            {
                case JournalEntryType.Standard:
                    prefix = "JE";
                    break;
                case JournalEntryType.Recurring:
                    prefix = "RJ";
                    break;
                case JournalEntryType.Reversal:
                    prefix = "RV";
                    break;
                case JournalEntryType.SystemGenerated:
                    prefix = "SJ";
                    break;
                case JournalEntryType.YearEndClosing:
                    prefix = "YE";
                    break;
                default:
                    prefix = "JE";
                    break;
            }
            
            // Find the latest journal entry number for this type, year, and month
            var latestJournalEntry = await _context.JournalEntries
                .Where(j => 
                    j.JournalEntryNumber.StartsWith($"{prefix}-{year}-{month:D2}-") && 
                    j.EntryType == entryType)
                .OrderByDescending(j => j.JournalEntryNumber)
                .FirstOrDefaultAsync(cancellationToken);
            
            int sequence = 1;
            
            if (latestJournalEntry != null)
            {
                // Extract the sequence number from the journal entry number
                var parts = latestJournalEntry.JournalEntryNumber.Split('-');
                if (parts.Length == 4 && int.TryParse(parts[3], out int lastSequence))
                {
                    sequence = lastSequence + 1;
                }
            }
            
            // Format: JE-YYYY-MM-NNNN (e.g., JE-2023-01-0001)
            return $"{prefix}-{year}-{month:D2}-{sequence:D4}";
        }
    }
}