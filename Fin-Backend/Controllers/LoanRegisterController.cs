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
    public class LoanRegisterController : ControllerBase
    {
        private readonly ILoanRegisterService _registerService;
        private readonly ILogger<LoanRegisterController> _logger;
        
        public LoanRegisterController(
            ILoanRegisterService registerService,
            ILogger<LoanRegisterController> logger)
        {
            _registerService = registerService ?? throw new ArgumentNullException(nameof(registerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Register an approved loan in the official register
        /// </summary>
        /// <param name="command">Registration details</param>
        /// <returns>Created register entry</returns>
        [HttpPost("register")]
        [Authorize(Roles = "Admin,FinanceOfficer")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> RegisterLoan([FromBody] RegisterLoanCommand command)
        {
            try
            {
                var registerEntry = await _registerService.RegisterLoanAsync(command);
                return CreatedAtAction(
                    nameof(GetBySerialNumber),
                    new { serialNumber = registerEntry.SerialNumber },
                    registerEntry);
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
        /// Get register entry by serial number
        /// </summary>
        /// <param name="serialNumber">Serial number (e.g., LH/2024/001)</param>
        /// <returns>Register entry</returns>
        [HttpGet("serial/{serialNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetBySerialNumber(string serialNumber)
        {
            var entry = await _registerService.GetBySerialNumberAsync(serialNumber);
            if (entry == null)
                return NotFound($"Register entry not found: {serialNumber}");
            
            return Ok(entry);
        }
        
        /// <summary>
        /// Get register entry by loan ID
        /// </summary>
        /// <param name="loanId">Loan ID</param>
        /// <returns>Register entry</returns>
        [HttpGet("loan/{loanId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetByLoanId(Guid loanId)
        {
            var entry = await _registerService.GetByLoanIdAsync(loanId);
            if (entry == null)
                return NotFound($"Register entry not found for loan: {loanId}");
            
            return Ok(entry);
        }
        
        /// <summary>
        /// Get all register entries for a member
        /// </summary>
        /// <param name="memberId">Member ID</param>
        /// <returns>List of register entries</returns>
        [HttpGet("member/{memberId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetMemberEntries(Guid memberId)
        {
            var entries = await _registerService.GetMemberRegisterEntriesAsync(memberId);
            return Ok(entries);
        }
        
        /// <summary>
        /// Get monthly register report
        /// </summary>
        /// <param name="year">Year</param>
        /// <param name="month">Month (1-12)</param>
        /// <returns>Monthly register report</returns>
        [HttpGet("monthly/{year}/{month}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<MonthlyRegisterReport>> GetMonthlyRegister(int year, int month)
        {
            if (month < 1 || month > 12)
                return BadRequest("Month must be between 1 and 12");
            
            var report = await _registerService.GetMonthlyRegisterAsync(year, month);
            return Ok(report);
        }
        
        /// <summary>
        /// Export monthly register to Excel
        /// </summary>
        /// <param name="year">Year</param>
        /// <param name="month">Month (1-12)</param>
        /// <returns>Excel file</returns>
        [HttpGet("monthly/{year}/{month}/export")]
        [Authorize(Roles = "Admin,FinanceOfficer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> ExportMonthlyRegister(int year, int month)
        {
            if (month < 1 || month > 12)
                return BadRequest("Month must be between 1 and 12");
            
            var excelData = await _registerService.ExportMonthlyRegisterAsync(year, month);
            
            var fileName = $"LoanRegister_{year}_{month:D2}.xlsx";
            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        
        /// <summary>
        /// Get register statistics
        /// </summary>
        /// <param name="year">Year</param>
        /// <param name="month">Optional month (1-12)</param>
        /// <returns>Register statistics</returns>
        [HttpGet("statistics/{year}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<RegisterStatistics>> GetStatistics(
            int year,
            [FromQuery] int? month = null)
        {
            if (month.HasValue && (month.Value < 1 || month.Value > 12))
                return BadRequest("Month must be between 1 and 12");
            
            var stats = await _registerService.GetRegisterStatisticsAsync(year, month);
            return Ok(stats);
        }
        
        /// <summary>
        /// Generate next serial number (for preview)
        /// </summary>
        /// <param name="year">Year</param>
        /// <param name="month">Month</param>
        /// <returns>Next serial number</returns>
        [HttpGet("next-serial/{year}/{month}")]
        [Authorize(Roles = "Admin,FinanceOfficer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetNextSerialNumber(int year, int month)
        {
            var serialNumber = await _registerService.GenerateSerialNumberAsync(year, month);
            return Ok(new { serialNumber, year, month });
        }
    }
}
