using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Accounting;

namespace FinTech.Core.Domain.Repositories.Accounting
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
            
        /// <summary>
        /// Gets all unposted journal entries for a specific financial period
        /// </summary>
        Task<IReadOnlyList<JournalEntry>> GetUnpostedEntriesByPeriodAsync(
            string financialPeriodId, 
            CancellationToken cancellationToken = default);
            
        /// <summary>
        /// Gets all journal entries with a specific reference type for a financial period
        /// </summary>
        Task<IReadOnlyList<JournalEntry>> GetByReferenceAsync(
            string financialPeriodId, 
            string referenceType, 
            CancellationToken cancellationToken = default);
    }
}
