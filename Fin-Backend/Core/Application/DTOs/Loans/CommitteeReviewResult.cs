using System;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class CommitteeReviewResult
    {
        public string ReviewId { get; set; }
        public string ApplicationId { get; set; }
        public string ReviewerName { get; set; }
        public string Decision { get; set; }
        public DateTime ReviewedAt { get; set; }
        public string OverallDecision { get; set; }
        public bool IsDecisionFinal { get; set; }
        public string Message { get; set; }
    }
}
