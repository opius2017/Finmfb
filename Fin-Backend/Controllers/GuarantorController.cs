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
    public class GuarantorController : ControllerBase
    {
        private readonly IGuarantorService _guarantorService;
        private readonly ILogger<GuarantorController> _logger;
        
        public GuarantorController(
            IGuarantorService guarantorService,
            ILogger<GuarantorController> logger)
        {
            _guarantorService = guarantorService ?? throw new ArgumentNullException(nameof(guarantorService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Check if a member is eligible to be a guarantor
        /// </summary>
        /// <param name="memberId">Member ID</param>
        /// <param name="guaranteedAmount">Amount to guarantee</param>
        /// <returns>Eligibility result</returns>
        [HttpGet("eligibility/{memberId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GuarantorEligibilityResult>> CheckEligibility(
            Guid memberId,
            [FromQuery] decimal guaranteedAmount)
        {
            try
            {
                var result = await _guarantorService.ValidateGuarantorEligibilityAsync(memberId, guaranteedAmount);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }
        
        /// <summary>
        /// Request consent from a guarantor
        /// </summary>
        /// <param name="request">Consent request details</param>
        /// <returns>Created consent request</returns>
        [HttpPost("consent/request")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> RequestConsent([FromBody] GuarantorConsentRequest request)
        {
            try
            {
                var consent = await _guarantorService.RequestConsentAsync(request);
                return CreatedAtAction(
                    nameof(GetConsentByToken),
                    new { token = consent.ConsentToken },
                    consent);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        /// <summary>
        /// Get consent request by token
        /// </summary>
        /// <param name="token">Consent token</param>
        /// <returns>Consent details</returns>
        [HttpGet("consent/{token}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetConsentByToken(string token)
        {
            try
            {
                // This would need to be implemented in the service
                return Ok(new { message = "Consent retrieval not yet implemented" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }
        
        /// <summary>
        /// Approve a guarantor consent request
        /// </summary>
        /// <param name="token">Consent token</param>
        /// <param name="request">Approval details</param>
        /// <returns>Updated consent</returns>
        [HttpPost("consent/{token}/approve")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ApproveConsent(
            string token,
            [FromBody] ApproveConsentRequest request)
        {
            try
            {
                var consent = await _guarantorService.ApproveConsentAsync(token, request?.Notes);
                return Ok(consent);
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
        /// Decline a guarantor consent request
        /// </summary>
        /// <param name="token">Consent token</param>
        /// <param name="request">Decline details</param>
        /// <returns>Updated consent</returns>
        [HttpPost("consent/{token}/decline")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeclineConsent(
            string token,
            [FromBody] DeclineConsentRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Reason))
                    return BadRequest("Decline reason is required");
                
                var consent = await _guarantorService.DeclineConsentAsync(token, request.Reason);
                return Ok(consent);
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
        /// Get all guarantor obligations for a member
        /// </summary>
        /// <param name="memberId">Member ID</param>
        /// <returns>List of guarantor obligations</returns>
        [HttpGet("obligations/{memberId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<GuarantorObligation>>> GetObligations(Guid memberId)
        {
            try
            {
                var obligations = await _guarantorService.GetGuarantorObligationsAsync(memberId);
                return Ok(obligations);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }
        
        /// <summary>
        /// Get pending consent requests for a guarantor
        /// </summary>
        /// <param name="memberId">Guarantor member ID</param>
        /// <returns>List of pending consent requests</returns>
        [HttpGet("consent/pending/{memberId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetPendingConsents(Guid memberId)
        {
            var consents = await _guarantorService.GetPendingConsentRequestsAsync(memberId);
            return Ok(consents);
        }
    }
    
    public class ApproveConsentRequest
    {
        public string Notes { get; set; }
    }
    
    public class DeclineConsentRequest
    {
        public string Reason { get; set; }
    }
}
