using System;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class CommitteeReviewSummary
    {
        public string ReviewId { get; set; }
        public string ReviewerName { get; set; }
        public string ReviewerMemberId { get; set; }
        public string Decision { get; set; }
        public decimal? RecommendedAmount { get; set; }
        public string Comments { get; set; }
        public int VotingWeight { get; set; }
        public DateTime ReviewedAt { get; set; }
    }
}
