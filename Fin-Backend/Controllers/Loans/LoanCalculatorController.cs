using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using FinTech.Core.Application.Services.LoanCalculation;

namespace FinTech.Controllers.V1
{
    /// <summary>
    /// Member Loan Calculator - Self-Service Tools
    /// Allows members to check borrowing capacity and calculate repayment schedules
    /// Promotes transparency and financial literacy
    /// </summary>
    [ApiController]
    [Route("api/v1/loan-calculator")]
    [ApiVersion("1.0")]
    public class LoanCalculatorController : ControllerBase
    {
        private readonly ILoanCalculatorService _loanCalculator;

        public LoanCalculatorController(ILoanCalculatorService loanCalculator)
        {
            _loanCalculator = loanCalculator;
        }

        /// <summary>
        /// Calculate maximum loan amount member can borrow
        /// </summary>
        /// <remarks>
        /// Returns maximum borrowing capacity based on:
        /// - Current savings balance
        /// - Configured loan multiplier for loan type
        /// - Loan type maximum amount
        /// - CBN deduction limit constraints (40% of income)
        /// 
        /// Example Response:
        /// {
        ///   "currentSavings": 200000,
        ///   "maxLoanAmount": 600000,
        ///   "recommendedLoanAmount": 540000,
        ///   "isEligible": true
        /// }
        /// </remarks>
        [HttpGet("member/{memberId}/loan-capacity/{loanTypeId}")]
        [ProducesResponseType(typeof(LoanCapacityResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetLoanCapacity(int memberId, int loanTypeId)
        {
            var result = await _loanCalculator.CalculateMemberLoanCapacity(memberId, loanTypeId);
            return Ok(result);
        }

        /// <summary>
        /// Calculate monthly repayment amount and generate payment schedule
        /// </summary>
        /// <remarks>
        /// Calculates using standard amortization formula
        /// Returns:
        /// - Monthly payment amount
        /// - Total interest cost
        /// - Total repayable amount
        /// - Full payment schedule with dates
        /// - First 3 and last 3 month examples
        /// 
        /// Example:
        /// POST /loan-calculator/calculate-repayment
        /// {
        ///   "principalAmount": 500000,
        ///   "annualInterestRate": 18,
        ///   "loanTermMonths": 24,
        ///   "repaymentFrequency": "Monthly"
        /// }
        /// </remarks>
        [HttpPost("calculate-repayment")]
        [ProducesResponseType(typeof(LoanRepaymentCalculation), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult CalculateRepayment([FromBody] RepaymentCalculationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = _loanCalculator.CalculateMonthlyRepayment(
                    request.PrincipalAmount,
                    request.AnnualInterestRate,
                    request.LoanTermMonths,
                    request.RepaymentFrequency);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Check member eligibility for a specific loan amount
        /// </summary>
        /// <remarks>
        /// Returns eligibility status including:
        /// - Whether member is eligible
        /// - Credit score
        /// - Risk rating (Low, Medium, High, Critical)
        /// - Failed eligibility criteria
        /// - Passed eligibility criteria
        /// - Whether committee review is required
        /// - Detailed message explaining decision
        /// </remarks>
        [HttpGet("member/{memberId}/check-eligibility")]
        [ProducesResponseType(typeof(EligibilityCheckResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CheckEligibility(
            int memberId,
            [FromQuery] int loanTypeId,
            [FromQuery] decimal requestedAmount)
        {
            try
            {
                var result = await _loanCalculator.CheckMemberEligibility(memberId, loanTypeId, requestedAmount);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Analyze member's deduction compliance
        /// </summary>
        /// <remarks>
        /// Verifies that monthly loan payment doesn't exceed CBN guideline of 40% of monthly income
        /// Essential for ensuring sustainable repayment and compliance
        /// Returns:
        /// - Current deductions on all active loans
        /// - Proposed new deduction
        /// - Total deductions after new loan
        /// - Maximum allowed deduction
        /// - Compliance status (Compliant or Exceeds limit)
        /// </remarks>
        [HttpPost("member/{memberId}/analyze-deduction")]
        [ProducesResponseType(typeof(DeductionAnalysisResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AnalyzeDeduction(int memberId, [FromBody] DeductionAnalysisRequest request)
        {
            try
            {
                var result = await _loanCalculator.AnalyzeDeductionCompliance(
                    memberId,
                    request.MonthlyDeduction,
                    request.MonthlyIncome);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Estimate total cost of a loan
        /// </summary>
        /// <remarks>
        /// Provides cost breakdown including:
        /// - Principal amount
        /// - Total interest
        /// - Processing fees
        /// - Other fees
        /// - Total cost
        /// - Total repayable amount
        /// - Effective interest rate
        /// </remarks>
        [HttpPost("estimate-loan-cost")]
        [ProducesResponseType(typeof(LoanCostEstimate), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult EstimateLoanCost([FromBody] LoanCostEstimateRequest request)
        {
            try
            {
                var result = _loanCalculator.EstimateLoanCost(
                    request.PrincipalAmount,
                    request.AnnualInterestRate,
                    request.LoanTermMonths,
                    request.ProcessingFeePercent);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Calculate member's credit score based on repayment history
        /// </summary>
        /// <remarks>
        /// Score Range: 0-100
        /// - 80+: Excellent (auto-approval eligible)
        /// - 60-79: Good (auto-approval with conditions)
        /// - 40-59: Fair (requires committee review)
        /// - &lt;40: Poor (likely rejection or special review)
        /// 
        /// Based on factors:
        /// - Successful loan repayments (5 pts each, max 20)
        /// - Previous defaults (-10 pts each, max -20)
        /// - On-time payments (+5 pts per active loan)
        /// - Late payments (-2 pts each)
        /// - Savings consistency (+10 pts if has savings)
        /// </remarks>
        [HttpGet("member/{memberId}/credit-score")]
        [ProducesResponseType(typeof(CreditScoreResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCreditScore(int memberId)
        {
            try
            {
                var score = await _loanCalculator.CalculateMemberCreditScore(memberId);
                var riskRating = score >= 80 ? "Low"
                    : score >= 60 ? "Medium"
                    : score >= 40 ? "High"
                    : "Critical";

                return Ok(new CreditScoreResponse
                {
                    MemberId = memberId,
                    CreditScore = score,
                    RiskRating = riskRating
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Analyze member's savings requirement for a loan
        /// </summary>
        /// <remarks>
        /// Checks if member meets:
        /// - Minimum absolute savings amount
        /// - Savings-to-loan ratio (25% minimum)
        /// - Provides gap analysis if below requirements
        /// </remarks>
        [HttpGet("member/{memberId}/savings-analysis")]
        [ProducesResponseType(typeof(SavingsAnalysisResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AnalyzeSavings(
            int memberId,
            [FromQuery] decimal requestedLoanAmount,
            [FromQuery] int loanTypeId)
        {
            try
            {
                var result = await _loanCalculator.AnalyzeSavingsRequirement(memberId, requestedLoanAmount, loanTypeId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    // Request DTOs
    public class RepaymentCalculationRequest
    {
        public decimal PrincipalAmount { get; set; }
        public decimal AnnualInterestRate { get; set; }
        public int LoanTermMonths { get; set; }
        public string RepaymentFrequency { get; set; } = "Monthly";
    }

    public class DeductionAnalysisRequest
    {
        public decimal MonthlyDeduction { get; set; }
        public decimal MonthlyIncome { get; set; }
    }

    public class LoanCostEstimateRequest
    {
        public decimal PrincipalAmount { get; set; }
        public decimal AnnualInterestRate { get; set; }
        public int LoanTermMonths { get; set; }
        public decimal ProcessingFeePercent { get; set; }
    }

    public class CreditScoreResponse
    {
        public int MemberId { get; set; }
        public decimal CreditScore { get; set; }
        public string RiskRating { get; set; }
    }
}
