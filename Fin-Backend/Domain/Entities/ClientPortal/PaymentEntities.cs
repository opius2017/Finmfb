using System;
using System.Collections.Generic;
using FinTech.Core.Domain.Entities.Common;
using FinTech.Core.Domain.Entities.Customers;
using FinTech.Core.Domain.Entities.Deposits;

namespace FinTech.Core.Domain.Entities.ClientPortal
{
    // These entities complement the existing ones in ApplicationDbContext
    
    public class Biller : BaseEntity
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public string LogoUrl { get; set; }
        public bool RequiresCustomerReference { get; set; }
        public string ReferenceNumberLabel { get; set; }
        public string PaymentInstructions { get; set; }
        public bool IsActive { get; set; }
        
        // Navigation properties
        public virtual ICollection<BillPayment> BillPayments { get; set; }
        public virtual ICollection<SavedPayee> SavedPayees { get; set; }
    }
    
    public class BillPayment : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public Guid BillerId { get; set; }
        public Guid AccountId { get; set; }
        public string CustomerReferenceNumber { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string ReferenceNumber { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime? ProcessedAt { get; set; }
        
        // Navigation properties
        public virtual Customer Customer { get; set; }
        public virtual Biller Biller { get; set; }
        public virtual DepositAccount Account { get; set; }
    }
    
    public class ExternalTransfer : BaseEntity
    {
        public Guid SourceAccountId { get; set; }
        public string DestinationAccountNumber { get; set; }
        public string DestinationBankName { get; set; }
        public string DestinationBankCode { get; set; }
        public string BeneficiaryName { get; set; }
        public decimal Amount { get; set; }
        public string ReferenceNumber { get; set; }
        public string Status { get; set; }
        public string TransferType { get; set; }
        public string Description { get; set; }
        public DateTime InitiatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public Guid CustomerId { get; set; }
        
        // Navigation properties
        public virtual DepositAccount SourceAccount { get; set; }
        public virtual Customer Customer { get; set; }
    }
    
    public class RecurringPayment : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public Guid SourceAccountId { get; set; }
        public string PaymentType { get; set; } // Transfer, Bill
        public decimal Amount { get; set; }
        public string Frequency { get; set; } // Daily, Weekly, Monthly, Quarterly, Annually
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Description { get; set; }
        public string Status { get; set; } // Active, Paused, Cancelled, Completed
        public DateTime? LastExecutionDate { get; set; }
        public DateTime? NextExecutionDate { get; set; }
        
        // For transfers
        public string DestinationAccountNumber { get; set; }
        public string DestinationBankName { get; set; }
        public string DestinationBankCode { get; set; }
        public string BeneficiaryName { get; set; }
        public string TransferType { get; set; }
        
        // For bill payments
        public Guid? BillerId { get; set; }
        public string CustomerReferenceNumber { get; set; }
        
        // Navigation properties
        public virtual Customer Customer { get; set; }
        public virtual DepositAccount SourceAccount { get; set; }
        public virtual Biller Biller { get; set; }
        public virtual ICollection<RecurringPaymentHistory> PaymentHistory { get; set; }
    }
    
    public class RecurringPaymentHistory : BaseEntity
    {
        public Guid RecurringPaymentId { get; set; }
        public DateTime ExecutionDate { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string ReferenceNumber { get; set; }
        public string FailureReason { get; set; }
        
        // Navigation properties
        public virtual RecurringPayment RecurringPayment { get; set; }
    }
}
