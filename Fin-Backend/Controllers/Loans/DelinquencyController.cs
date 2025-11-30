using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Loans;
using FinTech.Core.Application.Services.Loans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FinTech.Controllers.Loans
{
    [ApiController]
    [Route("api/delinquency")]
    [Authorize]
    public class DelinquencyController : ControllerBase
    {
        private readonly IDelinquencyManagementService _delinquencyService;
        private readonly ILogger<DelinquencyController> _logger;

        public DelinquencyController(
            IDelinquencyManagementService delinquencyService,
            ILogger<DelinquencyController> logger)
        {
            _delinquencyService = delinquencyService;
            _logger = logger;
        }

        /// <summary>
        /// Check delinquency status for a specific loan
        /// </summary>
        [HttpGet("loan/{loanId}")]
        [ProducesResponseType(typeof(DelinquencyCheckResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DelinquencyCheckResult>> CheckLoan(string loanId)
        {
            try
            {
                var result = await _delinquencyService.CheckLoanDelinquencyAsync(loanId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking delinquency for loan {LoanId}", loanId);
                return StatusCode(500, new { message = "An error occurred while checking delinquency" });
            }
        }

        /// <summary>
        /// Perform daily delinquency check for all active loans
        /// </summary>
        [HttpPost("check-daily")]
        [Authorize(Roles = "Admin,System")]
        [ProducesResponseType(typeof(DailyDelinquencyCheckResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<DailyDelinquencyCheckResult>> PerformDailyCheck()
        {
            try
            {
                var result = await _delinquencyService.PerformDailyDelinquencyCheckAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing daily delinquency check");
                return StatusCode(500, new { message = "An error occurred during daily check" });
            }
        }

        /// <summary>
        /// Get delinquent loans with optional filtering
        /// </summary>
        [HttpGet("delinquent-loans")]
        [ProducesResponseType(typeof(List<LoanDelinquencyDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<LoanDelinquencyDto>>> GetDelinquentLoans(
            [FromQuery] DelinquencyReportRequest request)
        {
            try
            {
                var result = await _delinquencyService.GetDelinquentLoansAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting delinquent loans");
                return StatusCode(500, new { message = "An error occurred while retrieving delinquent loans" });
            }
        }

        /// <summary>
        /// Get delinquency summary statistics
        /// </summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(DelinquencySummaryDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<DelinquencySummaryDto>> GetSummary()
        {
            try
            {
                var result = await _delinquencyService.GetDelinquencySummaryAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting delinquency summary");
                return StatusCode(500, new { message = "An error occurred while retrieving summary" });
            }
        }

        /// <summary>
        /// Apply penalty to an overdue loan
        /// </summary>
        [HttpPost("loan/{loanId}/penalty")]
        [Authorize(Roles = "Admin,LoanOfficer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<bool>> ApplyPenalty(string loanId, [FromBody] decimal amount)
        {
            try
            {
                var userName = User.Identity?.Name ?? "Unknown";
                var result = await _delinquencyService.ApplyPenaltyAsync(loanId, amount, userName);
                
                if (!result)
                    return NotFound(new { message = "Loan not found" });

                return Ok(new { message = $"Penalty of ₦{amount:N2} applied successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying penalty to loan {LoanId}", loanId);
                return StatusCode(500, new { message = "An error occurred while applying penalty" });
            }
        }

        /// <summary>
        /// Send delinquency notification to member
        /// </summary>
        [HttpPost("loan/{loanId}/notify")]
        [Authorize(Roles = "Admin,LoanOfficer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> SendNotification(string loanId, [FromBody] string notificationType)
        {
            try
            {
                var result = await _delinquencyService.SendDelinquencyNotificationAsync(loanId, notificationType);
                return Ok(new { message = "Notification sent successfully", sent = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification for loan {LoanId}", loanId);
                return StatusCode(500, new { message = "An error occurred while sending notification" });
            }
        }

        /// <summary>
        /// Get delinquency history for a loan
        /// </summary>
        [HttpGet("loan/{loanId}/history")]
        [ProducesResponseType(typeof(List<LoanDelinquencyDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<LoanDelinquencyDto>>> GetHistory(string loanId)
        {
            try
            {
                var result = await _delinquencyService.GetLoanDelinquencyHistoryAsync(loanId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting delinquency history for loan {LoanId}", loanId);
                return StatusCode(500, new { message = "An error occurred while retrieving history" });
            }
        }

        /// <summary>
        /// Get overdue loans with minimum days filter
        /// </summary>
        [HttpGet("overdue")]
        [ProducesResponseType(typeof(List<LoanDelinquencyDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<LoanDelinquencyDto>>> GetOverdueLoans([FromQuery] int minDays = 1)
        {
            try
            {
                var result = await _delinquencyService.IdentifyOverdueLoansAsync(minDays);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting overdue loans");
                return StatusCode(500, new { message = "An error occurred while retrieving overdue loans" });
            }
        }

        /// <summary>
        /// Calculate current delinquency rate
        /// </summary>
        [HttpGet("rate")]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        public async Task<ActionResult<decimal>> GetDelinquencyRate()
        {
            try
            {
                var result = await _delinquencyService.CalculateDelinquencyRateAsync();
                return Ok(new { delinquencyRate = result, percentage = $"{result:F2}%" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating delinquency rate");
                return StatusCode(500, new { message = "An error occurred while calculating rate" });
            }
        }
    }
}
