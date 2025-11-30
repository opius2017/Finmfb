using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Loans
{
    /// <summary>
    /// Comprehensive loan calculation summary
    /// </summary>
    public class LoanCalculationSummary
    {
        public decimal Principal { get; set; }
        public decimal AnnualInterestRate { get; set; }
        public int TenorMonths { get; set; }
        public decimal MonthlyEMI { get; set; }
        public decimal TotalInterest { get; set; }
        public decimal TotalRepayable { get; set; }
        public DateTime FirstPaymentDate { get; set; }
        public DateTime LastPaymentDate { get; set; }
        public List<AmortizationScheduleItem> AmortizationSchedule { get; set; }
    }
}
