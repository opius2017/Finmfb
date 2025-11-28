using System;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    public class TransactionDto
    {
        public Guid Id { get; set; }
        public string TransactionId { get; set; }
        public string AccountNumber { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "NGN";
        public decimal RunningBalance { get; set; }
        public string ReferenceNumber { get; set; }
        public string Channel { get; set; }
        public string Status { get; set; }
        public string Category { get; set; }
        public string BeneficiaryName { get; set; }
        public string BeneficiaryAccountNumber { get; set; }
        public string BeneficiaryBank { get; set; }
    }
}
