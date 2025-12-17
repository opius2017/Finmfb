using System;
using System.Threading.Tasks;
using FinTech.Core.Application.Interfaces.Loans;
using FinTech.Core.Application.DTOs.Loans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FinTech.Controllers.Loans
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

        /*
        [HttpPost("register")]
        [Authorize(Roles = "Admin,FinanceOfficer")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> RegisterLoan([FromBody] RegisterLoanCommand command)
        {
            try
            {
                // Interface mismatch
                // var registerEntry = await _registerService.RegisterLoanAsync(command);
                // return CreatedAtAction(
                //    nameof(GetBySerialNumber),
                //    new { serialNumber = registerEntry.SerialNumber },
                //    registerEntry);
                return BadRequest("Not implemented due to interface mismatch");
            }
            catch (ArgumentException ex) { return NotFound(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
        */

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

        /*
        // Other methods commented out due to interface mismatch
        */
    }
}
