using System;

namespace FinTech.Core.Application.DTOs.Loans
{
    /// <summary>
    /// Early repayment calculation result
    /// </summary>
    public class EarlyRepaymentCalculationDto
    {
        public decimal OutstandingPrincipal { get; set; }
        public decimal EarlyRepaymentAmount { get; set; }
        public decimal NewOutstandingBalance { get; set; }
        public decimal InterestSaved { get; set; }
        public decimal NewMonthlyEMI { get; set; }
        public bool LoanFullyPaid { get; set; }
        public DateTime RepaymentDate { get; set; }
    }
}
