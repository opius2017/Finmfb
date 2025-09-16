using System;
using FinTech.Domain.Common;
using FinTech.Domain.Events.Payroll;

namespace FinTech.Domain.Entities.Payroll
{
    public class SalaryPayment : BaseEntity
    {
        public int PayrollPeriodId { get; private set; }
        public int EmployeeId { get; private set; }
        public decimal GrossAmount { get; private set; }
        public decimal TaxAmount { get; private set; }
        public decimal PensionAmount { get; private set; }
        public decimal OtherDeductions { get; private set; }
        public decimal NetAmount { get; private set; }
        public string Reference { get; private set; }
        public string Status { get; private set; }
        public DateTime PaymentDate { get; private set; }
        
        private SalaryPayment() { } // For EF Core

        public SalaryPayment(
            int payrollPeriodId,
            int employeeId,
            decimal grossAmount,
            decimal taxAmount,
            decimal pensionAmount,
            decimal otherDeductions,
            string reference)
        {
            PayrollPeriodId = payrollPeriodId;
            EmployeeId = employeeId;
            GrossAmount = grossAmount;
            TaxAmount = taxAmount;
            PensionAmount = pensionAmount;
            OtherDeductions = otherDeductions;
            NetAmount = grossAmount - taxAmount - pensionAmount - otherDeductions;
            Reference = reference;
            Status = "PENDING";
        }

        public void Process(string description)
        {
            if (Status != "PENDING")
                throw new InvalidOperationException("Salary payment must be in PENDING status to process");

            Status = "PROCESSED";
            PaymentDate = DateTime.UtcNow;

            // Raise domain event
            AddDomainEvent(new SalaryPaymentProcessedEvent(
                EmployeeId,
                GrossAmount,
                TaxAmount,
                PensionAmount,
                OtherDeductions,
                "MONTHLY", // Assuming monthly pay period for simplicity
                Reference,
                description));
        }

        public void Cancel()
        {
            if (Status != "PENDING")
                throw new InvalidOperationException("Only pending salary payments can be cancelled");

            Status = "CANCELLED";
        }
    }
}