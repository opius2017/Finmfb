using MediatR;
using FinTech.Core.Domain.Common;

namespace FinTech.Core.Application.Features.LoanCommittee.Commands.ApproveApplication
{
    /// <summary>
    /// Command for Loan Committee to approve a high-value or high-risk loan application
    /// Implements governance and compliance requirements
    /// </summary>
    public class ApproveLoanByCommitteeCommand : IRequest<Result<ApproveLoanByCommitteeResponse>>
    {
        public int LoanApplicationId { get; set; }
        public string CommitteeDecision { get; set; } // "Approved", "Rejected", "ApprovedWithConditions"
        public string CommitteeNotes { get; set; }
        public string ApprovalConditions { get; set; }
        public string ApprovedByOfficerId { get; set; }
        public DateTime CommitteeMeetingDate { get; set; }
        public string CommitteeReviewers { get; set; } // Comma-separated officer IDs
    }

    public class ApproveLoanByCommitteeResponse
    {
        public int CommitteeApprovalId { get; set; }
        public string ApprovalRefNumber { get; set; }
        public string CommitteeDecision { get; set; }
        public DateTime CommitteeDecisionDate { get; set; }
        public string Message { get; set; }
    }
}
