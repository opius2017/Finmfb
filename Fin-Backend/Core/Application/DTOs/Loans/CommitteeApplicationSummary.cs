using System;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class CommitteeApplicationSummary
    {
        public string ApplicationId { get; set; }
        public string ApplicationNumber { get; set; }
        public string ApplicantName { get; set; }
        public decimal RequestedAmount { get; set; }
        public string LoanPurpose { get; set; }
        public DateTime ApplicationDate { get; set; }
        public int ReviewCount { get; set; }
        public bool HasReviewed { get; set; }
        public string Status { get; set; }
    }
}
