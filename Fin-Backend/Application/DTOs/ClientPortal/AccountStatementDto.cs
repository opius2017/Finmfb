using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FinTech.Core.Application.DTOs.ClientPortal
{

    // Account Statement DTOs
    public class AccountStatementDto
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public List<StatementTransactionDto> Transactions { get; set; }
    }
}