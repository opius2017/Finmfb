using System;
using System.Threading.Tasks;
using FinTech.Application.Services.Accounting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTech.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PeriodClosingController : ControllerBase
    {
        private readonly IPeriodClosingService _periodClosingService;

        public PeriodClosingController(IPeriodClosingService periodClosingService)
        {
            _periodClosingService = periodClosingService ?? throw new ArgumentNullException(nameof(periodClosingService));
        }

        /// <summary>
        /// Gets the current status of the period closing process for a financial period
        /// </summary>
        /// <param name="periodId">The ID of the financial period</param>
        /// <returns>The period closing status</returns>
        [HttpGet("status/{periodId}")]
        public async Task<IActionResult> GetPeriodClosingStatus(string periodId)
        {
            try
            {
                var result = await _periodClosingService.GetPeriodClosingStatusAsync(periodId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Initiates the period closing process for a financial period
        /// </summary>
        /// <param name="periodId">The ID of the financial period</param>
        /// <returns>The period closing status</returns>
        [HttpPost("initiate/{periodId}")]
        public async Task<IActionResult> InitiatePeriodClosing(string periodId)
        {
            try
            {
                var result = await _periodClosingService.InitiatePeriodClosingAsync(periodId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Validates the period closing process for a financial period
        /// </summary>
        /// <param name="periodId">The ID of the financial period</param>
        /// <returns>The period closing status</returns>
        [HttpPost("validate/{periodId}")]
        public async Task<IActionResult> ValidatePeriodClosing(string periodId)
        {
            try
            {
                var result = await _periodClosingService.ValidatePeriodClosingAsync(periodId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Posts the closing entries for a financial period
        /// </summary>
        /// <param name="periodId">The ID of the financial period</param>
        /// <returns>The period closing status</returns>
        [HttpPost("post-closing-entries/{periodId}")]
        public async Task<IActionResult> PostClosingEntries(string periodId)
        {
            try
            {
                var result = await _periodClosingService.PostClosingEntriesAsync(periodId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Completes the period closing process
        /// </summary>
        /// <param name="periodId">The ID of the financial period</param>
        /// <returns>The period closing status</returns>
        [HttpPost("complete/{periodId}")]
        public async Task<IActionResult> CompletePeriodClosing(string periodId)
        {
            try
            {
                var result = await _periodClosingService.CompletePeriodClosingAsync(periodId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Rolls back the period closing process
        /// </summary>
        /// <param name="periodId">The ID of the financial period</param>
        /// <param name="reason">The reason for rolling back</param>
        /// <returns>The period closing status</returns>
        [HttpPost("rollback/{periodId}")]
        public async Task<IActionResult> RollBackPeriodClosing(string periodId, [FromBody] RollbackRequest request)
        {
            try
            {
                var result = await _periodClosingService.RollBackPeriodClosingAsync(periodId, request.Reason);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class RollbackRequest
    {
        public string Reason { get; set; }
    }
}