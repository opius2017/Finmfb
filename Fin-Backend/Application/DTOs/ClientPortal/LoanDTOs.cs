using System;
using System.ComponentModel.DataAnnotations;

namespace FinTech.Application.DTOs.ClientPortal
{
    public class LoanApplicationDto
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
    }

    public class LoanEligibilityCheckDto
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
        [Range(0, 1000000000)]
        public decimal MonthlyIncome { get; set; }
        
        [Range(0, 1000000000)]
        public decimal ExistingMonthlyDebt { get; set; }
    }

    public class LoanSimulationDto
    {
        [Required]
        public Guid LoanProductId { get; set; }
        
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
        public string LoanAccountNumber { get; set; }
        
        [Required]
        [Range(1, 10000000)]
        public decimal Amount { get; set; }
        
        [Required]
        public string PaymentMethod { get; set; }
        
        [Required]
        public string SourceAccountNumber { get; set; }
    }

    public class LoanAccountSummaryDto
    {
        public string AccountNumber { get; set; }
        public string LoanProductName { get; set; }
        public decimal DisbursedAmount { get; set; }
        public DateTime DisbursementDate { get; set; }
        public decimal InterestRate { get; set; }
        public int Tenor { get; set; }
        public decimal OutstandingBalance { get; set; }
        public decimal NextInstallmentAmount { get; set; }
        public DateTime NextInstallmentDate { get; set; }
        public string Status { get; set; }
    }

    public class LoanApplicationSummaryDto
    {
        public Guid Id { get; set; }
        public string LoanProductName { get; set; }
        public decimal RequestedAmount { get; set; }
        public int RequestedTenor { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string Status { get; set; }
    }

    public class LoanRepaymentScheduleDto
    {
        public int InstallmentNumber { get; set; }
        public DateTime DueDate { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public string Status { get; set; }
    }

    public class LoanTransactionDto
    {
        public string ReferenceNumber { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal PenaltyAmount { get; set; }
        public string Status { get; set; }
    }
}