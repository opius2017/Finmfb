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
    [Route("api/commodity-voucher")]
    [Authorize]
    public class CommodityVoucherController : ControllerBase
    {
        private readonly ICommodityVoucherService _voucherService;
        private readonly ILogger<CommodityVoucherController> _logger;

        public CommodityVoucherController(
            ICommodityVoucherService voucherService,
            ILogger<CommodityVoucherController> logger)
        {
            _voucherService = voucherService;
            _logger = logger;
        }

        /// <summary>
        /// Generate a new commodity voucher
        /// </summary>
        [HttpPost("generate")]
        [Authorize(Roles = "Admin,LoanOfficer")]
        [ProducesResponseType(typeof(CommodityVoucherDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CommodityVoucherDto>> Generate([FromBody] GenerateVoucherRequest request)
        {
            try
            {
                var result = await _voucherService.GenerateVoucherAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating voucher");
                return StatusCode(500, new { message = "An error occurred while generating voucher" });
            }
        }

        /// <summary>
        /// Validate a voucher before redemption
        /// </summary>
        [HttpPost("validate")]
        [ProducesResponseType(typeof(VoucherValidationResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<VoucherValidationResult>> Validate([FromBody] ValidateVoucherRequest request)
        {
            try
            {
                var result = await _voucherService.ValidateVoucherAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating voucher");
                return StatusCode(500, new { message = "An error occurred while validating voucher" });
            }
        }

        /// <summary>
        /// Redeem a voucher for commodity purchase
        /// </summary>
        [HttpPost("redeem")]
        [Authorize(Roles = "Admin,StoreManager")]
        [ProducesResponseType(typeof(RedemptionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RedemptionResult>> Redeem([FromBody] RedeemVoucherRequest request)
        {
            try
            {
                var result = await _voucherService.RedeemVoucherAsync(request);
                
                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error redeeming voucher");
                return StatusCode(500, new { message = "An error occurred while redeeming voucher" });
            }
        }

        /// <summary>
        /// Get voucher by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CommodityVoucherDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommodityVoucherDto>> GetById(string id)
        {
            var result = await _voucherService.GetVoucherByIdAsync(id);
            
            if (result == null)
                return NotFound(new { message = "Voucher not found" });

            return Ok(result);
        }

        /// <summary>
        /// Get voucher by voucher number
        /// </summary>
        [HttpGet("number/{voucherNumber}")]
        [ProducesResponseType(typeof(CommodityVoucherDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommodityVoucherDto>> GetByNumber(string voucherNumber)
        {
            var result = await _voucherService.GetVoucherByNumberAsync(voucherNumber);
            
            if (result == null)
                return NotFound(new { message = "Voucher not found" });

            return Ok(result);
        }

        /// <summary>
        /// Get all vouchers for a member
        /// </summary>
        [HttpGet("member/{memberId}")]
        [ProducesResponseType(typeof(List<CommodityVoucherDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CommodityVoucherDto>>> GetMemberVouchers(
            string memberId,
            [FromQuery] string? status = null)
        {
            try
            {
                var result = await _voucherService.GetMemberVouchersAsync(memberId, status);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting member vouchers");
                return StatusCode(500, new { message = "An error occurred while retrieving vouchers" });
            }
        }

        /// <summary>
        /// Get all vouchers for a loan
        /// </summary>
        [HttpGet("loan/{loanId}")]
        [ProducesResponseType(typeof(List<CommodityVoucherDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CommodityVoucherDto>>> GetLoanVouchers(string loanId)
        {
            try
            {
                var result = await _voucherService.GetLoanVouchersAsync(loanId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting loan vouchers");
                return StatusCode(500, new { message = "An error occurred while retrieving vouchers" });
            }
        }

        /// <summary>
        /// Get redemption history for a voucher
        /// </summary>
        [HttpGet("{id}/redemptions")]
        [ProducesResponseType(typeof(List<CommodityRedemptionDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CommodityRedemptionDto>>> GetRedemptions(string id)
        {
            try
            {
                var result = await _voucherService.GetVoucherRedemptionsAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting voucher redemptions");
                return StatusCode(500, new { message = "An error occurred while retrieving redemptions" });
            }
        }

        /// <summary>
        /// Cancel a voucher
        /// </summary>
        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<bool>> Cancel(string id, [FromBody] string reason)
        {
            try
            {
                var userName = User.Identity?.Name ?? "Unknown";
                var result = await _voucherService.CancelVoucherAsync(id, userName, reason);
                
                if (!result)
                    return NotFound(new { message = "Voucher not found" });

                return Ok(new { message = "Voucher cancelled successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling voucher");
                return StatusCode(500, new { message = "An error occurred while cancelling voucher" });
            }
        }

        /// <summary>
        /// Get voucher balance
        /// </summary>
        [HttpGet("{id}/balance")]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        public async Task<ActionResult<decimal>> GetBalance(string id)
        {
            try
            {
                var result = await _voucherService.GetVoucherBalanceAsync(id);
                return Ok(new { voucherId = id, balance = result, formattedBalance = $"₦{result:N2}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting voucher balance");
                return StatusCode(500, new { message = "An error occurred while retrieving balance" });
            }
        }
    }
}
