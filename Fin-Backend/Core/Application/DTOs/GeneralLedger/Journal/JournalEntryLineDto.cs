using System;

namespace FinTech.Core.Application.DTOs.GeneralLedger.Journal
{
    public class JournalEntryLineDto
    {
        public string Id { get; set; } = string.Empty;
        public string JournalEntryId { get; set; } = string.Empty;
        public string JournalEntryNumber { get; set; } = string.Empty;
        public DateTime EntryDate { get; set; }
        public string AccountId { get; set; } = string.Empty;
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public bool IsDebit { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
    }
}
