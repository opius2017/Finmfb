using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Entities.Customers;
using FinTech.Core.Domain.Entities.Deposits;

namespace FinTech.Core.Domain.Entities.ClientPortal
{
    // These entities complement the existing ones in ApplicationDbContext
    
    public class Biller : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public bool RequiresCustomerReference { get; set; }
        public string? ReferenceNumberLabel { get; set; }
        public string? PaymentInstructions { get; set; }
        public bool IsActive { get; set; }
        
        // Navigation properties
        public virtual ICollection<BillPayment> BillPayments { get; set; } = new List<BillPayment>();
        public virtual ICollection<SavedPayee> SavedPayees { get; set; } = new List<SavedPayee>();
    }
    
    public class BillPayment : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public Guid BillerId { get; set; }
        public Guid AccountId { get; set; }
        public string CustomerReferenceNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ProcessedAt { get; set; }
        
        // Navigation properties
        public virtual Customer? Customer { get; set; }
        public virtual Biller? Biller { get; set; }
        public virtual DepositAccount? Account { get; set; }
    }
    
    public class ExternalTransfer : BaseEntity
    {
        public Guid SourceAccountId { get; set; }
        public string DestinationAccountNumber { get; set; } = string.Empty;
        public string DestinationBankName { get; set; } = string.Empty;
        public string DestinationBankCode { get; set; } = string.Empty;
        public string BeneficiaryName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string TransferType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime InitiatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public Guid CustomerId { get; set; }
        
        // Navigation properties
        public virtual DepositAccount? SourceAccount { get; set; }
        public virtual Customer? Customer { get; set; }
    }
    
    public class RecurringPayment : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public Guid SourceAccountId { get; set; }
        public string PaymentType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Frequency { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? LastExecutionDate { get; set; }
        public DateTime? NextExecutionDate { get; set; }
        
        [NotMapped]
        public string PaymentName => PaymentType == "Bill" ? (Biller?.Name ?? Description) : BeneficiaryName;
        
        [NotMapped]
        public string Currency => SourceAccount?.CurrencyCode ?? "NGN";
        
        // For transfers
        public string DestinationAccountNumber { get; set; } = string.Empty;
        public string DestinationBankName { get; set; } = string.Empty;
        public string DestinationBankCode { get; set; } = string.Empty;
        public string BeneficiaryName { get; set; } = string.Empty;
        public string TransferType { get; set; } = string.Empty;
        
        // For bill payments
        public Guid? BillerId { get; set; }
        public string CustomerReferenceNumber { get; set; } = string.Empty;
        
        // Navigation properties
        public virtual Customer? Customer { get; set; }
        public virtual DepositAccount? SourceAccount { get; set; }
        public virtual Biller? Biller { get; set; }
        public virtual ICollection<RecurringPaymentHistory> PaymentHistory { get; set; }

        // Added properties/aliases to resolve build errors
        [NotMapped] public Guid FromAccountId { get => SourceAccountId; set => SourceAccountId = value; }
        public string? Reference { get; set; }
        [NotMapped] public string ToAccountId { get => DestinationAccountNumber; set => DestinationAccountNumber = value; }
        public string? BeneficiaryId { get; set; } // Could map to BillerId if needed, but adding separate prop for now
        [NotMapped] public DateTime? NextPaymentDate { get => NextExecutionDate; set => NextExecutionDate = value; }
        [NotMapped] public DateTime? LastPaymentDate { get => LastExecutionDate; set => LastExecutionDate = value; }
    }
    
    public class RecurringPaymentHistory : BaseEntity
    {
        public Guid RecurringPaymentId { get; set; }
        public DateTime ExecutionDate { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
        public string? FailureReason { get; set; }
        
        // Navigation properties
        public virtual RecurringPayment? RecurringPayment { get; set; }
    }
}
