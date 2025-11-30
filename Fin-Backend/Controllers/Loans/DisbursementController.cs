using System;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Loans;
using FinTech.Core.Application.Services.Loans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FinTech.Controllers.Loans
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Finance,Admin")]
    public class DisbursementController : ControllerBase
    {
        private readonly ILoanDisbursementService _disbursementService;
        private readonly ILoanRegisterService _registerService;
        private readonly IMonthlyThresholdService _thresholdService;
        private readonly ILogger<DisbursementController> _logger;

        public DisbursementController(
            ILoanDisbursementService disbursementService,
            ILoanRegisterService registerService,
            IMonthlyThresholdService thresholdService,
            ILogger<DisbursementController> logger)
        {
            _disbursementService = disbursementService;
            _registerService = registerService;
            _thresholdService = thresholdService;
            _logger = logger;
        }

        /// <summary>
        /// Disburses a cash loan
        /// </summary>
        [HttpPost("disburse-cash-loan")]
        [ProducesResponseType(typeof(DisbursementResult), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> DisburseCashLoan([FromBody] DisbursementRequest request)
        {
            try
            {
                request.DisbursedBy = User.Identity.Name;
                var result = await _disbursementService.DisburseCashLoanAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disbursing cash loan");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Tracks disbursement transaction status
        /// </summary>
        [HttpGet("track-transaction/{transactionId}")]
        [ProducesResponseType(typeof(TransactionTrackingResult), 200)]
        public async Task<IActionResult> TrackTransaction(string transactionId)
        {
            try
            {
                var result = await _disbursementService.TrackDisbursementAsync(transactionId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking transaction");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets disbursement history for a member
        /// </summary>
        [HttpGet("history/{memberId}")]
        [ProducesResponseType(typeof(DisbursementHistory), 200)]
        public async Task<IActionResult> GetDisbursementHistory(string memberId)
        {
            try
            {
                var history = await _disbursementService.GetMemberDisbursementHistoryAsync(memberId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting disbursement history");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets loan register
        /// </summary>
        [HttpGet("loan-register")]
        [ProducesResponseType(typeof(System.Collections.Generic.List<LoanRegisterEntry>), 200)]
        public async Task<IActionResult> GetLoanRegister(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] string status)
        {
            try
            {
                var register = await _registerService.GetLoanRegisterAsync(fromDate, toDate, status);
                return Ok(register);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting loan register");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets loan by serial number
        /// </summary>
        [HttpGet("loan-register/{serialNumber}")]
        [ProducesResponseType(typeof(LoanRegisterEntry), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetBySerialNumber(string serialNumber)
        {
            try
            {
                var entry = await _registerService.GetBySerialNumberAsync(serialNumber);
                if (entry == null)
                    return NotFound(new { error = "Loan not found" });

                return Ok(entry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting loan by serial number");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Exports loan register to CSV
        /// </summary>
        [HttpGet("export-register")]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> ExportLoanRegister(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            try
            {
                var csv = await _registerService.ExportLoanRegisterAsync(fromDate, toDate);
                return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "loan-register.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting loan register");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets register statistics
        /// </summary>
        [HttpGet("register-statistics")]
        [ProducesResponseType(typeof(LoanRegisterStatistics), 200)]
        public async Task<IActionResult> GetRegisterStatistics([FromQuery] int? year)
        {
            try
            {
                var stats = await _registerService.GetRegisterStatisticsAsync(year);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting register statistics");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Checks monthly threshold availability
        /// </summary>
        [HttpPost("check-threshold")]
        [ProducesResponseType(typeof(ThresholdCheckResult), 200)]
        public async Task<IActionResult> CheckThreshold([FromBody] ThresholdCheckRequestDto request)
        {
            try
            {
                var result = await _thresholdService.CheckThresholdAsync(
                    request.LoanAmount,
                    request.TargetMonth,
                    request.TargetYear);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking threshold");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets monthly threshold information
        /// </summary>
        [HttpGet("threshold-info")]
        [ProducesResponseType(typeof(MonthlyThresholdInfo), 200)]
        public async Task<IActionResult> GetThresholdInfo(
            [FromQuery] int month,
            [FromQuery] int year)
        {
            try
            {
                var info = await _thresholdService.GetMonthlyThresholdInfoAsync(month, year);
                return Ok(info);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting threshold info");
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    public class ThresholdCheckRequestDto
    {
        public decimal LoanAmount { get; set; }
        public int? TargetMonth { get; set; }
        public int? TargetYear { get; set; }
    }
}
