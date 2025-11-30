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
    [Route("api/committee")]
    [Authorize(Roles = "Committee,Admin")]
    public class CommitteeController : ControllerBase
    {
        private readonly ILoanCommitteeService _committeeService;
        private readonly ILogger<CommitteeController> _logger;

        public CommitteeController(
            ILoanCommitteeService committeeService,
            ILogger<CommitteeController> logger)
        {
            _committeeService = committeeService;
            _logger = logger;
        }

        [HttpPost("reviews")]
        [ProducesResponseType(typeof(CommitteeReviewDto), StatusCodes.Status201Created)]
        public async Task<ActionResult<CommitteeReviewDto>> CreateReview([FromBody] CreateCommitteeReviewRequest request)
        {
            try
            {
                var result = await _committeeService.CreateReviewAsync(request);
                return CreatedAtAction(nameof(GetReview), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpGet("reviews/pending")]
        [ProducesResponseType(typeof(List<CommitteeReviewDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CommitteeReviewDto>>> GetPendingReviews()
        {
            try
            {
                var result = await _committeeService.GetPendingReviewsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending reviews");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpGet("reviews/{id}")]
        [ProducesResponseType(typeof(CommitteeReviewDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommitteeReviewDto>> GetReview(string id)
        {
            try
            {
                var result = await _committeeService.GetReviewByIdAsync(id);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting review");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpPost("reviews/{id}/decision")]
        [ProducesResponseType(typeof(CommitteeReviewDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommitteeReviewDto>> SubmitDecision(
            string id,
            [FromBody] SubmitReviewDecisionRequest request)
        {
            try
            {
                request.ReviewId = id;
                var result = await _committeeService.SubmitReviewDecisionAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting decision");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpGet("credit-profile/{memberId}")]
        [ProducesResponseType(typeof(MemberCreditProfileDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<MemberCreditProfileDto>> GetCreditProfile(string memberId)
        {
            try
            {
                var result = await _committeeService.GetMemberCreditProfileAsync(memberId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting credit profile");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpGet("repayment-score/{memberId}")]
        [ProducesResponseType(typeof(RepaymentScoreDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<RepaymentScoreDto>> GetRepaymentScore(string memberId)
        {
            try
            {
                var result = await _committeeService.CalculateRepaymentScoreAsync(memberId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating repayment score");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(CommitteeDashboardDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<CommitteeDashboardDto>> GetDashboard()
        {
            try
            {
                var result = await _committeeService.GetCommitteeDashboardAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }
    }
}
