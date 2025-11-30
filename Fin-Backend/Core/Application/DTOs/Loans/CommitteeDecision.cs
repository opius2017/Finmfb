namespace FinTech.Core.Application.DTOs.Loans
{
    public class CommitteeDecision
    {
        public string LoanApplicationId { get; set; }
        public string OverallDecision { get; set; } // APPROVED, REJECTED, PENDING
        public bool IsFinal { get; set; }
        public string Reason { get; set; }
        public int TotalReviews { get; set; }
        public int ApprovalVotes { get; set; }
        public int RejectionVotes { get; set; }
        public decimal ApprovalPercentage { get; set; }
        public decimal? RecommendedAmount { get; set; }
    }
}
