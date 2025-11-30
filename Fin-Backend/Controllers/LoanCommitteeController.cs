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
    [Authorize(Roles = "LoanCommittee,Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class LoanCommitteeController : ControllerBase
    {
        private readonly ICommitteeReviewService _committeeService;
        private readonly ILogger<LoanCommitteeController> _logger;
        
        public LoanCommitteeController(
            ICommitteeReviewService committeeService,
            ILogger<LoanCommitteeController> logger)
        {
            _committeeService = committeeService ?? throw new ArgumentNullException(nameof(committeeService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Get member credit profile for review
        /// </summary>
        /// <param name="memberId">Member ID</param>
        /// <returns>Comprehensive credit profile</returns>
        [HttpGet("credit-profile/{memberId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MemberCreditProfile>> GetCreditProfile(Guid memberId)
        {
            try
            {
                var profile = await _committeeService.GetMemberCreditProfileAsync(memberId);
                return Ok(profile);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }
        
        /// <summary>
        /// Calculate member repayment score
        /// </summary>
        /// <param name="memberId">Member ID</param>
        /// <returns>Repayment score details</returns>
        [HttpGet("repayment-score/{memberId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RepaymentScoreResult>> GetRepaymentScore(Guid memberId)
        {
            try
            {
                var score = await _committeeService.CalculateRepaymentScoreAsync(memberId);
                return Ok(score);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }
        
        /// <summary>
        /// Submit committee review for an application
        /// </summary>
        /// <param name="command">Review details</param>
        /// <returns>Created review</returns>
        [HttpPost("review")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> SubmitReview([FromBody] SubmitReviewCommand command)
        {
            try
            {
                var review = await _committeeService.SubmitReviewAsync(command);
                return CreatedAtAction(
                    nameof(GetApplicationReviews),
                    new { applicationId = command.ApplicationId },
                    review);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        /// <summary>
        /// Get all reviews for an application
        /// </summary>
        /// <param name="applicationId">Application ID</param>
        /// <returns>List of reviews</returns>
        [HttpGet("reviews/{applicationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetApplicationReviews(Guid applicationId)
        {
            var reviews = await _committeeService.GetApplicationReviewsAsync(applicationId);
            return Ok(reviews);
        }
        
        /// <summary>
        /// Get applications pending committee review
        /// </summary>
        /// <returns>List of pending applications</returns>
        [HttpGet("pending-applications")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetPendingApplications()
        {
            var applications = await _committeeService.GetPendingReviewApplicationsAsync();
            return Ok(applications);
        }
        
        /// <summary>
        /// Check if application has sufficient approvals
        /// </summary>
        /// <param name="applicationId">Application ID</param>
        /// <param name="requiredApprovals">Number of required approvals (default: 2)</param>
        /// <returns>Approval status</returns>
        [HttpGet("approval-status/{applicationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> CheckApprovalStatus(
            Guid applicationId,
            [FromQuery] int requiredApprovals = 2)
        {
            var hasSufficientApprovals = await _committeeService.HasSufficientApprovalsAsync(
                applicationId, 
                requiredApprovals);
            
            return Ok(new
            {
                applicationId,
                hasSufficientApprovals,
                requiredApprovals
            });
        }
    }
}
