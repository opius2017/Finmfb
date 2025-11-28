using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    // Transfer DTOs
    public class TransferDto
    {
        public Guid Id { get; set; }
        public string ReferenceNumber { get; set; }
        public string SourceAccountNumber { get; set; }
        public string DestinationAccountNumber { get; set; }
        public decimal Amount { get; set; }
        public string TransferType { get; set; }
        public string BeneficiaryName { get; set; }
        public string Description { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Status { get; set; }
        public decimal Charges { get; set; }
    }
}