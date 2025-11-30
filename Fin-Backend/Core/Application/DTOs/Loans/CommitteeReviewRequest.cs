namespace FinTech.Core.Application.DTOs.Loans
{
    public class CommitteeReviewRequest
    {
        public string LoanApplicationId { get; set; }
        public string ReviewerMemberId { get; set; }
        public string Decision { get; set; } // APPROVE, REJECT, REQUEST_MORE_INFO
        public decimal? RecommendedAmount { get; set; }
        public string Comments { get; set; }
        public int VotingWeight { get; set; } = 1;
    }
}
