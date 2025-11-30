using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FinTech.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ThresholdController : ControllerBase
    {
        private readonly IThresholdManager _thresholdManager;
        private readonly ILogger<ThresholdController> _logger;
        
        public ThresholdController(
            IThresholdManager thresholdManager,
            ILogger<ThresholdController> logger)
        {
            _thresholdManager = thresholdManager ?? throw new ArgumentNullException(nameof(thresholdManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Get threshold for a specific month
        /// </summary>
        /// <param name="year">Year</param>
        /// <param name="month">Month (1-12)</param>
        /// <returns>Monthly threshold</returns>
        [HttpGet("{year}/{month}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> GetThreshold(int year, int month)
        {
            if (month < 1 || month > 12)
                return BadRequest("Month must be between 1 and 12");
            
            var threshold = await _thresholdManager.GetOrCreateThresholdAsync(year, month);
            return Ok(threshold);
        }
        
        /// <summary>
        /// Check if amount can be allocated within threshold
        /// </summary>
        /// <param name="year">Year</param>
        /// <param name="month">Month (1-12)</param>
        /// <param name="amount">Amount to check</param>
        /// <returns>Threshold check result</returns>
        [HttpGet("{year}/{month}/check")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ThresholdCheckResult>> CheckThreshold(
            int year,
            int month,
            [FromQuery] decimal amount)
        {
            if (month < 1 || month > 12)
                return BadRequest("Month must be between 1 and 12");
            
            if (amount <= 0)
                return BadRequest("Amount must be greater than zero");
            
            var result = await _thresholdManager.CheckThresholdAsync(amount, year, month);
            return Ok(result);
        }
        
        /// <summary>
        /// Update threshold maximum amount (Super Admin only)
        /// </summary>
        /// <param name="year">Year</param>
        /// <param name="month">Month (1-12)</param>
        /// <param name="request">Update request</param>
        /// <returns>Updated threshold</returns>
        [HttpPut("{year}/{month}")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UpdateThreshold(
            int year,
            int month,
            [FromBody] UpdateThresholdRequest request)
        {
            if (month < 1 || month > 12)
                return BadRequest("Month must be between 1 and 12");
            
            if (request.MaximumAmount <= 0)
                return BadRequest("Maximum amount must be greater than zero");
            
            if (request.MaximumAmount > 3000000)
                return BadRequest("Maximum threshold limit is ₦3,000,000");
            
            try
            {
                var threshold = await _thresholdManager.UpdateThresholdAsync(year, month, request.MaximumAmount);
                return Ok(threshold);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        /// <summary>
        /// Get queued applications for a month
        /// </summary>
        /// <param name="year">Year</param>
        /// <param name="month">Month (1-12)</param>
        /// <returns>List of queued applications</returns>
        [HttpGet("{year}/{month}/queued")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<QueuedLoanApplication>>> GetQueuedApplications(int year, int month)
        {
            if (month < 1 || month > 12)
                return BadRequest("Month must be between 1 and 12");
            
            var queuedApps = await _thresholdManager.GetQueuedApplicationsAsync(year, month);
            return Ok(queuedApps);
        }
        
        /// <summary>
        /// Get threshold history for a year
        /// </summary>
        /// <param name="year">Year</param>
        /// <returns>List of monthly thresholds</returns>
        [HttpGet("history/{year}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetThresholdHistory(int year)
        {
            var history = await _thresholdManager.GetThresholdHistoryAsync(year);
            return Ok(history);
        }
        
        /// <summary>
        /// Get threshold utilization report
        /// </summary>
        /// <param name="year">Year</param>
        /// <param name="month">Optional month (1-12)</param>
        /// <returns>Utilization report</returns>
        [HttpGet("utilization/{year}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ThresholdUtilizationReport>> GetUtilizationReport(
            int year,
            [FromQuery] int? month = null)
        {
            if (month.HasValue && (month.Value < 1 || month.Value > 12))
                return BadRequest("Month must be between 1 and 12");
            
            var report = await _thresholdManager.GetUtilizationReportAsync(year, month);
            return Ok(report);
        }
        
        /// <summary>
        /// Manually trigger monthly rollover (Admin only)
        /// </summary>
        /// <returns>Rollover result</returns>
        [HttpPost("rollover")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> TriggerRollover()
        {
            await _thresholdManager.ProcessMonthlyRolloverAsync();
            return Ok(new { message = "Monthly rollover completed successfully" });
        }
    }
    
    public class UpdateThresholdRequest
    {
        public decimal MaximumAmount { get; set; }
    }
}
