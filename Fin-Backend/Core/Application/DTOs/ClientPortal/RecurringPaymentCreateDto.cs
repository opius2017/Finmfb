using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    // Transfer DTOs

    // Recurring Payment DTOs
    public class RecurringPaymentCreateDto
    {
        [Required]
        public string SourceAccountNumber { get; set; }
        
        [Required]
        [Range(0.01, 10000000)]
        public decimal Amount { get; set; }
        
        [Required]
        public string PaymentType { get; set; } // Transfer, Bill
        
        [Required]
        public string Frequency { get; set; } // Daily, Weekly, Monthly, Quarterly, Annually
        
        [Required]
        public DateTime StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        public string Description { get; set; }
        
        // For transfers
        public string DestinationAccountNumber { get; set; }
        public string DestinationBankName { get; set; }
        public string DestinationBankCode { get; set; }
        public string BeneficiaryName { get; set; }
        public string TransferType { get; set; }
        
        // For bill payments
        public Guid? BillerId { get; set; }
        public string CustomerReferenceNumber { get; set; }
    }
}