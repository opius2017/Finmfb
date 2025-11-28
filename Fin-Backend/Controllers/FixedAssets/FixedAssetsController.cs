using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinTech.Core.Application.Common.Models;
using FinTech.Core.Application.Features.FixedAssets.Commands.CreateFixedAsset;
using System.Threading;
using System.Threading.Tasks;

namespace FinTech.Controllers.FixedAssets
{
    /// <summary>
    /// Fixed Assets Management API
    /// Handles CRUD operations for fixed assets including depreciation tracking and disposal management
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class FixedAssetsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FixedAssetsController> _logger;

        public FixedAssetsController(IMediator mediator, ILogger<FixedAssetsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// List all fixed assets with pagination and filtering
        /// </summary>
        /// <remarks>
        /// Permission required: ViewFixedAssets or Admin
        /// Returns paginated list of fixed assets
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResult<FixedAssetListDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 401)]
        [ProducesResponseType(typeof(ApiResponse<object>), 403)]
        public async Task<IActionResult> ListAssets(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string searchTerm = null,
            [FromQuery] string categoryId = null,
            [FromQuery] string status = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"Listing fixed assets - Page: {pageNumber}, PageSize: {pageSize}");

                // TODO: Implement ListFixedAssetsQuery
                var dummyResult = new PaginatedResult<FixedAssetListDto>
                {
                    Items = new List<FixedAssetListDto>(),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = 0
                };

                return Ok(new ApiResponse<PaginatedResult<FixedAssetListDto>>
                {
                    Success = true,
                    Data = dummyResult
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing fixed assets");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Error = Error.New("InternalError", "An error occurred while listing assets")
                });
            }
        }

        /// <summary>
        /// Get a specific fixed asset by ID
        /// </summary>
        /// <remarks>
        /// Permission required: ViewFixedAssets or Admin
        /// </remarks>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<FixedAssetDetailDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 401)]
        public async Task<IActionResult> GetAsset(
            string id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"Getting fixed asset: {id}");

                // TODO: Implement GetFixedAssetQuery
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Error = Error.New("NotFound", $"Fixed asset with ID {id} not found")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting fixed asset: {id}");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Error = Error.New("InternalError", "An error occurred while retrieving the asset")
                });
            }
        }

        /// <summary>
        /// Create a new fixed asset
        /// </summary>
        /// <remarks>
        /// Permission required: CreateFixedAsset or Admin
        /// Required roles: AssetManager, Admin
        /// </remarks>
        [HttpPost]
        [Authorize(Roles = "AssetManager,Admin")]
        [ProducesResponseType(typeof(ApiResponse<CreateFixedAssetResponse>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 401)]
        [ProducesResponseType(typeof(ApiResponse<object>), 403)]
        public async Task<IActionResult> CreateAsset(
            [FromBody] CreateFixedAssetCommand command,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Error = Error.New("ValidationError", "Invalid model state")
                    });

                _logger.LogInformation($"Creating fixed asset: {command.AssetCode}");

                var result = await _mediator.Send(command, cancellationToken);

                if (!result.IsSuccess)
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Error = result.Error
                    });

                return CreatedAtAction(
                    nameof(GetAsset),
                    new { id = result.Value.Id },
                    new ApiResponse<CreateFixedAssetResponse>
                    {
                        Success = true,
                        Data = result.Value
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating fixed asset");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Error = Error.New("InternalError", "An error occurred while creating the asset")
                });
            }
        }

        /// <summary>
        /// Update a fixed asset
        /// </summary>
        /// <remarks>
        /// Permission required: EditFixedAsset or Admin
        /// Required roles: AssetManager, Admin
        /// </remarks>
        [HttpPut("{id}")]
        [Authorize(Roles = "AssetManager,Admin")]
        [ProducesResponseType(typeof(ApiResponse<FixedAssetDetailDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 401)]
        [ProducesResponseType(typeof(ApiResponse<object>), 403)]
        public async Task<IActionResult> UpdateAsset(
            string id,
            [FromBody] UpdateFixedAssetCommand command,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Error = Error.New("ValidationError", "Invalid model state")
                    });

                command.Id = id;
                _logger.LogInformation($"Updating fixed asset: {id}");

                // TODO: Implement UpdateFixedAssetHandler
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Error = Error.New("NotFound", $"Fixed asset with ID {id} not found")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating fixed asset: {id}");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Error = Error.New("InternalError", "An error occurred while updating the asset")
                });
            }
        }

        /// <summary>
        /// Delete a fixed asset
        /// </summary>
        /// <remarks>
        /// Permission required: DeleteFixedAsset or Admin
        /// Required roles: Admin
        /// Soft deletes the asset (marks as deleted)
        /// </remarks>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 401)]
        [ProducesResponseType(typeof(ApiResponse<object>), 403)]
        public async Task<IActionResult> DeleteAsset(
            string id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"Deleting fixed asset: {id}");

                // TODO: Implement DeleteFixedAssetHandler
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Error = Error.New("NotFound", $"Fixed asset with ID {id} not found")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting fixed asset: {id}");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Error = Error.New("InternalError", "An error occurred while deleting the asset")
                });
            }
        }

        /// <summary>
        /// Record depreciation for an asset
        /// </summary>
        /// <remarks>
        /// Permission required: RecordAssetDepreciation or Admin
        /// Required roles: Accountant, Admin
        /// </remarks>
        [HttpPost("{id}/depreciation")]
        [Authorize(Roles = "Accountant,Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> RecordDepreciation(
            string id,
            [FromBody] RecordDepreciationCommand command,
            CancellationToken cancellationToken = default)
        {
            try
            {
                command.AssetId = id;
                _logger.LogInformation($"Recording depreciation for asset: {id}");

                // TODO: Implement RecordDepreciationHandler
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Error = Error.New("NotFound", $"Fixed asset with ID {id} not found")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error recording depreciation for asset: {id}");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Error = Error.New("InternalError", "An error occurred while recording depreciation")
                });
            }
        }

        /// <summary>
        /// Dispose of a fixed asset
        /// </summary>
        /// <remarks>
        /// Permission required: DisposeAsset or Admin
        /// Required roles: AssetManager, Admin
        /// </remarks>
        [HttpPost("{id}/dispose")]
        [Authorize(Roles = "AssetManager,Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> DisposeAsset(
            string id,
            [FromBody] DisposeAssetCommand command,
            CancellationToken cancellationToken = default)
        {
            try
            {
                command.AssetId = id;
                _logger.LogInformation($"Disposing fixed asset: {id}");

                // TODO: Implement DisposeAssetHandler
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Error = Error.New("NotFound", $"Fixed asset with ID {id} not found")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error disposing asset: {id}");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Error = Error.New("InternalError", "An error occurred while disposing the asset")
                });
            }
        }
    }

    /// <summary>
    /// Standard API response wrapper
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public Error Error { get; set; }
    }

    // DTOs
    public class FixedAssetListDto
    {
        public string Id { get; set; }
        public string AssetCode { get; set; }
        public string AssetName { get; set; }
        public string CategoryName { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal BookValue { get; set; }
        public string Status { get; set; }
        public DateTime AcquisitionDate { get; set; }
    }

    public class FixedAssetDetailDto
    {
        public string Id { get; set; }
        public string AssetCode { get; set; }
        public string AssetName { get; set; }
        public string Description { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal AccumulatedDepreciation { get; set; }
        public decimal BookValue { get; set; }
        public decimal SalvageValue { get; set; }
        public int UsefulLifeYears { get; set; }
        public string Status { get; set; }
        public DateTime AcquisitionDate { get; set; }
    }

    // Commands
    public class UpdateFixedAssetCommand : IRequest<Result<FixedAssetDetailDto>>
    {
        public string Id { get; set; }
        public string AssetName { get; set; }
        public string Description { get; set; }
        public decimal SalvageValue { get; set; }
        public int UsefulLifeYears { get; set; }
        public string LocationId { get; set; }
        public string DepartmentId { get; set; }
    }

    public class RecordDepreciationCommand : IRequest<Result<object>>
    {
        public string AssetId { get; set; }
        public decimal DepreciationAmount { get; set; }
    }

    public class DisposeAssetCommand : IRequest<Result<object>>
    {
        public string AssetId { get; set; }
        public DateTime DisposalDate { get; set; }
        public decimal DisposalPrice { get; set; }
    }

    /// <summary>
    /// Paginated result for list responses
    /// </summary>
    public class PaginatedResult<T>
    {
        public List<T> Items { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}
