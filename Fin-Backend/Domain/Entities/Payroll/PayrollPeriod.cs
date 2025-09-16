using System;
using System.Collections.Generic;
using FinTech.Domain.Common;
using FinTech.Domain.Events.Payroll;

namespace FinTech.Domain.Entities.Payroll
{
    public class PayrollPeriod : BaseEntity
    {
        public string Period { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public DateTime ProcessingDate { get; private set; }
        public string Status { get; private set; }
        public decimal TotalGross { get; private set; }
        public decimal TotalTax { get; private set; }
        public decimal TotalPension { get; private set; }
        public decimal TotalOtherDeductions { get; private set; }
        public decimal TotalNet { get; private set; }
        public virtual ICollection<SalaryPayment> SalaryPayments { get; private set; } = new List<SalaryPayment>();
        public virtual ICollection<PayrollTransaction> Transactions { get; private set; } = new List<PayrollTransaction>();

        private PayrollPeriod() { } // For EF Core

        public PayrollPeriod(string period, DateTime startDate, DateTime endDate, DateTime processingDate)
        {
            Period = period;
            StartDate = startDate;
            EndDate = endDate;
            ProcessingDate = processingDate;
            Status = "PENDING";
            TotalGross = 0;
            TotalTax = 0;
            TotalPension = 0;
            TotalOtherDeductions = 0;
            TotalNet = 0;
        }

        public void ProcessPayroll()
        {
            if (Status != "PENDING")
                throw new InvalidOperationException("Payroll must be in PENDING status to process");

            Status = "PROCESSING";
        }

        public void CompleteProcessing()
        {
            if (Status != "PROCESSING")
                throw new InvalidOperationException("Payroll must be in PROCESSING status to complete");

            Status = "COMPLETED";
        }

        public void AddSalaryPayment(SalaryPayment payment)
        {
            if (Status != "PENDING" && Status != "PROCESSING")
                throw new InvalidOperationException("Cannot add salary payments to a completed payroll");

            SalaryPayments.Add(payment);

            // Update totals
            TotalGross += payment.GrossAmount;
            TotalTax += payment.TaxAmount;
            TotalPension += payment.PensionAmount;
            TotalOtherDeductions += payment.OtherDeductions;
            TotalNet += payment.NetAmount;
        }

        public void RemitTaxes(decimal amount, string taxType, string reference, string description)
        {
            if (Status != "COMPLETED")
                throw new InvalidOperationException("Payroll must be COMPLETED to remit taxes");

            if (amount <= 0)
                throw new ArgumentException("Tax amount must be positive", nameof(amount));

            var transaction = new PayrollTransaction(
                this.Id,
                "TAX_REMITTANCE",
                amount,
                taxType,
                reference,
                description);

            Transactions.Add(transaction);

            // Raise domain event
            AddDomainEvent(new PayrollTaxRemittedEvent(
                amount,
                taxType,
                this.Period,
                reference,
                description));
        }

        public void RemitPension(decimal amount, string pensionProvider, string reference, string description)
        {
            if (Status != "COMPLETED")
                throw new InvalidOperationException("Payroll must be COMPLETED to remit pension");

            if (amount <= 0)
                throw new ArgumentException("Pension amount must be positive", nameof(amount));

            var transaction = new PayrollTransaction(
                this.Id,
                "PENSION_REMITTANCE",
                amount,
                pensionProvider,
                reference,
                description);

            Transactions.Add(transaction);

            // Raise domain event
            AddDomainEvent(new PensionRemittedEvent(
                amount,
                pensionProvider,
                this.Period,
                reference,
                description));
        }

        public void AccrueExpense(decimal amount, string expenseType, string reference, string description)
        {
            if (Status != "PENDING" && Status != "PROCESSING")
                throw new InvalidOperationException("Cannot accrue expenses for a completed payroll");

            if (amount <= 0)
                throw new ArgumentException("Expense amount must be positive", nameof(amount));

            var transaction = new PayrollTransaction(
                this.Id,
                "EXPENSE_ACCRUAL",
                amount,
                expenseType,
                reference,
                description);

            Transactions.Add(transaction);

            // Raise domain event
            AddDomainEvent(new PayrollExpenseAccruedEvent(
                amount,
                expenseType,
                this.Period,
                reference,
                description));
        }
    }
}