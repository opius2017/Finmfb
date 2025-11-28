using System;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    public class SavingsGoalTransactionDto
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string TransactionType { get; set; }
        public string SourceAccountNumber { get; set; }
        public string DestinationAccountNumber { get; set; }
        public string Reference { get; set; }
        public string Status { get; set; }
        public bool IsAutomatic { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
