using System;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Loans;
using FinTech.Core.Application.Services.Loans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FinTech.Controllers.Loans
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LoanApplicationController : ControllerBase
    {
        private readonly ILoanEligibilityService _eligibilityService;
        private readonly ILoanCalculatorService _calculatorService;
        private readonly ILogger<LoanApplicationController> _logger;

        public LoanApplicationController(
            ILoanEligibilityService eligibilityService,
            ILoanCalculatorService calculatorService,
            ILogger<LoanApplicationController> logger)
        {
            _eligibilityService = eligibilityService;
            _calculatorService = calculatorService;
            _logger = logger;
        }

        /// <summary>
        /// Checks loan eligibility for a member
        /// </summary>
        [HttpPost("check-eligibility")]
        [ProducesResponseType(typeof(EligibilityCheckResult), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CheckEligibility([FromBody] EligibilityCheckRequest request)
        {
            try
            {
                var result = await _eligibilityService.CheckEligibilityAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking eligibility");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Calculates loan EMI and generates amortization schedule
        /// </summary>
        [HttpPost("calculate")]
        [ProducesResponseType(typeof(LoanCalculationSummary), 200)]
        [ProducesResponseType(400)]
        public IActionResult CalculateLoan([FromBody] LoanCalculationRequest request)
        {
            try
            {
                var summary = _calculatorService.CalculateLoanSummary(
                    request.Principal,
                    request.InterestRate,
                    request.TenorMonths,
                    request.StartDate ?? DateTime.UtcNow);

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating loan");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Calculates deduction rate impact
        /// </summary>
        [HttpPost("calculate-deduction-impact")]
        [ProducesResponseType(typeof(DeductionRateImpact), 200)]
        public IActionResult CalculateDeductionImpact([FromBody] DeductionImpactRequest request)
        {
            try
            {
                var emi = _calculatorService.CalculateEMI(
                    request.LoanAmount,
                    request.InterestRate,
                    request.TenorMonths);

                var impact = _calculatorService.CalculateDeductionRateImpact(
                    request.NetSalary,
                    request.ExistingDeductions,
                    emi,
                    request.MaxDeductionRate);

                return Ok(impact);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating deduction impact");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Generates eligibility report
        /// </summary>
        [HttpPost("eligibility-report")]
        [ProducesResponseType(typeof(EligibilityReport), 200)]
        public async Task<IActionResult> GenerateEligibilityReport([FromBody] EligibilityCheckRequest request)
        {
            try
            {
                var result = await _eligibilityService.CheckEligibilityAsync(request);
                var report = _eligibilityService.GenerateEligibilityReport(result);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating eligibility report");
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    // Request DTOs
    public class LoanCalculationRequest
    {
        public decimal Principal { get; set; }
        public decimal InterestRate { get; set; }
        public int TenorMonths { get; set; }
        public DateTime? StartDate { get; set; }
    }

    public class DeductionImpactRequest
    {
        public decimal LoanAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int TenorMonths { get; set; }
        public decimal NetSalary { get; set; }
        public decimal ExistingDeductions { get; set; }
        public decimal MaxDeductionRate { get; set; } = 45m;
    }
}
