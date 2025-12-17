using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Accounting;
using FinTech.Core.Application.Interfaces;
using FinTech.Core.Application.Interfaces.Services;
using FinTech.Core.Application.Interfaces.Services.Accounting;
using FinTech.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FinTech.Controllers.Accounting
{
    [ApiController]
    [Route("api/[controller]")]
    public class FixedAssetsController : ControllerBase
    {
        private readonly IFixedAssetService _fixedAssetService;
        private readonly ILogger<FixedAssetsController> _logger;
        private readonly IFixedAssetService _enhancedFixedAssetService;

        public FixedAssetsController(
            IFixedAssetService fixedAssetService,
            ILogger<FixedAssetsController> logger,
            IFixedAssetService enhancedFixedAssetService = null)
        {
            _fixedAssetService = fixedAssetService;
            _logger = logger;
            _enhancedFixedAssetService = enhancedFixedAssetService;
        }

        [HttpGet]
        [ResourceAuthorization("FixedAsset", ResourceOperation.Read)]
        public async Task<ActionResult<IEnumerable<FixedAssetDto>>> GetAll()
        {
            _logger.LogInformation("Getting all fixed assets");
            var assets = await _fixedAssetService.GetAllFixedAssetsAsync();
            return Ok(assets);
        }

        [HttpGet("{id}")]
        [ResourceAuthorization("FixedAsset", ResourceOperation.Read)]
        public async Task<ActionResult<FixedAssetDto>> GetById(string id)
        {
            _logger.LogInformation("Getting fixed asset with ID {AssetId}", id);
            var asset = await _fixedAssetService.GetFixedAssetAsync(id);
            
            if (asset == null)
            {
                _logger.LogWarning("Fixed asset with ID {AssetId} not found", id);
                return NotFound();
            }
            
            return Ok(asset);
        }

        [HttpPost]
        [ResourceAuthorization("FixedAsset", ResourceOperation.Create)]
        public async Task<ActionResult<FixedAssetDto>> Create(CreateFixedAssetDto createDto)
        {
            _logger.LogInformation("Creating new fixed asset {AssetName}", createDto.AssetName);
            
            var result = await _fixedAssetService.CreateFixedAssetAsync(createDto);
            _logger.LogInformation("Successfully created fixed asset with ID {AssetId}", result.Id);
            
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [ResourceAuthorization("FixedAsset", ResourceOperation.Update)]
        public async Task<IActionResult> Update(string id, UpdateFixedAssetDto updateDto)
        {
            _logger.LogInformation("Updating fixed asset with ID {AssetId}", id);
            
            // DTO doesn't have Id, assuming path ID is correct
            
            var result = await _fixedAssetService.UpdateFixedAssetAsync(id, updateDto);
            
            if (result == null)
            {
                _logger.LogWarning("Fixed asset with ID {AssetId} not found for update", id);
                return NotFound();
            }
            
            _logger.LogInformation("Successfully updated fixed asset with ID {AssetId}", id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ResourceAuthorization("FixedAsset", ResourceOperation.Delete)]
        public async Task<IActionResult> Delete(string id)
        {
            _logger.LogInformation("Deleting fixed asset with ID {AssetId}", id);
            
            var success = await _fixedAssetService.DeleteFixedAssetAsync(id);
            
            if (!success)
            {
                _logger.LogWarning("Fixed asset with ID {AssetId} not found for deletion", id);
                return NotFound();
            }
            
            _logger.LogInformation("Successfully deleted fixed asset with ID {AssetId}", id);
            return NoContent();
        }

        [HttpPost("{id}/dispose")]
        [ResourceAuthorization("FixedAsset", ResourceOperation.Process)]
        public async Task<IActionResult> Dispose(string id, AssetDisposalDto disposalDto)
        {
            _logger.LogInformation("Processing disposal for asset with ID {AssetId}", id);
            
            var result = await _fixedAssetService.RecordAssetDisposalAsync(id, disposalDto);
            
            if (result == null)
            {
                _logger.LogWarning("Failed to process disposal for asset with ID {AssetId}", id);
                return BadRequest("Failed to process disposal");
            }
            
            _logger.LogInformation("Successfully processed disposal for asset with ID {AssetId}", id);
            return NoContent();
        }

        [HttpPost("{id}/revalue")]
        [ResourceAuthorization("FixedAsset", ResourceOperation.Process)]
        public async Task<IActionResult> Revalue(string id, AssetRevaluationDto revaluationDto)
        {
            // Revaluation not implemented in service yet
            return StatusCode(501, "Not Implemented");
        }
        
        [HttpPost("{id}/depreciate")]
        [ResourceAuthorization("FixedAsset", ResourceOperation.Process)]
        public async Task<IActionResult> Depreciate(string id, AssetDepreciationDto depreciationDto)
        {
            _logger.LogInformation("Processing depreciation for asset with ID {AssetId}", id);
            
            // Service calculates amount automatically based on period
            var success = await _fixedAssetService.RecordDepreciationAsync(id, depreciationDto.FinancialPeriodId);
            
            if (!success)
            {
                _logger.LogWarning("Failed to process depreciation for asset with ID {AssetId}", id);
                return BadRequest("Failed to process depreciation");
            }
            
            _logger.LogInformation("Successfully processed depreciation for asset with ID {AssetId}", id);
            return NoContent();
        }
        
        #region Enhanced Fixed Asset Management
        
        [HttpGet("enhanced/{id}/schedule")]
        [ResourceAuthorization("FixedAsset", ResourceOperation.Read)]
        public async Task<IActionResult> GenerateDepreciationSchedule(
            string id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (_enhancedFixedAssetService == null)
                {
                    return BadRequest(new { message = "Enhanced Fixed Asset Service not available" });
                }
                
                var result = await _enhancedFixedAssetService.GenerateDepreciationScheduleAsync(id, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating depreciation schedule for asset {AssetId}", id);
                return BadRequest(new { message = ex.Message });
            }
        }
        
        [HttpGet("enhanced/report/{financialPeriodId}")]
        [ResourceAuthorization("FixedAsset", ResourceOperation.Read)]
        public async Task<IActionResult> GenerateFixedAssetReport(
            string financialPeriodId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (_enhancedFixedAssetService == null)
                {
                    return BadRequest(new { message = "Enhanced Fixed Asset Service not available" });
                }
                
                var result = await _enhancedFixedAssetService.GenerateFixedAssetReportAsync(financialPeriodId, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating fixed asset report for period {PeriodId}", financialPeriodId);
                return BadRequest(new { message = ex.Message });
            }
        }
        
        #endregion
    }
}

