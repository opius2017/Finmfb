using System;
using System.Collections.Generic;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Core.Domain.Services;
using Microsoft.Extensions.Logging;

namespace FinTech.Infrastructure.Services
{
    /// <summary>
    /// Implementation of loan eligibility checker for cooperative members
    /// </summary>
    public class LoanEligibilityCheckerService : ILoanEligibilityChecker
    {
        private readonly ILoanCalculator _loanCalculator;
        private readonly ILogger<LoanEligibilityCheckerService> _logger;
        
        // Configuration constants (should be moved to configuration)
        private const decimal NORMAL_LOAN_MULTIPLIER = 2.0m;      // 200%
        private const decimal COMMODITY_LOAN_MULTIPLIER = 3.0m;   // 300%
        private const decimal CAR_LOAN_MULTIPLIER = 5.0m;         // 500%
        private const int MINIMUM_MEMBERSHIP_MONTHS = 6;
        private const decimal MAXIMUM_DEDUCTION_RATE = 0.45m;     // 45%
        private const decimal MAXIMUM_DEBT_TO_INCOME_RATIO = 0.40m; // 40%
        
        public LoanEligibilityCheckerService(
            ILoanCalculator loanCalculator,
            ILogger<LoanEligibilityCheckerService> logger)
        {
            _loanCalculator = loanCalculator ?? throw new ArgumentNullException(nameof(loanCalculator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Check comprehensive loan eligibility
        /// </summary>
        public LoanEligibilityResult CheckEligibility(
            Member member,
            decimal loanAmount,
            LoanType loanType,
            int tenorMonths,
            decimal interestRate)
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member));
            
            if (loanAmount <= 0)
                throw new ArgumentException("Loan amount must be greater than zero", nameof(loanAmount));
            
            if (tenorMonths <= 0)
                throw new ArgumentException("Tenor must be greater than zero", nameof(tenorMonths));
            
            var result = new LoanEligibilityResult
            {
                IsEligible = true,
                Details = new EligibilityDetails
                {
                    RequestedAmount = loanAmount,
                    SavingsMultiplier = GetSavingsMultiplier(loanType)
                }
            };
            
            _logger.LogInformation(
                "Checking eligibility for Member={MemberNumber}, Amount={Amount}, Type={Type}",
                member.MemberNumber, loanAmount, loanType);
            
            // 1. Check savings multiplier requirement
            CheckSavingsRequirement(member, loanAmount, loanType, result);
            
            // 2. Check membership duration
            CheckMembershipDuration(member, result);
            
            // 3. Check deduction rate headroom (for salaried workers)
            if (member.NetSalary.HasValue && member.NetSalary.Value > 0)
            {
                decimal monthlyEMI = _loanCalculator.CalculateEMI(loanAmount, interestRate, tenorMonths);
                CheckDeductionRateHeadroom(member, monthlyEMI, result);
            }
            
            // 4. Check debt-to-income ratio
            if (member.NetSalary.HasValue && member.NetSalary.Value > 0)
            {
                CheckDebtToIncomeRatio(member, loanAmount, result);
            }
            
            // 5. Check if member is active
            if (!member.IsActive)
            {
                result.AddReason("Member account is not active");
            }
            
            _logger.LogInformation(
                "Eligibility check complete: Member={MemberNumber}, Eligible={IsEligible}, Reasons={ReasonCount}",
                member.MemberNumber, result.IsEligible, result.Reasons.Count);
            
            return result;
        }
        
        /// <summary>
        /// Check if member meets savings multiplier requirement
        /// </summary>
        private void CheckSavingsRequirement(Member member, decimal loanAmount, LoanType loanType, LoanEligibilityResult result)
        {
            decimal multiplier = GetSavingsMultiplier(loanType);
            decimal requiredSavings = loanAmount / multiplier;
            
            result.Details.RequiredSavings = requiredSavings;
            result.Details.ActualSavings = member.TotalSavings;
            result.Details.MeetsSavingsRequirement = member.TotalSavings >= requiredSavings;
            
            if (!result.Details.MeetsSavingsRequirement)
            {
                result.AddReason(
                    $"Insufficient savings balance. Required: ₦{requiredSavings:N2}, Available: ₦{member.TotalSavings:N2}");
            }
        }
        
        /// <summary>
        /// Check if member meets minimum membership duration
        /// </summary>
        private void CheckMembershipDuration(Member member, LoanEligibilityResult result)
        {
            int membershipMonths = (int)((DateTime.UtcNow - member.MembershipDate).TotalDays / 30.44);
            int requiredMonths = GetMinimumMembershipDuration();
            
            result.Details.MembershipDurationMonths = membershipMonths;
            result.Details.RequiredMembershipMonths = requiredMonths;
            result.Details.MeetsMembershipDuration = membershipMonths >= requiredMonths;
            
            if (!result.Details.MeetsMembershipDuration)
            {
                result.AddReason(
                    $"Membership duration below minimum. Required: {requiredMonths} months, Current: {membershipMonths} months");
            }
        }
        
        /// <summary>
        /// Check if member has sufficient deduction rate headroom
        /// </summary>
        private void CheckDeductionRateHeadroom(Member member, decimal monthlyEMI, LoanEligibilityResult result)
        {
            if (!member.NetSalary.HasValue || member.NetSalary.Value == 0)
            {
                result.AddReason("Net salary information is required for deduction rate calculation");
                return;
            }
            
            decimal currentDeductions = member.MonthlyContribution + member.TotalOutstandingLoans;
            decimal totalDeductions = currentDeductions + monthlyEMI;
            decimal deductionRate = totalDeductions / member.NetSalary.Value;
            decimal maxRate = GetMaximumDeductionRate();
            
            result.Details.MonthlyEMI = monthlyEMI;
            result.Details.CurrentMonthlyDeductions = currentDeductions;
            result.Details.TotalMonthlyDeductions = totalDeductions;
            result.Details.NetSalary = member.NetSalary.Value;
            result.Details.DeductionRate = deductionRate;
            result.Details.MaxDeductionRate = maxRate;
            result.Details.DeductionRateHeadroom = Math.Max(0, maxRate - deductionRate);
            result.Details.MeetsDeductionRateRequirement = deductionRate <= maxRate;
            
            if (!result.Details.MeetsDeductionRateRequirement)
            {
                result.AddReason(
                    $"Deduction rate exceeds maximum. Current: {deductionRate:P2}, Maximum: {maxRate:P2}, " +
                    $"Monthly EMI: ₦{monthlyEMI:N2}, Net Salary: ₦{member.NetSalary.Value:N2}");
            }
        }
        
        /// <summary>
        /// Check debt-to-income ratio
        /// </summary>
        private void CheckDebtToIncomeRatio(Member member, decimal newLoanAmount, LoanEligibilityResult result)
        {
            if (!member.NetSalary.HasValue || member.NetSalary.Value == 0)
            {
                return;
            }
            
            decimal totalDebt = member.TotalOutstandingLoans + newLoanAmount;
            decimal monthlyIncome = member.NetSalary.Value;
            decimal debtToIncomeRatio = totalDebt / (monthlyIncome * 12); // Annualized
            
            result.Details.TotalOutstandingLoans = member.TotalOutstandingLoans;
            result.Details.NewLoanAmount = newLoanAmount;
            result.Details.TotalDebt = totalDebt;
            result.Details.MonthlyIncome = monthlyIncome;
            result.Details.DebtToIncomeRatio = debtToIncomeRatio;
            result.Details.MaxDebtToIncomeRatio = MAXIMUM_DEBT_TO_INCOME_RATIO;
            result.Details.MeetsDebtToIncomeRequirement = debtToIncomeRatio <= MAXIMUM_DEBT_TO_INCOME_RATIO;
            
            if (!result.Details.MeetsDebtToIncomeRequirement)
            {
                result.AddReason(
                    $"Debt-to-income ratio exceeds maximum. Current: {debtToIncomeRatio:P2}, Maximum: {MAXIMUM_DEBT_TO_INCOME_RATIO:P2}");
            }
        }
        
        /// <summary>
        /// Get savings multiplier for loan type
        /// </summary>
        public decimal GetSavingsMultiplier(LoanType loanType)
        {
            return loanType switch
            {
                LoanType.Normal => NORMAL_LOAN_MULTIPLIER,
                LoanType.Commodity => COMMODITY_LOAN_MULTIPLIER,
                LoanType.Car => CAR_LOAN_MULTIPLIER,
                _ => throw new ArgumentException($"Unknown loan type: {loanType}", nameof(loanType))
            };
        }
        
        /// <summary>
        /// Get minimum membership duration
        /// </summary>
        public int GetMinimumMembershipDuration()
        {
            return MINIMUM_MEMBERSHIP_MONTHS;
        }
        
        /// <summary>
        /// Get maximum deduction rate
        /// </summary>
        public decimal GetMaximumDeductionRate()
        {
            return MAXIMUM_DEDUCTION_RATE;
        }
    }
}
