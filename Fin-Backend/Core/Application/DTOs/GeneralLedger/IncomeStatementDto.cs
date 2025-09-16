using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.GeneralLedger
{
    public class IncomeStatementDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime GeneratedDate { get; set; }
        public IEnumerable<FinancialStatementItemDto> Income { get; set; }
        public decimal TotalIncome { get; set; }
        public IEnumerable<FinancialStatementItemDto> Expenses { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetIncome => TotalIncome - TotalExpenses;
    }
}