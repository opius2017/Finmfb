using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using FinTech.Core.Application.Features.LoanConfiguration.Commands.CreateConfiguration;
using FinTech.Core.Application.Features.LoanConfiguration.Commands.UpdateConfiguration;
using FinTech.Core.Application.Features.LoanConfiguration.Queries.GetConfiguration;

namespace FinTech.Controllers.V1
{
    /// <summary>
    /// Super Admin Loan Configuration Management
    /// Controls system-wide parameters for interest rates, deduction limits, loan multipliers, etc.
    /// Only accessible by authorized Super Admins - restricted API
    /// </summary>
    [ApiController]
    [Route("api/v1/super-admin/loan-configurations")]
    [ApiVersion("1.0")]
    public class SuperAdminLoanConfigurationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SuperAdminLoanConfigurationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create a new system loan configuration parameter
        /// </summary>
        /// <remarks>
        /// Requires Super Admin role. Examples of configuration keys:
        /// - GLOBAL_INTEREST_RATE (18%)
        /// - MAX_DEDUCTION_RATE_PERCENT (40%)
        /// - LOAN_MULTIPLIER_NORMAL (3x)
        /// - COMMITTEE_APPROVAL_THRESHOLD (₦5,000,000)
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(CreateLoanConfigurationResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateConfiguration([FromBody] CreateLoanConfigurationCommand command)
        {
            var result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error });
            }

            return CreatedAtAction(nameof(GetConfiguration), new { id = result.Value.ConfigurationId }, result.Value);
        }

        /// <summary>
        /// Get a specific loan configuration parameter
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(LoanConfigurationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetConfiguration(int id)
        {
            var query = new GetLoanConfigurationQuery { ConfigurationId = id };
            var result = await _mediator.Send(query);

            if (result.IsFailure)
            {
                return NotFound(new { error = result.Error });
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Get configuration by key (e.g., "GLOBAL_INTEREST_RATE")
        /// </summary>
        [HttpGet("by-key/{configKey}")]
        [ProducesResponseType(typeof(LoanConfigurationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetConfigurationByKey(string configKey)
        {
            var query = new GetLoanConfigurationByKeyQuery { ConfigKey = configKey };
            var result = await _mediator.Send(query);

            if (result.IsFailure)
            {
                return NotFound(new { error = result.Error });
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Get all configurations paginated
        /// </summary>
        /// <param name="pageNumber">Page number (default 1)</param>
        /// <param name="pageSize">Items per page (default 10)</param>
        /// <param name="category">Filter by category (Interest, Deduction, Multiplier, Thresholds, Compliance)</param>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<LoanConfigurationDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllConfigurations([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string category = null)
        {
            var query = new GetAllLoanConfigurationsQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Category = category
            };
            var result = await _mediator.Send(query);

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error });
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Update an existing loan configuration parameter
        /// Tracks previous values for audit trail and approval routing
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UpdateLoanConfigurationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateConfiguration(int id, [FromBody] UpdateLoanConfigurationCommand command)
        {
            command.ConfigurationId = id;
            var result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error });
            }

            return Ok(result.Value);
        }
    }
}
