using System;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    public class AccountActivityDto
    {
        public DateTime Date { get; set; }
        public decimal TotalCredits { get; set; }
        public decimal TotalDebits { get; set; }
        public int TransactionCount { get; set; }
    }
}
