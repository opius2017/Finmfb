using System;
using System.Threading.Tasks;
using FinTech.Core.Application.Services.Accounting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTech.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RegulatoryMappingController : ControllerBase
    {
        private readonly IRegulatoryMappingService _regulatoryMappingService;

        public RegulatoryMappingController(IRegulatoryMappingService regulatoryMappingService)
        {
            _regulatoryMappingService = regulatoryMappingService ?? throw new ArgumentNullException(nameof(regulatoryMappingService));
        }

        /// <summary>
        /// Gets a specific regulatory mapping by ID
        /// </summary>
        /// <param name="id">The ID of the mapping</param>
        /// <returns>The regulatory mapping</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMapping(string id)
        {
            try
            {
                var result = await _regulatoryMappingService.GetMappingAsync(id);
                if (result == null)
                {
                    return NotFound();
                }
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Gets all regulatory mappings for a specific chart of account
        /// </summary>
        /// <param name="accountId">The ID of the chart of account</param>
        /// <returns>A list of regulatory mappings</returns>
        [HttpGet("by-account/{accountId}")]
        public async Task<IActionResult> GetMappingsByAccount(string accountId)
        {
            try
            {
                var result = await _regulatoryMappingService.GetMappingsByAccountAsync(accountId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Gets all regulatory mappings for a specific regulatory code
        /// </summary>
        /// <param name="codeId">The ID of the regulatory code</param>
        /// <returns>A list of regulatory mappings</returns>
        [HttpGet("by-code/{codeId}")]
        public async Task<IActionResult> GetMappingsByCode(string codeId)
        {
            try
            {
                var result = await _regulatoryMappingService.GetMappingsByRegulatoryCodeAsync(codeId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Creates a new regulatory mapping
        /// </summary>
        /// <param name="request">The mapping creation request</param>
        /// <returns>The created mapping</returns>
        [HttpPost]
        public async Task<IActionResult> CreateMapping(CreateRegulatoryMappingDto request)
        {
            try
            {
                var result = await _regulatoryMappingService.CreateMappingAsync(request);
                return CreatedAtAction(nameof(GetMapping), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing regulatory mapping
        /// </summary>
        /// <param name="id">The ID of the mapping to update</param>
        /// <param name="request">The mapping update request</param>
        /// <returns>The updated mapping</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMapping(string id, UpdateRegulatoryMappingDto request)
        {
            try
            {
                var result = await _regulatoryMappingService.UpdateMappingAsync(id, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Deactivates a regulatory mapping
        /// </summary>
        /// <param name="id">The ID of the mapping to deactivate</param>
        /// <returns>Success or failure</returns>
        [HttpPost("{id}/deactivate")]
        public async Task<IActionResult> DeactivateMapping(string id)
        {
            try
            {
                var result = await _regulatoryMappingService.DeactivateMappingAsync(id);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Activates a regulatory mapping
        /// </summary>
        /// <param name="id">The ID of the mapping to activate</param>
        /// <returns>Success or failure</returns>
        [HttpPost("{id}/activate")]
        public async Task<IActionResult> ActivateMapping(string id)
        {
            try
            {
                var result = await _regulatoryMappingService.ActivateMappingAsync(id);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Generates a regulatory report for a specific form and financial period
        /// </summary>
        /// <param name="reportingForm">The code of the reporting form</param>
        /// <param name="financialPeriodId">The ID of the financial period</param>
        /// <param name="includeZeroBalances">Whether to include zero balances</param>
        /// <param name="currencyCode">The currency code for the report</param>
        /// <returns>The regulatory report</returns>
        [HttpGet("report")]
        public async Task<IActionResult> GenerateReport(
            string reportingForm,
            string financialPeriodId,
            bool includeZeroBalances = false,
            string currencyCode = null)
        {
            try
            {
                var result = await _regulatoryMappingService.GenerateRegulatoryReportAsync(
                    reportingForm,
                    financialPeriodId,
                    includeZeroBalances,
                    currencyCode);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
