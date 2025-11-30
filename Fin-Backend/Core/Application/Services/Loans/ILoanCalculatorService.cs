using System;
using System.Collections.Generic;
using FinTech.Core.Application.DTOs.Loans;

namespace FinTech.Core.Application.Services.Loans
{
    /// <summary>
    /// Service for loan calculations including EMI, amortization, and penalties
    /// </summary>
    public interface ILoanCalculatorService
    {
        /// <summary>
        /// Calculate monthly EMI using reducing balance method
        /// </summary>
        /// <param name="principal">Loan principal amount</param>
        /// <param name="annualInterestRate">Annual interest rate (e.g., 12 for 12%)</param>
        /// <param name="tenureMonths">Loan tenure in months</param>
        /// <returns>Monthly EMI amount</returns>
        decimal CalculateEMI(decimal principal, decimal annualInterestRate, int tenureMonths);

        /// <summary>
        /// Generate complete amortization schedule
        /// </summary>
        /// <param name="request">Amortization schedule request</param>
        /// <returns>Complete amortization schedule</returns>
        AmortizationScheduleDto GenerateAmortizationSchedule(AmortizationScheduleRequest request);

        /// <summary>
        /// Calculate total interest payable over loan tenure
        /// </summary>
        /// <param name="principal">Loan principal amount</param>
        /// <param name="annualInterestRate">Annual interest rate</param>
        /// <param name="tenureMonths">Loan tenure in months</param>
        /// <returns>Total interest amount</returns>
        decimal CalculateTotalInterest(decimal principal, decimal annualInterestRate, int tenureMonths);

        /// <summary>
        /// Calculate penalty for overdue payment
        /// </summary>
        /// <param name="overdueAmount">Amount that is overdue</param>
        /// <param name="daysOverdue">Number of days overdue</param>
        /// <param name="penaltyRate">Daily penalty rate (e.g., 0.1 for 0.1% per day)</param>
        /// <returns>Penalty amount</returns>
        decimal CalculatePenalty(decimal overdueAmount, int daysOverdue, decimal penaltyRate);

        /// <summary>
        /// Calculate early repayment details
        /// </summary>
        /// <param name="request">Early repayment request</param>
        /// <returns>Early repayment calculation result</returns>
        EarlyRepaymentCalculationDto CalculateEarlyRepayment(EarlyRepaymentRequest request);

        /// <summary>
        /// Calculate outstanding balance at a specific point in time
        /// </summary>
        /// <param name="principal">Original principal</param>
        /// <param name="annualInterestRate">Annual interest rate</param>
        /// <param name="tenureMonths">Total tenure in months</param>
        /// <param name="paymentsMade">Number of payments already made</param>
        /// <returns>Outstanding principal balance</returns>
        decimal CalculateOutstandingBalance(decimal principal, decimal annualInterestRate, int tenureMonths, int paymentsMade);

        /// <summary>
        /// Calculate reducing balance for a specific installment
        /// </summary>
        /// <param name="outstandingBalance">Current outstanding balance</param>
        /// <param name="monthlyInterestRate">Monthly interest rate (annual rate / 12 / 100)</param>
        /// <param name="emiAmount">Monthly EMI amount</param>
        /// <returns>Interest and principal breakdown</returns>
        InstallmentBreakdown CalculateInstallmentBreakdown(decimal outstandingBalance, decimal monthlyInterestRate, decimal emiAmount);

        /// <summary>
        /// Validate loan calculation parameters
        /// </summary>
        /// <param name="principal">Loan principal</param>
        /// <param name="annualInterestRate">Annual interest rate</param>
        /// <param name="tenureMonths">Tenure in months</param>
        /// <returns>Validation result</returns>
        ValidationResult ValidateLoanParameters(decimal principal, decimal annualInterestRate, int tenureMonths);
    }

    #region Request/Response DTOs

    public class AmortizationScheduleRequest
    {
        public decimal Principal { get; set; }
        public decimal AnnualInterestRate { get; set; }
        public int TenureMonths { get; set; }
        public DateTime StartDate { get; set; }
        public string LoanNumber { get; set; } = string.Empty;
    }

    public class EarlyRepaymentRequest
    {
        public decimal OutstandingPrincipal { get; set; }
        public decimal AnnualInterestRate { get; set; }
        public int RemainingTenureMonths { get; set; }
        public decimal EarlyRepaymentAmount { get; set; }
        public DateTime RepaymentDate { get; set; }
    }

    public class InstallmentBreakdown
    {
        public decimal InterestAmount { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal TotalPayment { get; set; }
        public decimal RemainingBalance { get; set; }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    #endregion
}
