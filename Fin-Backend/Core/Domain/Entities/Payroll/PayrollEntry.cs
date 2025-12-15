using System;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Payroll
{
    public class PayrollEntry : AuditableEntity
    {
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string PayrollPeriod { get; set; }
        public DateTime PayrollDate { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal Allowances { get; set; }
        public decimal Deductions { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal NetSalary { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        
        // properties required by integration service
        public string PayrollRunId { get; set; }
        public string PayPeriod { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal PensionAmount { get; set; }
        public decimal OtherDeductions { get; set; }
        
        public PayrollEntry()
        {
            EmployeeId = string.Empty;
            EmployeeName = string.Empty;
            PayrollPeriod = string.Empty;
            PaymentMethod = string.Empty;
            PayrollDate = DateTime.UtcNow;
            Status = "PENDING";
        }
    }
}
