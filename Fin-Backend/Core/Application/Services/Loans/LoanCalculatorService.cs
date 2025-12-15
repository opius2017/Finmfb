using System;
using System.Collections.Generic;
using System.Linq;
using FinTech.Core.Application.DTOs.Loans;
using Microsoft.Extensions.Logging;

using FinTech.Core.Application.Interfaces.Loans;

namespace FinTech.Core.Application.Services.Loans
{
    /// <summary>
    /// Implementation of loan calculation service using reducing balance method
    /// </summary>
    public class LoanCalculatorService : ILoanCalculatorService
    {
        private readonly ILogger<LoanCalculatorService> _logger;

        public LoanCalculatorService(ILogger<LoanCalculatorService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Calculate monthly EMI using reducing balance method
        /// Formula: EMI = P × r × (1 + r)^n / ((1 + r)^n - 1)
        /// Where: P = Principal, r = Monthly interest rate, n = Number of months
        /// </summary>
        public decimal CalculateEMI(decimal principal, decimal annualInterestRate, int tenureMonths)
        {
            try
            {
                // Validate inputs
                var validation = ValidateLoanParameters(principal, annualInterestRate, tenureMonths);
                if (!validation.IsValid)
                {
                    throw new ArgumentException($"Invalid loan parameters: {string.Join(", ", validation.Errors)}");
                }

                // Convert annual rate to monthly rate (as decimal)
                decimal monthlyRate = (annualInterestRate / 12) / 100;

                // Handle zero interest rate case
                if (monthlyRate == 0)
                {
                    return principal / tenureMonths;
                }

                // Calculate EMI using reducing balance formula
                decimal numerator = principal * monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), tenureMonths);
                decimal denominator = (decimal)Math.Pow((double)(1 + monthlyRate), tenureMonths) - 1;

                decimal emi = numerator / denominator;

                _logger.LogInformation(
                    "Calculated EMI: ₦{EMI:N2} for Principal: ₦{Principal:N2}, Rate: {Rate}%, Tenure: {Tenure} months",
                    emi, principal, annualInterestRate, tenureMonths);

                return Math.Round(emi, 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating EMI");
                throw;
            }
        }

        /// <summary>
        /// Generate complete amortization schedule
        /// </summary>
        public AmortizationScheduleDto GenerateAmortizationSchedule(AmortizationScheduleRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "Generating amortization schedule for loan {LoanNumber}: Principal ₦{Principal:N2}, Rate {Rate}%, Tenure {Tenure} months",
                    request.LoanNumber, request.Principal, request.AnnualInterestRate, request.TenureMonths);

                // Calculate EMI
                decimal emi = CalculateEMI(request.Principal, request.AnnualInterestRate, request.TenureMonths);
                decimal monthlyRate = (request.AnnualInterestRate / 12) / 100;

                var schedule = new AmortizationScheduleDto
                {
                    LoanNumber = request.LoanNumber,
                    Principal = request.Principal,
                    AnnualInterestRate = request.AnnualInterestRate,
                    TenureMonths = request.TenureMonths,
                    MonthlyEMI = emi,
                    StartDate = request.StartDate,
                    Installments = new List<AmortizationInstallmentDto>()
                };

                decimal outstandingBalance = request.Principal;
                decimal totalInterest = 0;
                decimal totalPrincipal = 0;

                // Generate installment schedule
                for (int month = 1; month <= request.TenureMonths; month++)
                {
                    // Calculate interest for this month
                    decimal interestAmount = Math.Round(outstandingBalance * monthlyRate, 2);
                    
                    // Calculate principal for this month
                    decimal principalAmount = Math.Round(emi - interestAmount, 2);

                    // Adjust last installment to account for rounding differences
                    if (month == request.TenureMonths)
                    {
                        principalAmount = outstandingBalance;
                        emi = principalAmount + interestAmount;
                    }

                    // Update outstanding balance
                    decimal newBalance = outstandingBalance - principalAmount;

                    var installment = new AmortizationInstallmentDto
                    {
                        InstallmentNumber = month,
                        DueDate = request.StartDate.AddMonths(month),
                        OpeningBalance = outstandingBalance,
                        EMIAmount = emi,
                        InterestAmount = interestAmount,
                        PrincipalAmount = principalAmount,
                        ClosingBalance = Math.Max(0, newBalance),
                        CumulativeInterest = totalInterest + interestAmount,
                        CumulativePrincipal = totalPrincipal + principalAmount
                    };

                    schedule.Installments.Add(installment);

                    // Update totals
                    totalInterest += interestAmount;
                    totalPrincipal += principalAmount;
                    outstandingBalance = Math.Max(0, newBalance);
                }

                schedule.TotalInterest = Math.Round(totalInterest, 2);
                schedule.TotalPayment = Math.Round(totalPrincipal + totalInterest, 2);

                _logger.LogInformation(
                    "Generated amortization schedule: Total Interest ₦{Interest:N2}, Total Payment ₦{Total:N2}",
                    schedule.TotalInterest, schedule.TotalPayment);

                return schedule;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating amortization schedule");
                throw;
            }
        }

        /// <summary>
        /// Calculate total interest payable
        /// </summary>
        public decimal CalculateTotalInterest(decimal principal, decimal annualInterestRate, int tenureMonths)
        {
            try
            {
                decimal emi = CalculateEMI(principal, annualInterestRate, tenureMonths);
                decimal totalPayment = emi * tenureMonths;
                decimal totalInterest = totalPayment - principal;

                return Math.Round(totalInterest, 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total interest");
                throw;
            }
        }

        /// <summary>
        /// Calculate penalty for overdue payment
        /// </summary>
        public decimal CalculatePenalty(decimal overdueAmount, int daysOverdue, decimal penaltyRate)
        {
            try
            {
                if (overdueAmount <= 0 || daysOverdue <= 0 || penaltyRate < 0)
                {
                    return 0;
                }

                // Penalty = Overdue Amount × Daily Rate × Days Overdue
                decimal penalty = overdueAmount * (penaltyRate / 100) * daysOverdue;

                _logger.LogInformation(
                    "Calculated penalty: ₦{Penalty:N2} for overdue amount ₦{Amount:N2}, {Days} days at {Rate}% per day",
                    penalty, overdueAmount, daysOverdue, penaltyRate);

                return Math.Round(penalty, 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating penalty");
                throw;
            }
        }

        /// <summary>
        /// Calculate early repayment details
        /// </summary>
        public EarlyRepaymentCalculationDto CalculateEarlyRepayment(EarlyRepaymentRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "Calculating early repayment: Outstanding ₦{Outstanding:N2}, Repayment ₦{Repayment:N2}",
                    request.OutstandingPrincipal, request.EarlyRepaymentAmount);

                var result = new EarlyRepaymentCalculationDto
                {
                    OutstandingPrincipal = request.OutstandingPrincipal,
                    EarlyRepaymentAmount = request.EarlyRepaymentAmount,
                    RepaymentDate = request.RepaymentDate
                };

                // Calculate interest saved
                decimal totalInterestWithoutEarlyPayment = CalculateTotalInterest(
                    request.OutstandingPrincipal,
                    request.AnnualInterestRate,
                    request.RemainingTenureMonths);

                // Calculate new outstanding after early payment
                decimal newOutstanding = Math.Max(0, request.OutstandingPrincipal - request.EarlyRepaymentAmount);

                // Calculate interest on new outstanding
                decimal totalInterestWithEarlyPayment = 0;
                if (newOutstanding > 0)
                {
                    totalInterestWithEarlyPayment = CalculateTotalInterest(
                        newOutstanding,
                        request.AnnualInterestRate,
                        request.RemainingTenureMonths);
                }

                result.InterestSaved = Math.Round(totalInterestWithoutEarlyPayment - totalInterestWithEarlyPayment, 2);
                result.NewOutstandingBalance = newOutstanding;
                result.LoanFullyPaid = newOutstanding == 0;

                // Calculate new EMI if loan not fully paid
                if (!result.LoanFullyPaid)
                {
                    result.NewMonthlyEMI = CalculateEMI(
                        newOutstanding,
                        request.AnnualInterestRate,
                        request.RemainingTenureMonths);
                }

                _logger.LogInformation(
                    "Early repayment calculation: Interest saved ₦{Saved:N2}, New outstanding ₦{Outstanding:N2}, Fully paid: {FullyPaid}",
                    result.InterestSaved, result.NewOutstandingBalance, result.LoanFullyPaid);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating early repayment");
                throw;
            }
        }

        /// <summary>
        /// Calculate outstanding balance
        /// </summary>
        public decimal CalculateOutstandingBalance(decimal principal, decimal annualInterestRate, int tenureMonths, int paymentsMade)
        {
            try
            {
                if (paymentsMade >= tenureMonths)
                {
                    return 0;
                }

                if (paymentsMade == 0)
                {
                    return principal;
                }

                // Generate schedule and get balance after payments made
                var request = new AmortizationScheduleRequest
                {
                    Principal = principal,
                    AnnualInterestRate = annualInterestRate,
                    TenureMonths = tenureMonths,
                    StartDate = DateTime.UtcNow
                };

                var schedule = GenerateAmortizationSchedule(request);
                var lastPayment = schedule.Installments[paymentsMade - 1];

                return lastPayment.ClosingBalance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating outstanding balance");
                throw;
            }
        }

        /// <summary>
        /// Calculate installment breakdown
        /// </summary>
        public InstallmentBreakdown CalculateInstallmentBreakdown(decimal outstandingBalance, decimal monthlyInterestRate, decimal emiAmount)
        {
            try
            {
                decimal interestAmount = Math.Round(outstandingBalance * monthlyInterestRate, 2);
                decimal principalAmount = Math.Round(emiAmount - interestAmount, 2);
                decimal remainingBalance = Math.Max(0, outstandingBalance - principalAmount);

                return new InstallmentBreakdown
                {
                    InterestAmount = interestAmount,
                    PrincipalAmount = principalAmount,
                    TotalPayment = emiAmount,
                    RemainingBalance = remainingBalance
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating installment breakdown");
                throw;
            }
        }

        /// <summary>
        /// Validate loan parameters
        /// </summary>
        public ValidationResult ValidateLoanParameters(decimal principal, decimal annualInterestRate, int tenureMonths)
        {
            var result = new ValidationResult { IsValid = true };

            if (principal <= 0)
            {
                result.IsValid = false;
                result.Errors.Add("Principal must be greater than zero");
            }

            if (principal > 100000000) // 100 million max
            {
                result.IsValid = false;
                result.Errors.Add("Principal exceeds maximum allowed amount");
            }

            if (annualInterestRate < 0)
            {
                result.IsValid = false;
                result.Errors.Add("Interest rate cannot be negative");
            }

            if (annualInterestRate > 100)
            {
                result.IsValid = false;
                result.Errors.Add("Interest rate exceeds maximum allowed (100%)");
            }

            if (tenureMonths <= 0)
            {
                result.IsValid = false;
                result.Errors.Add("Tenure must be greater than zero");
            }

            if (tenureMonths > 360) // 30 years max
            {
                result.IsValid = false;
                result.Errors.Add("Tenure exceeds maximum allowed (360 months)");
            }

            return result;
        }
    }
}
