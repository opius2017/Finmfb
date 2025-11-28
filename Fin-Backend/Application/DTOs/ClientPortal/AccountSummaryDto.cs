using System;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    public class AccountSummaryDto
    {
        public decimal TotalBalance { get; set; }
        public decimal TotalSavings { get; set; }
        public decimal TotalCurrent { get; set; }
        public decimal TotalFixed { get; set; }
        public int AccountCount { get; set; }
        public DateTime LastUpdateTime { get; set; }
    }
}
