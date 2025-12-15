using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Accounting;
using FinTech.Core.Application.DTOs.GeneralLedger.Journal;

namespace FinTech.Core.Application.Interfaces.Services.Accounting
{
    public interface IJournalEntryService
    {
        Task<JournalEntry> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<JournalEntry> GetByJournalNumberAsync(string journalNumber, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<JournalEntry>> GetByStatusAsync(JournalEntryStatus status, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<JournalEntry>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<JournalEntry>> GetByFinancialPeriodAsync(string financialPeriodId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<JournalEntry>> GetByAccountIdAsync(string accountId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<JournalEntry>> GetPendingApprovalsAsync(CancellationToken cancellationToken = default);
        Task<string> CreateJournalEntryAsync(JournalEntry journalEntry, CancellationToken cancellationToken = default);
        Task<string> CreateJournalEntryAsync(JournalEntryDto journalEntryDto, string tenantId = null, CancellationToken cancellationToken = default);
        Task<string> CreateJournalEntryAsync(object journalEntry, string tenantId, CancellationToken cancellationToken = default);
        Task UpdateJournalEntryAsync(JournalEntry journalEntry, CancellationToken cancellationToken = default);
        Task<string> SubmitForApprovalAsync(string id, string submittedBy, CancellationToken cancellationToken = default);
        Task<string> ApproveJournalEntryAsync(string id, string approvedBy, CancellationToken cancellationToken = default);
        Task<string> RejectJournalEntryAsync(string id, string rejectedBy, string rejectionReason, CancellationToken cancellationToken = default);
        Task<string> PostJournalEntryAsync(string id, string postedBy, CancellationToken cancellationToken = default);
        Task<string> ReverseJournalEntryAsync(string id, string reversedBy, string reversalReason, CancellationToken cancellationToken = default);
        Task<string> GenerateJournalNumberAsync(JournalEntryType entryType, CancellationToken cancellationToken = default);
    }
}
