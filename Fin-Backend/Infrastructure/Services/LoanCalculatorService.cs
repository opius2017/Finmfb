using System;
using System.Collections.Generic;
using FinTech.Core.Domain.Services;
using Microsoft.Extensions.Logging;

namespace FinTech.Infrastructure.Services
{
    /// <summary>
    /// Implementation of loan calculator using reducing balance method
    /// </summary>
    public class LoanCalculatorService : ILoanCalculator
    {
        private readonly ILogger<LoanCalculatorService> _logger;
        
        public LoanCalculatorService(ILogger<LoanCalculatorService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Calculate monthly EMI using reducing balance method
        /// Formula: EMI = P × r × (1 + r)^n / ((1 + r)^n - 1)
        /// Where: P = Principal, r = Monthly interest rate, n = Number of months
        /// </summary>
        public decimal CalculateEMI(decimal principal, decimal annualInterestRate, int tenorMonths)
        {
            if (principal <= 0)
                throw new ArgumentException("Principal must be greater than zero", nameof(principal));
            
            if (annualInterestRate < 0)
                throw new ArgumentException("Interest rate cannot be negative", nameof(annualInterestRate));
            
            if (tenorMonths <= 0)
                throw new ArgumentException("Tenor must be greater than zero", nameof(tenorMonths));
            
            // Handle zero interest rate case
            if (annualInterestRate == 0)
            {
                return Math.Round(principal / tenorMonths, 2);
            }
            
            // Convert annual rate to monthly rate (as decimal)
            decimal monthlyRate = (annualInterestRate / 100) / 12;
            
            // Calculate EMI using the formula
            double rateDouble = (double)monthlyRate;
            double principalDouble = (double)principal;
            int n = tenorMonths;
            
            double emi = principalDouble * rateDouble * Math.Pow(1 + rateDouble, n) / (Math.Pow(1 + rateDouble, n) - 1);
            
            decimal result = Math.Round((decimal)emi, 2);
            
            _logger.LogInformation(
                "Calculated EMI: Principal={Principal}, Rate={Rate}%, Tenor={Tenor} months, EMI={EMI}",
                principal, annualInterestRate, tenorMonths, result);
            
            return result;
        }
        
        /// <summary>
        /// Calculate total interest payable
        /// </summary>
        public decimal CalculateTotalInterest(decimal emi, int tenorMonths, decimal principal)
        {
            if (emi < 0 || tenorMonths <= 0 || principal < 0)
                throw new ArgumentException("Invalid input parameters for interest calculation");
            
            decimal totalRepayment = emi * tenorMonths;
            decimal totalInterest = totalRepayment - principal;
            
            return Math.Round(totalInterest, 2);
        }
        
        /// <summary>
        /// Generate complete amortization schedule using reducing balance method
        /// </summary>
        public List<AmortizationEntry> GenerateAmortizationSchedule(LoanCalculationInput input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            
            var schedule = new List<AmortizationEntry>();
            
            decimal emi = CalculateEMI(input.PrincipalAmount, input.AnnualInterestRate, input.TenorMonths);
            decimal monthlyRate = (input.AnnualInterestRate / 100) / 12;
            decimal outstandingBalance = input.PrincipalAmount;
            DateTime currentDueDate = input.FirstPaymentDate;
            
            for (int i = 1; i <= input.TenorMonths; i++)
            {
                // Calculate interest on outstanding balance
                decimal interestDue = Math.Round(outstandingBalance * monthlyRate, 2);
                
                // Calculate principal component
                decimal principalDue = emi - interestDue;
                
                // For last installment, adjust to clear any rounding differences
                if (i == input.TenorMonths)
                {
                    principalDue = outstandingBalance;
                    interestDue = emi - principalDue;
                }
                
                decimal closingBalance = outstandingBalance - principalDue;
                
                var entry = new AmortizationEntry
                {
                    InstallmentNumber = i,
                    DueDate = currentDueDate,
                    OpeningBalance = Math.Round(outstandingBalance, 2),
                    PrincipalDue = Math.Round(principalDue, 2),
                    InterestDue = Math.Round(interestDue, 2),
                    TotalDue = Math.Round(emi, 2),
                    ClosingBalance = Math.Round(Math.Max(0, closingBalance), 2)
                };
                
                schedule.Add(entry);
                
                // Update for next iteration
                outstandingBalance = closingBalance;
                currentDueDate = currentDueDate.AddMonths(1);
            }
            
            _logger.LogInformation(
                "Generated amortization schedule: Principal={Principal}, Tenor={Tenor} months, Total Entries={Count}",
                input.PrincipalAmount, input.TenorMonths, schedule.Count);
            
            return schedule;
        }
        
        /// <summary>
        /// Calculate penalty for late payment
        /// </summary>
        public decimal CalculatePenalty(decimal overdueAmount, int daysOverdue, decimal penaltyRate)
        {
            if (overdueAmount < 0)
                throw new ArgumentException("Overdue amount cannot be negative", nameof(overdueAmount));
            
            if (daysOverdue < 0)
                throw new ArgumentException("Days overdue cannot be negative", nameof(daysOverdue));
            
            if (penaltyRate < 0)
                throw new ArgumentException("Penalty rate cannot be negative", nameof(penaltyRate));
            
            // Calculate penalty: Overdue Amount × Daily Rate × Days Overdue
            decimal penalty = overdueAmount * (penaltyRate / 100) * daysOverdue;
            
            return Math.Round(penalty, 2);
        }
        
        /// <summary>
        /// Calculate early repayment amount
        /// </summary>
        public decimal CalculateEarlyRepaymentAmount(decimal outstandingPrincipal, DateTime lastPaymentDate, decimal annualInterestRate)
        {
            if (outstandingPrincipal < 0)
                throw new ArgumentException("Outstanding principal cannot be negative", nameof(outstandingPrincipal));
            
            if (annualInterestRate < 0)
                throw new ArgumentException("Interest rate cannot be negative", nameof(annualInterestRate));
            
            // Calculate days since last payment
            int daysSinceLastPayment = (DateTime.UtcNow - lastPaymentDate).Days;
            
            // Calculate accrued interest
            decimal dailyRate = (annualInterestRate / 100) / 365;
            decimal accruedInterest = Math.Round(outstandingPrincipal * dailyRate * daysSinceLastPayment, 2);
            
            decimal totalAmount = outstandingPrincipal + accruedInterest;
            
            _logger.LogInformation(
                "Early repayment calculation: Principal={Principal}, Accrued Interest={Interest}, Total={Total}",
                outstandingPrincipal, accruedInterest, totalAmount);
            
            return totalAmount;
        }
        
        /// <summary>
        /// Allocate payment to interest first, then principal (reducing balance method)
        /// </summary>
        public PaymentAllocation AllocatePayment(decimal paymentAmount, decimal outstandingPrincipal, decimal accruedInterest)
        {
            if (paymentAmount < 0)
                throw new ArgumentException("Payment amount cannot be negative", nameof(paymentAmount));
            
            if (outstandingPrincipal < 0)
                throw new ArgumentException("Outstanding principal cannot be negative", nameof(outstandingPrincipal));
            
            if (accruedInterest < 0)
                throw new ArgumentException("Accrued interest cannot be negative", nameof(accruedInterest));
            
            decimal interestPayment = 0;
            decimal principalPayment = 0;
            decimal remainingPayment = paymentAmount;
            
            // First, pay off accrued interest
            if (remainingPayment > 0 && accruedInterest > 0)
            {
                interestPayment = Math.Min(remainingPayment, accruedInterest);
                remainingPayment -= interestPayment;
            }
            
            // Then, pay off principal
            if (remainingPayment > 0 && outstandingPrincipal > 0)
            {
                principalPayment = Math.Min(remainingPayment, outstandingPrincipal);
                remainingPayment -= principalPayment;
            }
            
            decimal newBalance = outstandingPrincipal - principalPayment;
            
            var allocation = new PaymentAllocation
            {
                TotalPayment = paymentAmount,
                InterestPayment = Math.Round(interestPayment, 2),
                PrincipalPayment = Math.Round(principalPayment, 2),
                RemainingBalance = Math.Round(Math.Max(0, newBalance), 2)
            };
            
            _logger.LogInformation(
                "Payment allocation: Total={Total}, Interest={Interest}, Principal={Principal}, Remaining={Remaining}",
                allocation.TotalPayment, allocation.InterestPayment, allocation.PrincipalPayment, allocation.RemainingBalance);
            
            return allocation;
        }
    }
}
