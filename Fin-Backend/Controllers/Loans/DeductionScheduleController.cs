using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Loans;
using FinTech.Core.Application.Interfaces.Loans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FinTech.Controllers.Loans
{
    [ApiController]
    [Route("api/deduction-schedule")]
    [Authorize]
    public class DeductionScheduleController : ControllerBase
    {
        private readonly IDeductionScheduleService _scheduleService;
        private readonly ILogger<DeductionScheduleController> _logger;

        public DeductionScheduleController(
            IDeductionScheduleService scheduleService,
            ILogger<DeductionScheduleController> logger)
        {
            _scheduleService = scheduleService;
            _logger = logger;
        }

        /// <summary>
        /// Generate a new deduction schedule for a specific month
        /// </summary>
        [HttpPost("generate")]
        [Authorize(Roles = "Admin,PayrollManager")]
        [ProducesResponseType(typeof(DeductionScheduleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DeductionScheduleDto>> Generate([FromBody] GenerateDeductionScheduleRequest request)
        {
            try
            {
                var result = await _scheduleService.GenerateScheduleAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating deduction schedule");
                return StatusCode(500, new { message = "An error occurred while generating the schedule" });
            }
        }

        /// <summary>
        /// Get deduction schedule by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DeductionScheduleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DeductionScheduleDto>> GetById(string id)
        {
            var result = await _scheduleService.GetScheduleByIdAsync(id);
            
            if (result == null)
                return NotFound(new { message = "Schedule not found" });

            return Ok(result);
        }

        /// <summary>
        /// Get deduction schedule by month and year
        /// </summary>
        [HttpGet("month/{year}/{month}")]
        [ProducesResponseType(typeof(DeductionScheduleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DeductionScheduleDto>> GetByMonth(int year, int month)
        {
            if (month < 1 || month > 12)
                return BadRequest(new { message = "Invalid month. Must be between 1 and 12" });

            var result = await _scheduleService.GetScheduleByMonthAsync(month, year);
            
            if (result == null)
                return NotFound(new { message = $"No schedule found for {month}/{year}" });

            return Ok(result);
        }

        /// <summary>
        /// Get all deduction schedules with optional filtering
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<DeductionScheduleDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<DeductionScheduleDto>>> GetAll(
            [FromQuery] int? year = null,
            [FromQuery] string? status = null)
        {
            var result = await _scheduleService.GetSchedulesAsync(year, status);
            return Ok(result);
        }

        /// <summary>
        /// Approve a deduction schedule
        /// </summary>
        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Admin,PayrollManager")]
        [ProducesResponseType(typeof(DeductionScheduleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DeductionScheduleDto>> Approve(
            string id,
            [FromBody] ApproveDeductionScheduleRequest request)
        {
            try
            {
                request.ScheduleId = id;
                var result = await _scheduleService.ApproveScheduleAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving schedule {ScheduleId}", id);
                return StatusCode(500, new { message = "An error occurred while approving the schedule" });
            }
        }

        /// <summary>
        /// Submit schedule to payroll system
        /// </summary>
        [HttpPost("{id}/submit")]
        [Authorize(Roles = "Admin,PayrollManager")]
        [ProducesResponseType(typeof(DeductionScheduleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DeductionScheduleDto>> Submit(
            string id,
            [FromBody] SubmitDeductionScheduleRequest request)
        {
            try
            {
                request.ScheduleId = id;
                var result = await _scheduleService.SubmitScheduleAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting schedule {ScheduleId}", id);
                return StatusCode(500, new { message = "An error occurred while submitting the schedule" });
            }
        }

        /// <summary>
        /// Export schedule to Excel/CSV/PDF
        /// </summary>
        [HttpGet("{id}/export")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Export(string id, [FromQuery] string format = "EXCEL")
        {
            try
            {
                var request = new ExportDeductionScheduleRequest
                {
                    ScheduleId = id,
                    Format = format.ToUpper()
                };

                var result = await _scheduleService.ExportScheduleAsync(request);

                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                if (result.FileContent != null)
                {
                    return File(result.FileContent, result.ContentType!, result.FileName);
                }

                return Ok(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting schedule {ScheduleId}", id);
                return StatusCode(500, new { message = "An error occurred while exporting the schedule" });
            }
        }

        /// <summary>
        /// Cancel a deduction schedule
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<bool>> Cancel(string id)
        {
            try
            {
                var userName = User.Identity?.Name ?? "Unknown";
                var result = await _scheduleService.CancelScheduleAsync(id, userName);
                
                if (!result)
                    return NotFound(new { message = "Schedule not found" });

                return Ok(new { message = "Schedule cancelled successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling schedule {ScheduleId}", id);
                return StatusCode(500, new { message = "An error occurred while cancelling the schedule" });
            }
        }

        /// <summary>
        /// Create a new version of an existing schedule
        /// </summary>
        [HttpPost("{id}/version")]
        [Authorize(Roles = "Admin,PayrollManager")]
        [ProducesResponseType(typeof(DeductionScheduleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DeductionScheduleDto>> CreateNewVersion(string id)
        {
            try
            {
                var userName = User.Identity?.Name ?? "Unknown";
                var result = await _scheduleService.CreateNewVersionAsync(id, userName);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new version for schedule {ScheduleId}", id);
                return StatusCode(500, new { message = "An error occurred while creating new version" });
            }
        }
    }
}
