using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.GeneralLedger.Journal
{
    public class JournalEntryDto
    {
        public string Id { get; set; }
        public string JournalEntryNumber { get; set; }
        public DateTime EntryDate { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public int EntryType { get; set; }
        public string Reference { get; set; }
        public string SourceDocument { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string PostedBy { get; set; }
        public DateTime? PostedDate { get; set; }
        public string FinancialPeriodId { get; set; }
        public string ModuleSource { get; set; }
        public bool IsRecurring { get; set; }
        public string RecurrencePattern { get; set; }
        public string Notes { get; set; }
        public IEnumerable<JournalEntryLineDto> JournalEntryLines { get; set; }
    }
}
