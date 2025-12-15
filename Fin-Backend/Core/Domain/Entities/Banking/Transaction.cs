using System;
using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Banking
{
    public class Transaction : BaseEntity
    {
        public string BankAccountId { get; private set; }
        public string TransactionType { get; private set; }
        public decimal Amount { get; private set; }
        public string Reference { get; private set; }
        public string Description { get; private set; }
        public decimal RunningBalance { get; private set; }
        public DateTime TransactionDate { get; private set; }
        public string Status { get; private set; }

        private Transaction() 
        {
            BankAccountId = string.Empty;
            TransactionType = string.Empty;
            Reference = string.Empty;
            Description = string.Empty;
            Status = string.Empty;
        } // For EF Core

        public Transaction(
            string bankAccountId,
            string transactionType,
            decimal amount,
            string reference,
            string description,
            decimal runningBalance)
        {
            BankAccountId = bankAccountId;
            TransactionType = transactionType;
            Amount = amount;
            Reference = reference;
            Description = description;
            RunningBalance = runningBalance;
            TransactionDate = DateTime.UtcNow;
            Status = "COMPLETED";
        }

        public void UpdateStatus(string status)
        {
            Status = status;
        }
    }
}
