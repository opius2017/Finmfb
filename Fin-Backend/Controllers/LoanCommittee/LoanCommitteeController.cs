using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using FinTech.Core.Application.Features.LoanCommittee.Commands.ApproveApplication;
using FinTech.Core.Application.Features.LoanCommittee.Queries.GetPendingApprovals;

namespace FinTech.Controllers.V1
{
    /// <summary>
    /// Loan Committee Approval Management
    /// Handles approval workflows for high-value and high-risk loan applications
    /// Implements governance and compliance requirements aligned with microfinance best practices
    /// </summary>
    [ApiController]
    [Route("api/v1/loan-committee")]
    [ApiVersion("1.0")]
    public class LoanCommitteeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LoanCommitteeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get pending loan applications awaiting committee approval
        /// </summary>
        /// <remarks>
        /// Returns applications in "Pending" or "InReview" status
        /// Can filter by risk rating (Low, Medium, High, Critical)
        /// Paginated results for performance
        /// </remarks>
        [HttpGet("pending-approvals")]
        [ProducesResponseType(typeof(PaginatedList<CommitteeApprovalDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPendingApprovals(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string riskRating = null,
            [FromQuery] string status = null)
        {
            var query = new GetPendingCommitteeApprovalsQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                RiskRating = riskRating,
                Status = status
            };

            var result = await _mediator.Send(query);

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error });
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Get detailed approval information for a specific committee review
        /// </summary>
        /// <remarks>
        /// Includes:
        /// - Member information and history
        /// - Loan application details
        /// - Risk assessment and credit score
        /// - Guarantor details and equity verification
        /// - Previous loan repayment history
        /// - Committee recommendation from credit officer
        /// </remarks>
        [HttpGet("approval/{approvalRefNumber}")]
        [ProducesResponseType(typeof(CommitteeApprovalDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetApprovalDetail(string approvalRefNumber)
        {
            var query = new GetCommitteeApprovalDetailQuery { ApprovalRefNumber = approvalRefNumber };
            var result = await _mediator.Send(query);

            if (result.IsFailure)
            {
                return NotFound(new { error = result.Error });
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Committee approves or rejects a high-value or high-risk loan application
        /// </summary>
        /// <remarks>
        /// Committee decision options:
        /// - "Approved": Proceed to disbursement
        /// - "Rejected": Application denied, provide reason
        /// - "ApprovedWithConditions": Approval with special terms (higher rate, lower amount, additional guarantor, etc.)
        /// 
        /// Audit trail includes:
        /// - Committee members who reviewed
        /// - Meeting date and decision date
        /// - Committee notes and conditions
        /// - Guarantor approval status
        /// 
        /// Applicant is notified immediately and can appeal if rejected
        /// </remarks>
        [HttpPost("approve-application")]
        [ProducesResponseType(typeof(ApproveLoanByCommitteeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ApproveLoanApplication([FromBody] ApproveLoanByCommitteeCommand command)
        {
            var result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                return BadRequest(new { error = result.Error });
            }

            return Ok(result.Value);
        }
    }
}
