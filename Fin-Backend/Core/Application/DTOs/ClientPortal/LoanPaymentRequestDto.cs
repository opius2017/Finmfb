using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    // Loan Payment DTOs
    public class LoanPaymentRequestDto
    {
        [Required]
        public string LoanAccountNumber { get; set; }
        
        [Required]
        [Range(1, 10000000)]
        public decimal Amount { get; set; }
        
        [Required]
        public string PaymentMethod { get; set; }
        
        [Required]
        public string SourceAccountNumber { get; set; }
        
        public string Description { get; set; }
    }
}