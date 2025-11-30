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
        
        public PayrollEntry()
        {
            PayrollDate = DateTime.UtcNow;
            Status = "PENDING";
        }
    }
}
