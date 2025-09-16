using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.GeneralLedger
{
    public class CashFlowStatementDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime GeneratedDate { get; set; }
        public IEnumerable<FinancialStatementItemDto> OperatingActivities { get; set; }
        public decimal NetOperatingCashFlow { get; set; }
        public IEnumerable<FinancialStatementItemDto> InvestingActivities { get; set; }
        public decimal NetInvestingCashFlow { get; set; }
        public IEnumerable<FinancialStatementItemDto> FinancingActivities { get; set; }
        public decimal NetFinancingCashFlow { get; set; }
        public decimal NetCashFlow => NetOperatingCashFlow + NetInvestingCashFlow + NetFinancingCashFlow;
        public decimal BeginningCashBalance { get; set; }
        public decimal EndingCashBalance { get; set; }
    }
}