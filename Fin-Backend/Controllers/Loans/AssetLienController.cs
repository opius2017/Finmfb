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
    [Route("api/asset-lien")]
    [Authorize]
    public class AssetLienController : ControllerBase
    {
        private readonly IAssetLienService _lienService;
        private readonly ILogger<AssetLienController> _logger;

        public AssetLienController(
            IAssetLienService lienService,
            ILogger<AssetLienController> logger)
        {
            _lienService = lienService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new asset lien
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,LoanOfficer")]
        [ProducesResponseType(typeof(AssetLienDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AssetLienDto>> Create([FromBody] CreateAssetLienRequest request)
        {
            try
            {
                var result = await _lienService.CreateAssetLienAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating asset lien");
                return StatusCode(500, new { message = "An error occurred while creating asset lien" });
            }
        }

        /// <summary>
        /// Release an asset lien
        /// </summary>
        [HttpPost("{id}/release")]
        [Authorize(Roles = "Admin,LoanOfficer")]
        [ProducesResponseType(typeof(AssetLienDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AssetLienDto>> Release(string id, [FromBody] ReleaseAssetLienRequest request)
        {
            try
            {
                request.LienId = id;
                var result = await _lienService.ReleaseAssetLienAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing asset lien");
                return StatusCode(500, new { message = "An error occurred while releasing asset lien" });
            }
        }

        /// <summary>
        /// Get asset lien by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AssetLienDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AssetLienDto>> GetById(string id)
        {
            var result = await _lienService.GetAssetLienByIdAsync(id);
            
            if (result == null)
                return NotFound(new { message = "Asset lien not found" });

            return Ok(result);
        }

        /// <summary>
        /// Get all asset liens for a loan
        /// </summary>
        [HttpGet("loan/{loanId}")]
        [ProducesResponseType(typeof(List<AssetLienDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<AssetLienDto>>> GetLoanLiens(string loanId)
        {
            try
            {
                var result = await _lienService.GetLoanAssetLiensAsync(loanId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting loan asset liens");
                return StatusCode(500, new { message = "An error occurred while retrieving asset liens" });
            }
        }

        /// <summary>
        /// Get all asset liens for a member
        /// </summary>
        [HttpGet("member/{memberId}")]
        [ProducesResponseType(typeof(List<AssetLienDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<AssetLienDto>>> GetMemberLiens(
            string memberId,
            [FromQuery] string? status = null)
        {
            try
            {
                var result = await _lienService.GetMemberAssetLiensAsync(memberId, status);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting member asset liens");
                return StatusCode(500, new { message = "An error occurred while retrieving asset liens" });
            }
        }

        /// <summary>
        /// Get all active asset liens
        /// </summary>
        [HttpGet("active")]
        [Authorize(Roles = "Admin,LoanOfficer")]
        [ProducesResponseType(typeof(List<AssetLienDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<AssetLienDto>>> GetActiveLiens()
        {
            try
            {
                var result = await _lienService.GetActiveAssetLiensAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active asset liens");
                return StatusCode(500, new { message = "An error occurred while retrieving active liens" });
            }
        }

        /// <summary>
        /// Check if a loan has active liens
        /// </summary>
        [HttpGet("loan/{loanId}/has-active")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> HasActiveLiens(string loanId)
        {
            try
            {
                var result = await _lienService.HasActiveLiensAsync(loanId);
                return Ok(new { loanId, hasActiveLiens = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking active liens for loan");
                return StatusCode(500, new { message = "An error occurred while checking liens" });
            }
        }

        /// <summary>
        /// Get total value of assets under lien for a member
        /// </summary>
        [HttpGet("member/{memberId}/total-value")]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        public async Task<ActionResult<decimal>> GetMemberTotalLienValue(string memberId)
        {
            try
            {
                var result = await _lienService.GetMemberTotalLienValueAsync(memberId);
                return Ok(new 
                { 
                    memberId, 
                    totalLienValue = result, 
                    formattedValue = $"₦{result:N2}" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting member total lien value");
                return StatusCode(500, new { message = "An error occurred while calculating total value" });
            }
        }
    }
}
