using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.GeneralLedger.Regulatory
{
    public class NDICReturnsDto
    {
        public DateTime AsOfDate { get; set; }
        public DateTime GeneratedDate { get; set; }
        public decimal TotalDeposits { get; set; }
        public decimal InsuredDeposits { get; set; }
        public decimal PremiumPayable { get; set; }
        public IEnumerable<NDICReturnItemDto> ReturnItems { get; set; }
    }
}
