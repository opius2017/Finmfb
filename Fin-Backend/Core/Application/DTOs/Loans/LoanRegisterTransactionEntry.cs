using System;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class LoanRegisterTransactionEntry
    {
        public string EntryId { get; set; }
        public DateTime RegisterDate { get; set; }
        public string EntryType { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public string TransactionReference { get; set; }
        public string RecordedBy { get; set; }
    }
}
