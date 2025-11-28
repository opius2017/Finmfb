using System;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    public class TransactionSearchDto
    {
        public string AccountNumber { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public string TransactionType { get; set; } // Credit, Debit
        public string SearchTerm { get; set; } // Search in description, reference, beneficiary
        public string Category { get; set; }
        public string Channel { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
