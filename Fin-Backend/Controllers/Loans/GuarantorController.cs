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
    [Route("api/guarantors")]
    [Authorize]
    public class GuarantorController : ControllerBase
    {
        private readonly IGuarantorService _guarantorService;
        private readonly ILogger<GuarantorController> _logger;

        public GuarantorController(
            IGuarantorService guarantorService,
            ILogger<GuarantorController> logger)
        {
            _guarantorService = guarantorService;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(GuarantorDto), StatusCodes.Status201Created)]
        public async Task<ActionResult<GuarantorDto>> AddGuarantor([FromBody] AddGuarantorRequest request)
        {
            try
            {
                var result = await _guarantorService.AddGuarantorAsync(request);
                return CreatedAtAction(nameof(GetLoanGuarantors), new { loanApplicationId = request.LoanApplicationId }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding guarantor");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpGet("eligibility/{memberId}")]
        [ProducesResponseType(typeof(GuarantorEligibilityDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<GuarantorEligibilityDto>> CheckEligibility(
            string memberId,
            [FromQuery] decimal guaranteeAmount)
        {
            try
            {
                var result = await _guarantorService.CheckGuarantorEligibilityAsync(memberId, guaranteeAmount);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking eligibility");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpPost("{guarantorId}/consent")]
        [ProducesResponseType(typeof(GuarantorDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<GuarantorDto>> ProcessConsent(
            string guarantorId,
            [FromBody] ProcessConsentRequest request)
        {
            try
            {
                request.GuarantorId = guarantorId;
                var result = await _guarantorService.ProcessConsentAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing consent");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpGet("loan/{loanApplicationId}")]
        [ProducesResponseType(typeof(List<GuarantorDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<GuarantorDto>>> GetLoanGuarantors(string loanApplicationId)
        {
            try
            {
                var result = await _guarantorService.GetLoanGuarantorsAsync(loanApplicationId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting loan guarantors");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpGet("dashboard/{memberId}")]
        [ProducesResponseType(typeof(GuarantorDashboardDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<GuarantorDashboardDto>> GetDashboard(string memberId)
        {
            try
            {
                var result = await _guarantorService.GetGuarantorDashboardAsync(memberId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpGet("member/{memberId}/guaranteed-loans")]
        [ProducesResponseType(typeof(List<GuaranteedLoanDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<GuaranteedLoanDto>>> GetGuaranteedLoans(string memberId)
        {
            try
            {
                var result = await _guarantorService.GetGuaranteedLoansAsync(memberId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting guaranteed loans");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpDelete("{guarantorId}")]
        [Authorize(Roles = "Admin,LoanOfficer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RemoveGuarantor(string guarantorId)
        {
            try
            {
                var result = await _guarantorService.RemoveGuarantorAsync(guarantorId);
                if (!result)
                {
                    return NotFound(new { message = "Guarantor not found" });
                }
                return Ok(new { message = "Guarantor removed successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing guarantor");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }
    }
}
