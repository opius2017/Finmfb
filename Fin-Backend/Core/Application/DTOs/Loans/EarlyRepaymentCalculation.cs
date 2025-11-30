using System;

namespace FinTech.Core.Application.DTOs.Loans
{
    /// <summary>
    /// Result of early repayment calculation
    /// </summary>
    public class EarlyRepaymentCalculation
    {
        public decimal OutstandingPrincipal { get; set; }
        public decimal AccruedInterest { get; set; }
        public decimal TotalEarlyRepaymentAmount { get; set; }
        public int DaysElapsed { get; set; }
        public DateTime CalculationDate { get; set; }
    }
}
