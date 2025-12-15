using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Loans;

namespace FinTech.Core.Application.Interfaces.Loans
{
    public interface ILoanCommitteeService
    {
        Task<CommitteeReviewDto> CreateReviewAsync(CreateCommitteeReviewRequest request);
        Task<MemberCreditProfileDto> GetMemberCreditProfileAsync(string memberId);
        Task<RepaymentScoreDto> CalculateRepaymentScoreAsync(string memberId);
        Task<CommitteeReviewDto> SubmitReviewDecisionAsync(SubmitReviewDecisionRequest request);
        Task<List<CommitteeReviewDto>> GetPendingReviewsAsync();
        Task<CommitteeReviewDto> GetReviewByIdAsync(string reviewId);
        Task<CommitteeDashboardDto> GetCommitteeDashboardAsync();
    }
}
