using System;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Loans;
using FinTech.Core.Application.Services.Loans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FinTech.Controllers.Loans
{
    /// <summary>
    /// Controller for loan eligibility checks
    /// </summary>
    [ApiController]
    [Route("api/loan-eligibility")]
    [Authorize]
    public class LoanEligibilityController : ControllerBase
    {
        private readonly ILoanEligibilityService _eligibilityService;
        private readonly ILogger<LoanEligibilityController> _logger;

        public LoanEligibilityController(
            ILoanEligibilityService eligibilityService,
            ILogger<LoanEligibilityController> logger)
        {
            _eligibilityService = eligibilityService;
            _logger = logger;
        }

        /// <summary>
        /// Check complete loan eligibility
        /// </summary>
        [HttpPost("check")]
        [ProducesResponseType(typeof(LoanEligibilityResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LoanEligibilityResultDto>> CheckEligibility(
            [FromBody] LoanEligibilityRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.MemberId))
                {
                    return BadRequest(new { message = "Member ID is required" });
                }

                if (string.IsNullOrWhiteSpace(request.LoanProductId))
                {
                    return BadRequest(new { message = "Loan Product ID is required" });
                }

                if (request.RequestedAmount <= 0)
                {
                    return BadRequest(new { message = "Requested amount must be greater than zero" });
                }

                if (request.TenureMonths <= 0)
                {
                    return BadRequest(new { message = "Tenure must be greater than zero" });
                }

                var result = await _eligibilityService.CheckEligibilityAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking eligibility");
                return StatusCode(500, new { message = "An error occurred while checking eligibility" });
            }
        }

        /// <summary>
        /// Check savings multiplier eligibility
        /// </summary>
        [HttpGet("savings-multiplier/{memberId}")]
        [ProducesResponseType(typeof(SavingsMultiplierCheckDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SavingsMultiplierCheckDto>> CheckSavingsMultiplier(
            string memberId,
            [FromQuery] decimal requestedAmount,
            [FromQuery] string loanProductId)
        {
            try
            {
                if (requestedAmount <= 0)
                {
                    return BadRequest(new { message = "Requested amount must be greater than zero" });
                }

                var result = await _eligibilityService.CheckSavingsMultiplierAsync(
                    memberId, 
                    requestedAmount, 
                    loanProductId);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking savings multiplier");
                return StatusCode(500, new { message = "An error occurred while checking savings multiplier" });
            }
        }

        /// <summary>
        /// Check membership duration eligibility
        /// </summary>
        [HttpGet("membership-duration/{memberId}")]
        [ProducesResponseType(typeof(MembershipDurationCheckDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MembershipDurationCheckDto>> CheckMembershipDuration(
            string memberId,
            [FromQuery] int minimumMonths = 6)
        {
            try
            {
                var result = await _eligibilityService.CheckMembershipDurationAsync(memberId, minimumMonths);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking membership duration");
                return StatusCode(500, new { message = "An error occurred while checking membership duration" });
            }
        }

        /// <summary>
        /// Calculate deduction rate headroom
        /// </summary>
        [HttpPost("deduction-rate-headroom")]
        [ProducesResponseType(typeof(DeductionRateHeadroomDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DeductionRateHeadroomDto>> CalculateDeductionRateHeadroom(
            [FromBody] DeductionRateHeadroomRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.MemberId))
                {
                    return BadRequest(new { message = "Member ID is required" });
                }

                if (request.RequestedAmount <= 0)
                {
                    return BadRequest(new { message = "Requested amount must be greater than zero" });
                }

                var result = await _eligibilityService.CalculateDeductionRateHeadroomAsync(
                    request.MemberId,
                    request.RequestedAmount,
                    request.TenureMonths,
                    request.InterestRate);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating deduction rate headroom");
                return StatusCode(500, new { message = "An error occurred while calculating deduction rate headroom" });
            }
        }

        /// <summary>
        /// Check debt-to-income ratio
        /// </summary>
        [HttpPost("debt-to-income-ratio")]
        [ProducesResponseType(typeof(DebtToIncomeRatioDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DebtToIncomeRatioDto>> CheckDebtToIncomeRatio(
            [FromBody] DebtToIncomeRatioRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.MemberId))
                {
                    return BadRequest(new { message = "Member ID is required" });
                }

                if (request.RequestedAmount <= 0)
                {
                    return BadRequest(new { message = "Requested amount must be greater than zero" });
                }

                var result = await _eligibilityService.CheckDebtToIncomeRatioAsync(
                    request.MemberId,
                    request.RequestedAmount,
                    request.TenureMonths,
                    request.InterestRate);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking debt-to-income ratio");
                return StatusCode(500, new { message = "An error occurred while checking debt-to-income ratio" });
            }
        }

        /// <summary>
        /// Generate comprehensive eligibility report
        /// </summary>
        [HttpGet("report/{memberId}")]
        [ProducesResponseType(typeof(EligibilityReportDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EligibilityReportDto>> GenerateEligibilityReport(
            string memberId,
            [FromQuery] string loanProductId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(loanProductId))
                {
                    return BadRequest(new { message = "Loan Product ID is required" });
                }

                var result = await _eligibilityService.GenerateEligibilityReportAsync(memberId, loanProductId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating eligibility report");
                return StatusCode(500, new { message = "An error occurred while generating eligibility report" });
            }
        }

        /// <summary>
        /// Calculate maximum eligible loan amount
        /// </summary>
        [HttpGet("maximum-amount/{memberId}")]
        [ProducesResponseType(typeof(MaximumEligibleAmountDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MaximumEligibleAmountDto>> CalculateMaximumEligibleAmount(
            string memberId,
            [FromQuery] string loanProductId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(loanProductId))
                {
                    return BadRequest(new { message = "Loan Product ID is required" });
                }

                var result = await _eligibilityService.CalculateMaximumEligibleAmountAsync(memberId, loanProductId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating maximum eligible amount");
                return StatusCode(500, new { message = "An error occurred while calculating maximum eligible amount" });
            }
        }
    }

    #region Request Models

    public class DeductionRateHeadroomRequest
    {
        public string MemberId { get; set; } = string.Empty;
        public decimal RequestedAmount { get; set; }
        public int TenureMonths { get; set; }
        public decimal InterestRate { get; set; }
    }

    public class DebtToIncomeRatioRequest
    {
        public string MemberId { get; set; } = string.Empty;
        public decimal RequestedAmount { get; set; }
        public int TenureMonths { get; set; }
        public decimal InterestRate { get; set; }
    }

    #endregion
}
