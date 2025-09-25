using System;
using System.Collections.Generic;
using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Events.Payroll;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Payroll
{
    public class PayrollPeriod : AggregateRoot
    {
        public string Period { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsPosted { get; set; }
        public DateTime? PostedDate { get; set; }
        public bool IsClosed { get; private set; }
        public DateTime? ClosedDate { get; private set; }
        public string Status { get; set; }
        public decimal TotalGross { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalPension { get; set; }
        public decimal TotalOtherDeductions { get; set; }
        public decimal TotalNet { get; set; }
        public DateTime ProcessingDate { get; set; }

        private readonly List<SalaryPayment> _salaryPayments = new List<SalaryPayment>();
        public IReadOnlyCollection<SalaryPayment> SalaryPayments => _salaryPayments.AsReadOnly();

        private readonly List<PayrollTransaction> _transactions = new List<PayrollTransaction>();
        public IReadOnlyCollection<PayrollTransaction> Transactions => _transactions.AsReadOnly();


        private PayrollPeriod() 
        { 
            Period = string.Empty;
            Status = string.Empty;
        } // For EF Core

        public PayrollPeriod(string period, DateTime startDate, DateTime endDate)
        {
            Period = period;
            StartDate = startDate;
            EndDate = endDate;
            Status = "Open";
            ProcessingDate = DateTime.Now;
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
            _salaryPayments.Add(payment);
            TotalGross += payment.GrossAmount;
            TotalTax += payment.TaxAmount;
            TotalPension += payment.PensionAmount;
            TotalOtherDeductions += payment.OtherDeductions;
            TotalNet += payment.NetAmount;
        }

        public void AddTransaction(PayrollTransaction transaction)
        {
            _transactions.Add(transaction);
        }

        public void Post()
        {
            if (Status != "COMPLETED")
                throw new InvalidOperationException("Payroll must be COMPLETED to post");

            if (IsPosted)
                throw new InvalidOperationException("Payroll is already posted");

            IsPosted = true;
            PostedDate = DateTime.Now;
        }

        public void Close()
        {
            if (Status != "COMPLETED")
                throw new InvalidOperationException("Payroll must be COMPLETED to close");

            if (IsClosed)
                throw new InvalidOperationException("Payroll is already closed");

            IsClosed = true;
            ClosedDate = DateTime.Now;
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
