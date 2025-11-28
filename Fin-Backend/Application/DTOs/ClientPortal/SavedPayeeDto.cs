using System;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    public class SavedPayeeDto
    {
        public Guid Id { get; set; }
        public string PayeeName { get; set; }
        public string AccountNumber { get; set; }
        public string BankName { get; set; }
        public string BankCode { get; set; }
        public string PayeeType { get; set; }
        public string Reference { get; set; }
        public string Category { get; set; }
        public bool IsFavorite { get; set; }
        public int TransactionCount { get; set; }
        public DateTime? LastUsed { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
