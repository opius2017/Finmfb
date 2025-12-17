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
    [Route("api/[controller]")]
    [Authorize]
    public class RepaymentController : ControllerBase
    {
        private readonly ILoanRepaymentService _repaymentService;
        private readonly ILogger<RepaymentController> _logger;

        public RepaymentController(
            ILoanRepaymentService repaymentService,
            ILogger<RepaymentController> logger)
        {
            _repaymentService = repaymentService;
            _logger = logger;
        }

        /// <summary>
        /// Processes a loan repayment
        /// </summary>
        [HttpPost("process-payment")]
        [ProducesResponseType(typeof(RepaymentResult), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ProcessRepayment([FromBody] RepaymentRequest request)
        {
            try
            {
                request.ProcessedBy = User.Identity.Name;
                var result = await _repaymentService.ProcessRepaymentAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing repayment");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Processes a partial payment
        /// </summary>
        [HttpPost("process-partial-payment")]
        [ProducesResponseType(typeof(RepaymentResult), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ProcessPartialPayment([FromBody] RepaymentRequest request)
        {
            try
            {
                request.ProcessedBy = User.Identity.Name;
                var result = await _repaymentService.ProcessPartialPaymentAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing partial payment");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets repayment history for a loan
        /// </summary>
        [HttpGet("history/{loanId}")]
        [ProducesResponseType(typeof(RepaymentHistory), 200)]
        public async Task<IActionResult> GetRepaymentHistory(string loanId)
        {
            try
            {
                var history = await _repaymentService.GetLoanRepaymentHistoryAsync(loanId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting repayment history");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets repayment schedule for a loan
        /// </summary>
        [HttpGet("schedule/{loanId}")]
        [ProducesResponseType(typeof(List<RepaymentScheduleItem>), 200)]
        public async Task<IActionResult> GetRepaymentSchedule(string loanId)
        {
            try
            {
                var schedule = await _repaymentService.GetRepaymentScheduleAsync(loanId);
                return Ok(schedule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting repayment schedule");
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
