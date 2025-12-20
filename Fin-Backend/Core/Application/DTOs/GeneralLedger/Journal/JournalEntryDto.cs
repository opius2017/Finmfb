using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.GeneralLedger.Journal
{
    public class JournalEntryDto
    {
        public string Id { get; set; } = string.Empty;
        public string JournalEntryNumber { get; set; } = string.Empty;
        public DateTime EntryDate { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Status { get; set; }
        public int EntryType { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string SourceDocument { get; set; } = string.Empty;
        public string ApprovedBy { get; set; } = string.Empty;
        public DateTime? ApprovalDate { get; set; }
        public string PostedBy { get; set; } = string.Empty;
        public DateTime? PostedDate { get; set; }
        public string FinancialPeriodId { get; set; } = string.Empty;
        public string ModuleSource { get; set; } = string.Empty;
        public bool IsRecurring { get; set; }
        public string RecurrencePattern { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public IEnumerable<JournalEntryLineDto> JournalEntryLines { get; set; } = new List<JournalEntryLineDto>();
        public string Source { get; set; } = string.Empty;
        public IEnumerable<JournalEntryLineDto> Lines { get; set; } = new List<JournalEntryLineDto>();
    }
}
