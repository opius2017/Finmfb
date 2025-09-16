using System;

namespace FinTech.Core.Application.DTOs.GeneralLedger.Journal
{
    public class JournalEntryLineDto
    {
        public string Id { get; set; }
        public string JournalEntryId { get; set; }
        public string JournalEntryNumber { get; set; }
        public DateTime EntryDate { get; set; }
        public string AccountId { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
        public bool IsDebit { get; set; }
        public string Description { get; set; }
        public string Reference { get; set; }
    }
}