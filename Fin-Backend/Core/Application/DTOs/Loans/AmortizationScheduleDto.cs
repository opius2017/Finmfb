using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Loans
{
    /// <summary>
    /// Complete amortization schedule for a loan
    /// </summary>
    public class AmortizationScheduleDto
    {
        public string LoanNumber { get; set; } = string.Empty;
        public decimal Principal { get; set; }
        public decimal AnnualInterestRate { get; set; }
        public int TenureMonths { get; set; }
        public decimal MonthlyEMI { get; set; }
        public decimal TotalInterest { get; set; }
        public decimal TotalPayment { get; set; }
        public DateTime StartDate { get; set; }
        public List<AmortizationInstallmentDto> Installments { get; set; } = new List<AmortizationInstallmentDto>();
    }

    /// <summary>
    /// Individual installment in amortization schedule
    /// </summary>
    public class AmortizationInstallmentDto
    {
        public int InstallmentNumber { get; set; }
        public DateTime DueDate { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal EMIAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal ClosingBalance { get; set; }
        public decimal CumulativeInterest { get; set; }
        public decimal CumulativePrincipal { get; set; }
    }
}
