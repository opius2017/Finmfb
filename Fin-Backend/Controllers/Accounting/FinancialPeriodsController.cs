using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Accounting;
using FinTech.Core.Application.Services.Accounting;
using FinTech.Core.Domain.Entities.Accounting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinTech.Controllers.Accounting.Accounting
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FinancialPeriodsController : ControllerBase
    {
        private readonly IFinancialPeriodService _financialPeriodService;
        private readonly IPeriodClosingService _periodClosingService;
        private readonly ILogger<FinancialPeriodsController> _logger;

        public FinancialPeriodsController(
            IFinancialPeriodService financialPeriodService,
            IPeriodClosingService periodClosingService,
            ILogger<FinancialPeriodsController> logger)
        {
            _financialPeriodService = financialPeriodService;
            _periodClosingService = periodClosingService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FinancialPeriod>>> GetAllPeriods()
        {
            try
            {
                var fiscalYearId = HttpContext.Request.Query["fiscalYearId"].ToString();
                
                if (!string.IsNullOrEmpty(fiscalYearId))
                {
                    var periods = await _financialPeriodService.GetByFiscalYearAsync(fiscalYearId);
                    return Ok(periods);
                }
                else
                {
                    var periods = await _financialPeriodService.GetAllPeriodsAsync();
                    return Ok(periods);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving financial periods");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("open")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FinancialPeriod>>> GetOpenPeriods()
        {
            try
            {
                var periods = await _financialPeriodService.GetOpenPeriodsAsync();
                return Ok(periods);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving open financial periods");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("current")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FinancialPeriod>> GetCurrentPeriod()
        {
            try
            {
                var period = await _financialPeriodService.GetCurrentPeriodAsync();
                if (period == null)
                {
                    return NotFound("No current financial period found");
                }
                return Ok(period);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current financial period");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FinancialPeriod>> GetPeriodById(string id)
        {
            try
            {
                var period = await _financialPeriodService.GetByIdAsync(id);
                if (period == null)
                {
                    return NotFound($"Financial period with ID {id} not found");
                }
                return Ok(period);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving financial period {PeriodId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("byDate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FinancialPeriod>> GetPeriodByDate([FromQuery] DateTime date)
        {
            try
            {
                var period = await _financialPeriodService.GetByDateAsync(date);
                if (period == null)
                {
                    return NotFound($"No financial period found for date {date:yyyy-MM-dd}");
                }
                return Ok(period);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving financial period for date {Date}", date);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Accountant,Admin")]
        public async Task<ActionResult<string>> CreatePeriod(FinancialPeriod period)
        {
            try
            {
                period.CreatedBy = User.Identity?.Name;
                var periodId = await _financialPeriodService.CreatePeriodAsync(period);
                return CreatedAtAction(nameof(GetPeriodById), new { id = periodId }, periodId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating financial period");
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Accountant,Admin")]
        public async Task<IActionResult> UpdatePeriod(string id, FinancialPeriod period)
        {
            try
            {
                if (id != period.Id)
                {
                    return BadRequest("ID mismatch");
                }
                
                period.LastModifiedBy = User.Identity?.Name;
                await _financialPeriodService.UpdatePeriodAsync(period);
                return NoContent();
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating financial period {PeriodId}", id);
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}/open")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Accountant,Admin")]
        public async Task<IActionResult> OpenPeriod(string id)
        {
            try
            {
                await _financialPeriodService.OpenPeriodAsync(id, User.Identity?.Name);
                return NoContent();
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening financial period {PeriodId}", id);
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}/close")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Accountant,Admin")]
        public async Task<IActionResult> ClosePeriod(string id)
        {
            try
            {
                await _financialPeriodService.ClosePeriodAsync(id, User.Identity?.Name);
                return NoContent();
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing financial period {PeriodId}", id);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/month-end-closing")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Accountant,Admin")]
        public async Task<ActionResult<PeriodClosingResultDto>> RunMonthEndClosing(string id)
        {
            try
            {
                var period = await _financialPeriodService.GetByIdAsync(id);
                if (period == null)
                {
                    return NotFound($"Financial period with ID {id} not found");
                }

                var result = await _periodClosingService.ExecuteMonthEndClosingAsync(id, User.Identity?.Name);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running month-end closing for period {PeriodId}", id);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/quarter-end-closing")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Accountant,Admin")]
        public async Task<ActionResult<PeriodClosingResultDto>> RunQuarterEndClosing(string id)
        {
            try
            {
                var period = await _financialPeriodService.GetByIdAsync(id);
                if (period == null)
                {
                    return NotFound($"Financial period with ID {id} not found");
                }

                var result = await _periodClosingService.ExecuteQuarterEndClosingAsync(id, User.Identity?.Name);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running quarter-end closing for period {PeriodId}", id);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/year-end-closing")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Accountant,Admin")]
        public async Task<ActionResult<PeriodClosingResultDto>> RunYearEndClosing(string id)
        {
            try
            {
                var period = await _financialPeriodService.GetByIdAsync(id);
                if (period == null)
                {
                    return NotFound($"Financial period with ID {id} not found");
                }

                var result = await _periodClosingService.ExecuteYearEndClosingAsync(id, User.Identity?.Name);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running year-end closing for period {PeriodId}", id);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}/closing-status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PeriodClosingStatusDto>> GetClosingStatus(string id)
        {
            try
            {
                var period = await _financialPeriodService.GetByIdAsync(id);
                if (period == null)
                {
                    return NotFound($"Financial period with ID {id} not found");
                }

                var status = await _periodClosingService.GetPeriodClosingStatusAsync(id);
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving closing status for period {PeriodId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}/prereq-check")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PeriodClosingPrerequisiteCheckDto>> CheckClosingPrerequisites(string id)
        {
            try
            {
                var period = await _financialPeriodService.GetByIdAsync(id);
                if (period == null)
                {
                    return NotFound($"Financial period with ID {id} not found");
                }

                var checkResult = await _periodClosingService.CheckClosingPrerequisitesAsync(id);
                return Ok(checkResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking closing prerequisites for period {PeriodId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}

