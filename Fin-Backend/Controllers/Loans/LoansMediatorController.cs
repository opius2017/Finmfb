using Microsoft.AspNetCore.Mvc;
using MediatR;
using FinTech.Core.Application.Features.Loans.Commands.CreateLoan;
using FinTech.Core.Application.Features.Loans.Queries.GetLoan;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace FinTech.Controllers.Loans
{
    [ApiController]
    [Route("api/v1/loans")]
    [ApiVersion("1.0")]
    public class LoansMediatorController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LoansMediatorController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create a new loan application
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(CreateLoanResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateLoan([FromBody] CreateLoanCommand command)
        {
            var result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error });
            }

            return CreatedAtAction(
                nameof(GetLoan),
                new { id = result.Value.LoanId },
                result.Value);
        }

        /// <summary>
        /// Get loan by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(LoanDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetLoan(string id)
        {
            var query = new GetLoanQuery(id);
            var result = await _mediator.Send(query);

            if (result.IsFailure)
            {
                return NotFound(new { error = result.Error });
            }

            return Ok(result.Value);
        }
    }
}
