using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Loans;

namespace FinTech.Core.Application.Interfaces.Loans
{
    /// <summary>
    /// Service for checking loan eligibility based on cooperative rules
    /// </summary>
    public interface ILoanEligibilityService
    {
        /// <summary>
        /// Check complete eligibility for a loan application
        /// </summary>
        /// <param name="request">Eligibility check request</param>
        /// <returns>Detailed eligibility result</returns>
        Task<LoanEligibilityResultDto> CheckEligibilityAsync(LoanEligibilityRequest request);

        /// <summary>
        /// Check savings multiplier eligibility
        /// </summary>
        /// <param name="memberId">Member ID</param>
        /// <param name="requestedAmount">Requested loan amount</param>
        /// <param name="loanProductId">Loan product ID</param>
        /// <returns>Savings multiplier check result</returns>
        Task<SavingsMultiplierCheckDto> CheckSavingsMultiplierAsync(string memberId, decimal requestedAmount, string loanProductId);

        /// <summary>
        /// Check membership duration eligibility
        /// </summary>
        /// <param name="memberId">Member ID</param>
        /// <param name="minimumMonths">Minimum required months</param>
        /// <returns>Membership duration check result</returns>
        Task<MembershipDurationCheckDto> CheckMembershipDurationAsync(string memberId, int minimumMonths);

        /// <summary>
        /// Calculate deduction rate headroom
        /// </summary>
        /// <param name="memberId">Member ID</param>
        /// <param name="requestedAmount">Requested loan amount</param>
        /// <param name="tenureMonths">Loan tenure in months</param>
        /// <param name="interestRate">Annual interest rate</param>
        /// <returns>Deduction rate headroom result</returns>
        Task<DeductionRateHeadroomDto> CalculateDeductionRateHeadroomAsync(
            string memberId, 
            decimal requestedAmount, 
            int tenureMonths, 
            decimal interestRate);

        /// <summary>
        /// Check debt-to-income ratio
        /// </summary>
        /// <param name="memberId">Member ID</param>
        /// <param name="requestedAmount">Requested loan amount</param>
        /// <param name="tenureMonths">Loan tenure in months</param>
        /// <param name="interestRate">Annual interest rate</param>
        /// <returns>Debt-to-income ratio check result</returns>
        Task<DebtToIncomeRatioDto> CheckDebtToIncomeRatioAsync(
            string memberId, 
            decimal requestedAmount, 
            int tenureMonths, 
            decimal interestRate);

        /// <summary>
        /// Generate eligibility report
        /// </summary>
        /// <param name="memberId">Member ID</param>
        /// <param name="loanProductId">Loan product ID</param>
        /// <returns>Comprehensive eligibility report</returns>
        Task<EligibilityReportDto> GenerateEligibilityReportAsync(string memberId, string loanProductId);

        /// <summary>
        /// Calculate maximum eligible loan amount
        /// </summary>
        /// <param name="memberId">Member ID</param>
        /// <param name="loanProductId">Loan product ID</param>
        /// <returns>Maximum eligible amount</returns>
        Task<MaximumEligibleAmountDto> CalculateMaximumEligibleAmountAsync(string memberId, string loanProductId);
    }
}
