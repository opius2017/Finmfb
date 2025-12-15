using System;
using System.Collections.Generic;
using FinTech.Core.Domain.Entities.Loans;

namespace FinTech.Core.Domain.Services
{
    /// <summary>
    /// Service for checking loan eligibility for cooperative members
    /// </summary>
    public interface ILoanEligibilityChecker
    {
        /// <summary>
        /// Check if member is eligible for a loan
        /// </summary>
        /// <param name="member">Member to check</param>
        /// <param name="loanAmount">Requested loan amount</param>
        /// <param name="loanType">Type of loan (Normal, Commodity, Car)</param>
        /// <param name="tenorMonths">Loan tenor in months</param>
        /// <param name="interestRate">Annual interest rate</param>
        /// <returns>Eligibility result with details</returns>
        LoanEligibilityResult CheckEligibility(Member member, decimal loanAmount, LoanType loanType, int tenorMonths, decimal interestRate);
        
        /// <summary>
        /// Get savings multiplier for loan type
        /// </summary>
        /// <param name="loanType">Type of loan</param>
        /// <returns>Savings multiplier (e.g., 2.0 for 200%)</returns>
        decimal GetSavingsMultiplier(LoanType loanType);
        
        /// <summary>
        /// Get minimum membership duration in months
        /// </summary>
        /// <returns>Minimum months of membership required</returns>
        int GetMinimumMembershipDuration();
        
        /// <summary>
        /// Get maximum deduction rate
        /// </summary>
        /// <returns>Maximum deduction rate as decimal (e.g., 0.45 for 45%)</returns>
        decimal GetMaximumDeductionRate();
    }
    
    /// <summary>
    /// Loan types with different savings multipliers
    /// </summary>
    public enum LoanType
    {
        Normal = 1,      // 200% savings multiplier
        Commodity = 2,   // 300% savings multiplier
        Car = 3          // 500% savings multiplier
    }
    
    /// <summary>
    /// Result of loan eligibility check
    /// </summary>
    public class LoanEligibilityResult
    {
        public bool IsEligible { get; set; }
        public List<string> Reasons { get; set; } = new List<string>();
        public EligibilityDetails Details { get; set; } = new EligibilityDetails();
        
        public void AddReason(string reason)
        {
            Reasons.Add(reason);
            IsEligible = false;
        }
    }
    
    /// <summary>
    /// Detailed eligibility information
    /// </summary>
    public class EligibilityDetails
    {
        // Savings Check
        public decimal RequestedAmount { get; set; }
        public decimal RequiredSavings { get; set; }
        public decimal ActualSavings { get; set; }
        public decimal SavingsMultiplier { get; set; }
        public bool MeetsSavingsRequirement { get; set; }
        
        // Membership Duration Check
        public int MembershipDurationMonths { get; set; }
        public int RequiredMembershipMonths { get; set; }
        public bool MeetsMembershipDuration { get; set; }
        
        // Deduction Rate Check
        public decimal MonthlyEMI { get; set; }
        public decimal CurrentMonthlyDeductions { get; set; }
        public decimal TotalMonthlyDeductions { get; set; }
        public decimal NetSalary { get; set; }
        public decimal DeductionRate { get; set; }
        public decimal MaxDeductionRate { get; set; }
        public decimal DeductionRateHeadroom { get; set; }
        public bool MeetsDeductionRateRequirement { get; set; }
        
        // Debt-to-Income Check
        public decimal TotalOutstandingLoans { get; set; }
        public decimal NewLoanAmount { get; set; }
        public decimal TotalDebt { get; set; }
        public decimal MonthlyIncome { get; set; }
        public decimal DebtToIncomeRatio { get; set; }
        public decimal MaxDebtToIncomeRatio { get; set; }
        public bool MeetsDebtToIncomeRequirement { get; set; }
    }
}
