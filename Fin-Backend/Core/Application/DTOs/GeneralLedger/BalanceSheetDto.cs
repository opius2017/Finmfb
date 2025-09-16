using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.GeneralLedger
{
    public class BalanceSheetDto
    {
        public DateTime AsOfDate { get; set; }
        public DateTime GeneratedDate { get; set; }
        public IEnumerable<FinancialStatementItemDto> Assets { get; set; }
        public decimal TotalAssets { get; set; }
        public IEnumerable<FinancialStatementItemDto> Liabilities { get; set; }
        public decimal TotalLiabilities { get; set; }
        public IEnumerable<FinancialStatementItemDto> Equity { get; set; }
        public decimal TotalEquity { get; set; }
        public bool IsBalanced => System.Math.Abs(TotalAssets - (TotalLiabilities + TotalEquity)) < 0.01m;
    }
}