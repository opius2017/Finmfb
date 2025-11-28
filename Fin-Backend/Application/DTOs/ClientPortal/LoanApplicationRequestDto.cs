using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    // Loan Application Request DTOs
    public class LoanApplicationRequestDto
    {
        [Required]
        public Guid LoanProductId { get; set; }
        
        [Required]
        [Range(1000, 10000000)]
        public decimal RequestedAmount { get; set; }
        
        [Required]
        [Range(1, 60)]
        public int RequestedTenor { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Purpose { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string RepaymentSource { get; set; }
        
        public DateTime? PreferredDisbursementDate { get; set; }
        
        [Required]
        [Range(0, 1000000000)]
        public decimal MonthlyIncome { get; set; }
        
        [Range(0, 1000000000)]
        public decimal ExistingMonthlyDebt { get; set; }
        
        public List<LoanDocumentUploadDto> Documents { get; set; }
    }
}