using System;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    public class AccountBalanceDto
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal AvailableBalance { get; set; }
        public string Currency { get; set; } = "NGN";
        public DateTime AsOfDate { get; set; }
    }
}
