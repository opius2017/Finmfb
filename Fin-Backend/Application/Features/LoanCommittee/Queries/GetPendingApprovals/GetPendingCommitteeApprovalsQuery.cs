using MediatR;
using FinTech.Core.Domain.Common;

namespace FinTech.Core.Application.Features.LoanCommittee.Queries.GetPendingApprovals
{
    /// <summary>
    /// Query to retrieve pending loan applications awaiting committee approval
    /// </summary>
    public class GetPendingCommitteeApprovalsQuery : IRequest<Result<PaginatedList<CommitteeApprovalDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string RiskRating { get; set; } // Filter by risk level
        public string Status { get; set; } // Filter by approval status
    }

    /// <summary>
    /// Query to get committee approval details by reference number
    /// </summary>
    public class GetCommitteeApprovalDetailQuery : IRequest<Result<CommitteeApprovalDetailDto>>
    {
        public string ApprovalRefNumber { get; set; }
    }

    public class CommitteeApprovalDto
    {
        public int Id { get; set; }
        public string ApprovalRefNumber { get; set; }
        public int LoanApplicationId { get; set; }
        public string MemberName { get; set; }
        public decimal RequestedAmount { get; set; }
        public string RiskRating { get; set; }
        public string Status { get; set; }
        public DateTime SubmittedToCommitteeDate { get; set; }
        public string CreditOfficerRecommendation { get; set; }
    }

    public class CommitteeApprovalDetailDto
    {
        public int Id { get; set; }
        public string ApprovalRefNumber { get; set; }
        public int LoanApplicationId { get; set; }
        public string MemberName { get; set; }
        public decimal RequestedAmount { get; set; }
        public string RiskRating { get; set; }
        public string Status { get; set; }
        public DateTime SubmittedToCommitteeDate { get; set; }
        public string ReferralReason { get; set; }
        public string CommitteeSummary { get; set; }
        public decimal? LoanRepaymentHistoryScore { get; set; }
        public int PreviousSuccessfulLoans { get; set; }
        public int PreviousDefaultCount { get; set; }
        public bool HasDefaultHistory { get; set; }
        public string Guarantor1Name { get; set; }
        public decimal? Guarantor1MemberSavings { get; set; }
        public string Guarantor2Name { get; set; }
        public decimal? Guarantor2MemberSavings { get; set; }
        public bool AllGuarantorsApproved { get; set; }
        public DateTime? CommitteeMeetingDate { get; set; }
        public string CommitteeReviewers { get; set; }
        public DateTime? CommitteeDecisionDate { get; set; }
        public string CommitteeDecision { get; set; }
        public string CommitteeNotes { get; set; }
        public string ApprovalConditions { get; set; }
        public string CreditOfficerRecommendation { get; set; }
    }
}
