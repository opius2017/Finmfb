using System;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    public class AccountDto
    {
        public Guid Id { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string ProductName { get; set; }
        public string ProductType { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal AvailableBalance { get; set; }
        public string Currency { get; set; } = "NGN";
        public string Status { get; set; }
        public DateTime OpenDate { get; set; }
        public DateTime? LastActivityDate { get; set; }
    }
}
