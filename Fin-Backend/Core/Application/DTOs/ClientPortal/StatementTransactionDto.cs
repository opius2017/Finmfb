using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FinTech.Core.Application.DTOs.ClientPortal
{

    public class StatementTransactionDto
    {
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string ReferenceNumber { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public decimal Balance { get; set; }
    }
