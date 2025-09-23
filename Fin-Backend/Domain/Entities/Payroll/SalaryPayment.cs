using System;
using FinTech.Domain.Common;
using FinTech.Domain.Events.Payroll;
using FinTech.Domain.Entities.Common;

namespace FinTech.Domain.Entities.Payroll
{
    public class SalaryPayment : AggregateRoot
    {
        public int EmployeeId { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal PensionAmount { get; set; }
        public decimal OtherDeductions { get; set; }
        public decimal NetAmount { get; set; }
        public string Reference { get; set; }
        public string Status { get; set; }

        private SalaryPayment() 
        {
            Reference = string.Empty;
            Status = string.Empty;
        } // For EF Core

        public SalaryPayment(int employeeId, decimal grossAmount, decimal taxAmount, decimal pensionAmount, decimal otherDeductions, string reference, string payPeriod)
        {
            EmployeeId = employeeId;
            GrossAmount = grossAmount;
            TaxAmount = taxAmount;
            PensionAmount = pensionAmount;
            OtherDeductions = otherDeductions;
            NetAmount = grossAmount - taxAmount - pensionAmount - otherDeductions;
            Reference = reference;
            Status = "Pending";

            AddDomainEvent(new SalaryPaymentProcessedEvent(
                employeeId,
                grossAmount,
                taxAmount,
                pensionAmount,
                otherDeductions,
                payPeriod,
                reference,
                "Salary payment processed"
                ));
        }
    }
}