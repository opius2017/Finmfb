using System;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Loans;
using FinTech.Core.Application.Interfaces.Loans;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FinTech.Controllers.Loans
{
    [ApiController]
    [Route("api/loans/[controller]")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminController : ControllerBase
    {
        private readonly IMonthlyThresholdService _thresholdService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IMonthlyThresholdService thresholdService,
            ILogger<AdminController> logger)
        {
            _thresholdService = thresholdService;
            _logger = logger;
        }

        /// <summary>
        /// Sets monthly loan threshold
        /// </summary>
        [HttpPost("set-threshold")]
        [ProducesResponseType(typeof(MonthlyThresholdResult), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> SetMonthlyThreshold([FromBody] SetThresholdRequest request)
        {
            try
            {
                var result = await _thresholdService.SetMonthlyThresholdAsync(
                    request.Month,
                    request.Year,
                    request.MaxLoanAmount,
                    User.Identity.Name);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting monthly threshold");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Performs monthly rollover (scheduled job endpoint)
        /// </summary>
        [HttpPost("monthly-rollover")]
        [ProducesResponseType(typeof(MonthlyRolloverResult), 200)]
        public async Task<IActionResult> PerformMonthlyRollover()
        {
            try
            {
                var result = await _thresholdService.PerformMonthlyRolloverAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing monthly rollover");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets threshold breach alerts
        /// </summary>
        [HttpGet("threshold-alerts")]
        [ProducesResponseType(typeof(List<ThresholdAlert>), 200)]
        public async Task<IActionResult> GetThresholdAlerts()
        {
            try
            {
                var alerts = await _thresholdService.GetThresholdAlertsAsync();
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting threshold alerts");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Releases threshold allocation (when loan is rejected)
        /// </summary>
        [HttpPost("release-threshold-allocation")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> ReleaseThresholdAllocation([FromBody] ReleaseAllocationRequest request)
        {
            try
            {
                await _thresholdService.ReleaseThresholdAllocationAsync(
                    request.LoanAmount,
                    request.Month,
                    request.Year);

                return Ok(new { message = "Threshold allocation released successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing threshold allocation");
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    // Request DTOs
    public class SetThresholdRequest
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal MaxLoanAmount { get; set; }
    }

    public class ReleaseAllocationRequest
    {
        public decimal LoanAmount { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
}
