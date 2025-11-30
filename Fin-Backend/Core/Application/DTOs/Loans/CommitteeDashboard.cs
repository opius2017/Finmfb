using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class CommitteeDashboard
    {
        public string CommitteeMemberId { get; set; }
        public int TotalPendingApplications { get; set; }
        public int ApplicationsReviewedByMe { get; set; }
        public int ApplicationsAwaitingMyReview { get; set; }
        
        public List<CommitteeApplicationSummary> PendingApplications { get; set; } = new List<CommitteeApplicationSummary>();
    }
}
