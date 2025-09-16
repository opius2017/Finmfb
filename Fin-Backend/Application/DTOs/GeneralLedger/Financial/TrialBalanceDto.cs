using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.GeneralLedger.Financial
{
    public class TrialBalanceDto
    {
        public DateTime AsOfDate { get; set; }
        public DateTime GeneratedDate { get; set; }
        public IEnumerable<TrialBalanceItemDto> AccountBalances { get; set; }
        public decimal TotalDebits { get; set; }
        public decimal TotalCredits { get; set; }
    }
}