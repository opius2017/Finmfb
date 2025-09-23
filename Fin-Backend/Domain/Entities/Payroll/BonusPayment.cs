using System;
using FinTech.Domain.Common;
using FinTech.Domain.Events.Payroll;
using FinTech.Domain.Entities.Common;

namespace FinTech.Domain.Entities.Payroll
{
    public class BonusPayment : BaseEntity
    {
        public int EmployeeId { get; private set; }
        public decimal Amount { get; private set; }
        public string BonusType { get; private set; }
        public string Reference { get; private set; }
        public string Status { get; private set; }
        public DateTime PaymentDate { get; private set; }
        
        private BonusPayment() { } // For EF Core

        public BonusPayment(
            int employeeId,
            decimal amount,
            string bonusType,
            string reference)
        {
            EmployeeId = employeeId;
            Amount = amount;
            BonusType = bonusType;
            Reference = reference;
            Status = "PENDING";
        }

        public void Process(string description)
        {
            if (Status != "PENDING")
                throw new InvalidOperationException("Bonus payment must be in PENDING status to process");

            Status = "PROCESSED";
            PaymentDate = DateTime.UtcNow;

            // Raise domain event
            AddDomainEvent(new BonusPaymentProcessedEvent(
                EmployeeId,
                Amount,
                Reference,
                description));
        }

        public void Cancel()
        {
            if (Status != "PENDING")
                throw new InvalidOperationException("Only pending bonus payments can be cancelled");

            Status = "CANCELLED";
        }
    }
}