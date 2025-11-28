using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FinTech.Core.Application.DTOs.ClientPortal
{


    // Client Account DTOs
    public class ClientAccountDto
    {
        public Guid Id { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string AccountType { get; set; }
        public decimal AvailableBalance { get; set; }
        public decimal BookBalance { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public DateTime OpenDate { get; set; }
        public DateTime LastTransactionDate { get; set; }
    }
}