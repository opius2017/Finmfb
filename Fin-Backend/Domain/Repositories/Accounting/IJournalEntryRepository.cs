using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Domain.Entities.Accounting;

namespace FinTech.Domain.Repositories.Accounting
{
    /// <summary>
    /// Repository interface for Journal Entries
    /// </summary>
    public interface IJournalEntryRepository : IRepository<JournalEntry>
    {
        Task<JournalEntry> GetByJournalNumberAsync(
            string journalNumber, 
            CancellationToken cancellationToken = default);
            
        Task<IReadOnlyList<JournalEntry>> GetByStatusAsync(
            JournalEntryStatus status, 
            CancellationToken cancellationToken = default);
            
        Task<IReadOnlyList<JournalEntry>> GetByDateRangeAsync(
            DateTime startDate, 
            DateTime endDate, 
            CancellationToken cancellationToken = default);
            
        Task<IReadOnlyList<JournalEntry>> GetByFinancialPeriodAsync(
            string financialPeriodId, 
            CancellationToken cancellationToken = default);
            
        Task<IReadOnlyList<JournalEntry>> GetByAccountIdAsync(
            string accountId, 
            CancellationToken cancellationToken = default);
            
        Task<IReadOnlyList<JournalEntry>> GetPendingApprovalsAsync(
            CancellationToken cancellationToken = default);
            
        Task<string> GenerateJournalNumberAsync(
            JournalEntryType entryType,
            CancellationToken cancellationToken = default);
    }
}