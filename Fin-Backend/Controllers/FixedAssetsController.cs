using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Application.DTOs.FixedAssets;
using FinTech.Application.Services;
using FinTech.Application.Services.Accounting;
using FinTech.Infrastructure.Security.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FinTech.WebAPI.Controllers
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
            var assets = await _fixedAssetService.GetAllAssetsAsync();
            return Ok(assets);
        }

        [HttpGet("{id}")]
        [ResourceAuthorization("FixedAsset", ResourceOperation.Read)]
        public async Task<ActionResult<FixedAssetDto>> GetById(int id)
        {
            _logger.LogInformation("Getting fixed asset with ID {AssetId}", id);
            var asset = await _fixedAssetService.GetAssetByIdAsync(id);
            
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
            _logger.LogInformation("Creating new fixed asset {AssetName}", createDto.Name);
            
            var result = await _fixedAssetService.CreateAssetAsync(createDto);
            _logger.LogInformation("Successfully created fixed asset with ID {AssetId}", result.Id);
            
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [ResourceAuthorization("FixedAsset", ResourceOperation.Update)]
        public async Task<IActionResult> Update(int id, UpdateFixedAssetDto updateDto)
        {
            _logger.LogInformation("Updating fixed asset with ID {AssetId}", id);
            
            if (id != updateDto.Id)
            {
                _logger.LogWarning("ID mismatch: URL ID {UrlId} vs DTO ID {DtoId}", id, updateDto.Id);
                return BadRequest("ID mismatch");
            }
            
            var success = await _fixedAssetService.UpdateAssetAsync(updateDto);
            
            if (!success)
            {
                _logger.LogWarning("Fixed asset with ID {AssetId} not found for update", id);
                return NotFound();
            }
            
            _logger.LogInformation("Successfully updated fixed asset with ID {AssetId}", id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ResourceAuthorization("FixedAsset", ResourceOperation.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting fixed asset with ID {AssetId}", id);
            
            var success = await _fixedAssetService.DeleteAssetAsync(id);
            
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
        public async Task<IActionResult> Dispose(int id, AssetDisposalDto disposalDto)
        {
            _logger.LogInformation("Processing disposal for asset with ID {AssetId}", id);
            
            if (id != disposalDto.AssetId)
            {
                _logger.LogWarning("ID mismatch: URL ID {UrlId} vs DTO ID {DtoId}", id, disposalDto.AssetId);
                return BadRequest("ID mismatch");
            }
            
            var success = await _fixedAssetService.ProcessAssetDisposalAsync(disposalDto);
            
            if (!success)
            {
                _logger.LogWarning("Failed to process disposal for asset with ID {AssetId}", id);
                return BadRequest("Failed to process disposal");
            }
            
            _logger.LogInformation("Successfully processed disposal for asset with ID {AssetId}", id);
            return NoContent();
        }

        [HttpPost("{id}/revalue")]
        [ResourceAuthorization("FixedAsset", ResourceOperation.Process)]
        public async Task<IActionResult> Revalue(int id, AssetRevaluationDto revaluationDto)
        {
            _logger.LogInformation("Processing revaluation for asset with ID {AssetId}", id);
            
            if (id != revaluationDto.AssetId)
            {
                _logger.LogWarning("ID mismatch: URL ID {UrlId} vs DTO ID {DtoId}", id, revaluationDto.AssetId);
                return BadRequest("ID mismatch");
            }
            
            var success = await _fixedAssetService.ProcessAssetRevaluationAsync(revaluationDto);
            
            if (!success)
            {
                _logger.LogWarning("Failed to process revaluation for asset with ID {AssetId}", id);
                return BadRequest("Failed to process revaluation");
            }
            
            _logger.LogInformation("Successfully processed revaluation for asset with ID {AssetId}", id);
            return NoContent();
        }
        
        [HttpPost("{id}/depreciate")]
        [ResourceAuthorization("FixedAsset", ResourceOperation.Process)]
        public async Task<IActionResult> Depreciate(int id, AssetDepreciationDto depreciationDto)
        {
            _logger.LogInformation("Processing depreciation for asset with ID {AssetId}", id);
            
            if (id != depreciationDto.AssetId)
            {
                _logger.LogWarning("ID mismatch: URL ID {UrlId} vs DTO ID {DtoId}", id, depreciationDto.AssetId);
                return BadRequest("ID mismatch");
            }
            
            var success = await _fixedAssetService.ProcessAssetDepreciationAsync(depreciationDto);
            
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