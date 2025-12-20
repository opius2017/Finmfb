using System;
using System.ComponentModel.DataAnnotations;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    public class LoanApplicationDto
    {
        [Required]
        public string LoanProductId { get; set; } = string.Empty;
        
        [Required]
        [Range(1000, 10000000)]
        public decimal RequestedAmount { get; set; }
        
        [Required]
        [Range(1, 60)]
        public int RequestedTenor { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Purpose { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string RepaymentSource { get; set; } = string.Empty;
        
        public DateTime? PreferredDisbursementDate { get; set; }
    }

    public class LoanEligibilityCheckDto
    {
        [Required]
        public string LoanProductId { get; set; } = string.Empty;
        
        [Required]
        [Range(1000, 10000000)]
        public decimal RequestedAmount { get; set; }
        
        [Required]
        [Range(1, 60)]
        public int RequestedTenor { get; set; }
        
        [Required]
        [Range(0, 1000000000)]
        public decimal MonthlyIncome { get; set; }
        
        [Range(0, 1000000000)]
        public decimal ExistingMonthlyDebt { get; set; }
    }

    public class LoanSimulationDto
    {
        [Required]
        public string LoanProductId { get; set; } = string.Empty;
        
        [Required]
        [Range(1000, 10000000)]
        public decimal Amount { get; set; }
        
        [Required]
        [Range(1, 60)]
        public int Tenor { get; set; }
    }

    public class LoanRepaymentDto
    {
        [Required]
        public string LoanAccountNumber { get; set; } = string.Empty;
        
        [Required]
        [Range(1, 10000000)]
        public decimal Amount { get; set; }
        
        [Required]
        public string PaymentMethod { get; set; } = string.Empty;
        
        [Required]
        public string SourceAccountNumber { get; set; } = string.Empty;
    }

    public class LoanAccountSummaryDto
    {
        public string Id { get; set; } = string.Empty;
        public string LoanNumber { get; set; } = string.Empty;
        public string LoanType { get; set; } = string.Empty;
        public decimal PrincipalAmount { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string LoanProductName { get; set; } = string.Empty;
        public decimal DisbursedAmount { get; set; }
        public DateTime DisbursementDate { get; set; }
        public decimal InterestRate { get; set; }
        public int Tenor { get; set; }
        public decimal OutstandingBalance { get; set; }
        public decimal NextPaymentAmount { get; set; }
        public DateTime NextPaymentDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class LoanApplicationSummaryDto
    {
        public string Id { get; set; } = string.Empty;
        public string LoanProductName { get; set; } = string.Empty;
        public decimal RequestedAmount { get; set; }
        public int RequestedTenor { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class LoanRepaymentScheduleDto
    {
        public int InstallmentNumber { get; set; }
        public DateTime DueDate { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class LoanTransactionDto
    {
        public string ReferenceNumber { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal PrincipalAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal PenaltyAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
