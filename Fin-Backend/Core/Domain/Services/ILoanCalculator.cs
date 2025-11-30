using System;
using System.Collections.Generic;

namespace FinTech.Core.Domain.Services
{
    /// <summary>
    /// Service for loan calculations using reducing balance method
    /// </summary>
    public interface ILoanCalculator
    {
        /// <summary>
        /// Calculate monthly EMI (Equated Monthly Installment) using reducing balance method
        /// </summary>
        /// <param name="principal">Loan principal amount</param>
        /// <param name="annualInterestRate">Annual interest rate (e.g., 12 for 12%)</param>
        /// <param name="tenorMonths">Loan tenor in months</param>
        /// <returns>Monthly EMI amount</returns>
        decimal CalculateEMI(decimal principal, decimal annualInterestRate, int tenorMonths);
        
        /// <summary>
        /// Calculate total interest payable over the loan term
        /// </summary>
        /// <param name="emi">Monthly EMI amount</param>
        /// <param name="tenorMonths">Loan tenor in months</param>
        /// <param name="principal">Loan principal amount</param>
        /// <returns>Total interest amount</returns>
        decimal CalculateTotalInterest(decimal emi, int tenorMonths, decimal principal);
        
        /// <summary>
        /// Generate complete amortization schedule
        /// </summary>
        /// <param name="input">Loan calculation input parameters</param>
        /// <returns>List of amortization entries for each installment</returns>
        List<AmortizationEntry> GenerateAmortizationSchedule(LoanCalculationInput input);
        
        /// <summary>
        /// Calculate penalty for late payment
        /// </summary>
        /// <param name="overdueAmount">Amount overdue</param>
        /// <param name="daysOverdue">Number of days overdue</param>
        /// <param name="penaltyRate">Daily penalty rate (e.g., 0.05 for 0.05% per day)</param>
        /// <returns>Penalty amount</returns>
        decimal CalculatePenalty(decimal overdueAmount, int daysOverdue, decimal penaltyRate);
        
        /// <summary>
        /// Calculate early repayment amount (outstanding principal + accrued interest)
        /// </summary>
        /// <param name="outstandingPrincipal">Current outstanding principal</param>
        /// <param name="lastPaymentDate">Date of last payment</param>
        /// <param name="annualInterestRate">Annual interest rate</param>
        /// <returns>Total early repayment amount</returns>
        decimal CalculateEarlyRepaymentAmount(decimal outstandingPrincipal, DateTime lastPaymentDate, decimal annualInterestRate);
        
        /// <summary>
        /// Allocate payment to interest and principal
        /// </summary>
        /// <param name="paymentAmount">Total payment amount</param>
        /// <param name="outstandingPrincipal">Current outstanding principal</param>
        /// <param name="accruedInterest">Current accrued interest</param>
        /// <returns>Payment allocation breakdown</returns>
        PaymentAllocation AllocatePayment(decimal paymentAmount, decimal outstandingPrincipal, decimal accruedInterest);
    }
    
    /// <summary>
    /// Input parameters for loan calculations
    /// </summary>
    public class LoanCalculationInput
    {
        public decimal PrincipalAmount { get; set; }
        public decimal AnnualInterestRate { get; set; }
        public int TenorMonths { get; set; }
        public DateTime DisbursementDate { get; set; }
        public DateTime FirstPaymentDate { get; set; }
    }
    
    /// <summary>
    /// Represents a single entry in the amortization schedule
    /// </summary>
    public class AmortizationEntry
    {
        public int InstallmentNumber { get; set; }
        public DateTime DueDate { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal PrincipalDue { get; set; }
        public decimal InterestDue { get; set; }
        public decimal TotalDue { get; set; }
        public decimal ClosingBalance { get; set; }
    }
    
    /// <summary>
    /// Payment allocation breakdown
    /// </summary>
    public class PaymentAllocation
    {
        public decimal TotalPayment { get; set; }
        public decimal InterestPayment { get; set; }
        public decimal PrincipalPayment { get; set; }
        public decimal RemainingBalance { get; set; }
    }
}
