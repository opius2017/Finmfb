using System;
using System.Collections.Generic;

namespace FinTech.Application.Services
{
    public class JournalEntryDto
    {
        public int Id { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Reference { get; set; }
        public string Description { get; set; }
        public string Source { get; set; }
        public bool IsPosted { get; set; }
        public DateTime? PostedDate { get; set; }
        public List<JournalEntryLineDto> Lines { get; set; } = new List<JournalEntryLineDto>();
    }

    public class JournalEntryLineDto
    {
        public int Id { get; set; }
        public int JournalEntryId { get; set; }
        public int AccountId { get; set; }
        public string Description { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
    }
}