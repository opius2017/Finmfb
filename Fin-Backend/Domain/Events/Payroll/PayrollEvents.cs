using System;
using FinTech.Domain.Common;
using FinTech.Domain.Entities.Common;
using FinTech.Domain.Entities.Payroll;

namespace FinTech.Domain.Events.Payroll
{
    public class SalaryPaymentProcessedEvent : DomainEvent
    {
        public int EmployeeId { get; }
        public decimal GrossAmount { get; }
        public decimal TaxAmount { get; }
        public decimal PensionAmount { get; }
        public decimal OtherDeductions { get; }
        public string PayPeriod { get; }
        public string Reference { get; }
        public string Description { get; }

        public SalaryPaymentProcessedEvent(int employeeId, decimal grossAmount, decimal taxAmount, 
            decimal pensionAmount, decimal otherDeductions, string payPeriod, string reference, string description)
        {
            EmployeeId = employeeId;
            GrossAmount = grossAmount;
            TaxAmount = taxAmount;
            PensionAmount = pensionAmount;
            OtherDeductions = otherDeductions;
            PayPeriod = payPeriod;
            Reference = reference;
            Description = description;
        }
    }

    public class PayrollTaxRemittedEvent : DomainEvent
    {
        public decimal Amount { get; }
        public string TaxType { get; }
        public string TaxPeriod { get; }
        public string Reference { get; }
        public string Description { get; }

        public PayrollTaxRemittedEvent(decimal amount, string taxType, string taxPeriod, string reference, string description)
        {
            Amount = amount;
            TaxType = taxType;
            TaxPeriod = taxPeriod;
            Reference = reference;
            Description = description;
        }
    }

    public class PensionRemittedEvent : DomainEvent
    {
        public decimal Amount { get; }
        public string PensionProvider { get; }
        public string PensionPeriod { get; }
        public string Reference { get; }
        public string Description { get; }

        public PensionRemittedEvent(decimal amount, string pensionProvider, string pensionPeriod, string reference, string description)
        {
            Amount = amount;
            PensionProvider = pensionProvider;
            PensionPeriod = pensionPeriod;
            Reference = reference;
            Description = description;
        }
    }

    public class BonusPaymentProcessedEvent : DomainEvent
    {
        public int EmployeeId { get; }
        public decimal Amount { get; }
        public string Reference { get; }
        public string Description { get; }

        public BonusPaymentProcessedEvent(int employeeId, decimal amount, string reference, string description)
        {
            EmployeeId = employeeId;
            Amount = amount;
            Reference = reference;
            Description = description;
        }
    }

    public class PayrollExpenseAccruedEvent : DomainEvent
    {
        public decimal Amount { get; }
        public string ExpenseType { get; }
        public string Period { get; }
        public string Reference { get; }
        public string Description { get; }

        public PayrollExpenseAccruedEvent(decimal amount, string expenseType, string period, string reference, string description)
        {
            Amount = amount;
            ExpenseType = expenseType;
            Period = period;
            Reference = reference;
            Description = description;
        }
    }
}