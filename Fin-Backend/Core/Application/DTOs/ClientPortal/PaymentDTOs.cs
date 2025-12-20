using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    // Fund Transfer DTOs
    // Fund Transfer DTOs
    public class FundTransferDto
    {
        [Required]
        public string SourceAccountNumber { get; set; } = string.Empty;
        
        [Required]
        public string DestinationAccountNumber { get; set; } = string.Empty;
        
        [Required]
        [Range(0.01, 10000000)]
        public decimal Amount { get; set; }
        
        [Required]
        public string TransferType { get; set; } = string.Empty; // Internal, RTGS, NEFT, IMPS, etc.
        
        public string? DestinationBankName { get; set; }
        
        public string? DestinationBankCode { get; set; }
        
        public string? BeneficiaryName { get; set; }
        
        public string? Description { get; set; }
        
        public bool SaveAsTemplate { get; set; }
        
        public string? TemplateName { get; set; }
        
        // For tracking activity, populated by the controller
        public string? IpAddress { get; set; }
        
        public string? UserAgent { get; set; }

        // FinTech Best Practice: Transaction reference for tracking
        public string? Reference { get; set; }
    }

    public class SaveTransferTemplateDto
    {
        [Required]
        public string TemplateName { get; set; } = string.Empty;
        
        [Required]
        public string SourceAccountNumber { get; set; } = string.Empty;
        
        [Required]
        public string DestinationAccountNumber { get; set; } = string.Empty;
        
        [Required]
        public string TransferType { get; set; } = string.Empty;
        
        public string? DestinationBankName { get; set; }
        
        public string? DestinationBankCode { get; set; }
        
        public string? BeneficiaryName { get; set; }
        
        [Range(0.01, 10000000)]
        public decimal Amount { get; set; }
        
        public string? Description { get; set; }
    }

    // Bill Payment DTOs
    public class BillPaymentDto
    {
        [Required]
        public string SourceAccountNumber { get; set; } = string.Empty;
        
        [Required]
        public string BillerId { get; set; } = string.Empty;
        
        [Required]
        public string CustomerReferenceNumber { get; set; } = string.Empty;
        
        [Required]
        [Range(0.01, 10000000)]
        public decimal Amount { get; set; }
        
        public string? Description { get; set; }
        
        public bool SavePayee { get; set; }
        
        public string? PayeeName { get; set; }
        
        // For tracking activity, populated by the controller
        public string? IpAddress { get; set; }
        
        public string? UserAgent { get; set; }
    }

    public class SavePayeeDto
    {
        [Required]
        public string BillerId { get; set; } = string.Empty;
        
        [Required]
        public string CustomerReferenceNumber { get; set; } = string.Empty;
        
        public string? PayeeName { get; set; }
    }

    // Recurring Payment DTOs
    public class RecurringPaymentDto
    {
        [Required]
        public string SourceAccountNumber { get; set; } = string.Empty;
        
        [Required]
        [Range(0.01, 10000000)]
        public decimal Amount { get; set; }
        
        [Required]
        public string PaymentType { get; set; } = string.Empty; // Transfer, Bill
        
        [Required]
        public string Frequency { get; set; } = string.Empty; // Daily, Weekly, Monthly, Quarterly, Annually
        
        [Required]
        public DateTime StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        public string? Description { get; set; }
        
        // For transfers
        public string? DestinationAccountNumber { get; set; }
        public string? DestinationBankName { get; set; }
        public string? DestinationBankCode { get; set; }
        public string? BeneficiaryName { get; set; }
        public string? TransferType { get; set; }
        
        // For bill payments
        public string? BillerId { get; set; }
        public string? CustomerReferenceNumber { get; set; }
    }

    public class RecurringPaymentUpdateDto
    {
        [Range(0.01, 10000000)]
        public decimal? Amount { get; set; }
        
        public string? Frequency { get; set; }
        
        public DateTime? StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        public string? Description { get; set; }
        
        public string? Status { get; set; } // Active, Paused
    }

    // Payment History DTOs
    public class PaymentHistoryRequestDto
    {
        public DateTime? FromDate { get; set; }
        
        public DateTime? ToDate { get; set; }
        
        public string? TransactionType { get; set; } // Fund Transfer, Bill Payment, Recurring Payment
        
        public string? Status { get; set; } // Pending, Completed, Failed
        
        public decimal? MinAmount { get; set; }
        
        public decimal? MaxAmount { get; set; }
        
        public int Page { get; set; } = 1;
        
        public int PageSize { get; set; } = 20;
    }

    // Response DTOs
    public class TransferResult
    {
        public bool Success { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public string SourceAccountNumber { get; set; } = string.Empty;
        public string DestinationAccountNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class PaymentResult
    {
        public bool Success { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public string BillerName { get; set; } = string.Empty;
        public string CustomerReferenceNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class BillerInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public bool RequiresCustomerReference { get; set; }
        public string ReferenceNumberLabel { get; set; } = string.Empty;
    }
}
