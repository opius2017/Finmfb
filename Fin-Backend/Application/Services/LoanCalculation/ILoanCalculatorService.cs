using System;
using System.Threading.Tasks;

namespace FinTech.Core.Application.Services.LoanCalculation
{
    /// <summary>
    /// Service for calculating loan eligibility and member loan capacity
    /// Implements business rules aligned with microfinance best practices
    /// </summary>
    public interface ILoanCalculatorService
    {
        /// <summary>
        /// Calculate maximum loan amount a member can borrow based on savings and configuration
        /// </summary>
        Task<LoanCapacityResult> CalculateMemberLoanCapacity(int memberId, int loanTypeId);

        /// <summary>
        /// Calculate monthly repayment amount and schedule
        /// </summary>
        LoanRepaymentCalculation CalculateMonthlyRepayment(decimal principalAmount, decimal annualInterestRate, int loanTermMonths, string repaymentFrequency);

        /// <summary>
        /// Check if member meets basic eligibility criteria
        /// </summary>
        Task<EligibilityCheckResult> CheckMemberEligibility(int memberId, int loanTypeId, decimal requestedAmount);

        /// <summary>
        /// Calculate whether member's deduction would exceed maximum allowed
        /// Implements Central Bank compliance rules
        /// </summary>
        Task<DeductionAnalysisResult> AnalyzeDeductionCompliance(int memberId, decimal monthlyDeduction, decimal monthlyIncome);

        /// <summary>
        /// Estimate total cost of loan including interest and fees
        /// </summary>
        LoanCostEstimate EstimateLoanCost(decimal principalAmount, decimal annualInterestRate, int loanTermMonths, decimal processingFeePercent);

        /// <summary>
        /// Calculate credit score based on repayment history
        /// </summary>
        Task<decimal> CalculateMemberCreditScore(int memberId);

        /// <summary>
        /// Check savings adequacy for loan application
        /// </summary>
        Task<SavingsAnalysisResult> AnalyzeSavingsRequirement(int memberId, decimal requestedLoanAmount, int loanTypeId);
    }

    public class LoanCapacityResult
    {
        public int MemberId { get; set; }
        public decimal CurrentSavings { get; set; }
        public decimal MaxLoanMultiplier { get; set; }
        public decimal MaxLoanAmount { get; set; }
        public decimal RecommendedLoanAmount { get; set; }
        public bool IsEligible { get; set; }
        public string EligibilityNotes { get; set; }
    }

    public class LoanRepaymentCalculation
    {
        public decimal MonthlyPayment { get; set; }
        public decimal TotalPayable { get; set; }
        public decimal TotalInterest { get; set; }
        public int LoanTermMonths { get; set; }
        public decimal[] MonthlySchedule { get; set; }
        public DateTime[] PaymentDates { get; set; }
    }

    public class EligibilityCheckResult
    {
        public int MemberId { get; set; }
        public bool IsEligible { get; set; }
        public decimal CreditScore { get; set; }
        public string RiskRating { get; set; }
        public string[] FailedCriteria { get; set; }
        public string[] PassedCriteria { get; set; }
        public bool RequiresCommitteeReview { get; set; }
        public string Message { get; set; }
    }

    public class DeductionAnalysisResult
    {
        public decimal RequestedMonthlyDeduction { get; set; }
        public decimal MemberMonthlyIncome { get; set; }
        public decimal CurrentDeductions { get; set; }
        public decimal TotalDeductionsAfterLoan { get; set; }
        public decimal MaxAllowedDeduction { get; set; }
        public decimal DeductionPercentage { get; set; }
        public bool IsCompliant { get; set; }
        public string ComplianceStatus { get; set; }
        public string Message { get; set; }
    }

    public class LoanCostEstimate
    {
        public decimal PrincipalAmount { get; set; }
        public decimal TotalInterest { get; set; }
        public decimal ProcessingFee { get; set; }
        public decimal OtherFees { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalPayable { get; set; }
        public decimal EffectiveInterestRate { get; set; }
    }

    public class SavingsAnalysisResult
    {
        public int MemberId { get; set; }
        public decimal CurrentSavings { get; set; }
        public decimal MinimumSavingsRequired { get; set; }
        public decimal SavingsToLoanRatio { get; set; }
        public decimal MinimumSavingsRatioRequired { get; set; }
        public bool MeetsMinimumSavings { get; set; }
        public bool MeetsMinimumRatio { get; set; }
        public string AnalysisMessage { get; set; }
    }
}
