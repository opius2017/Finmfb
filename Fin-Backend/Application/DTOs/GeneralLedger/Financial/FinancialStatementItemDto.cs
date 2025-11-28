using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.GeneralLedger.Financial
{
    public class FinancialStatementItemDto
    {
        public string AccountId { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
        public decimal Amount { get; set; }
        public string ParentCategory { get; set; }
        public string SubCategory { get; set; }
        public int SortOrder { get; set; }
        public bool IsBold { get; set; }
        public bool IsSubtotal { get; set; }
        public int IndentLevel { get; set; }
    }
}
