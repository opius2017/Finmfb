using System;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class QueuedLoanApplicationDto
    {
        public Guid ApplicationId { get; set; }
        public string ApplicationNumber { get; set; } = string.Empty;
        public Guid MemberId { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public decimal RequestedAmount { get; set; }
        public DateTime ApprovedDate { get; set; }
        public DateTime QueuedDate { get; set; }
        public int QueuePosition { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
