using System;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class BankTransferResult
    {
        public bool Success { get; set; }
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string RecipientAccount { get; set; }
        public string RecipientName { get; set; }
        public DateTime TransferDate { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }
}
