using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class GuarantorDashboard
    {
        public string MemberId { get; set; }
        public string MemberName { get; set; }
        public decimal TotalSavings { get; set; }
        public decimal FreeEquity { get; set; }
        public decimal LockedEquity { get; set; }
        
        public List<GuarantorObligation> ActiveObligations { get; set; } = new List<GuarantorObligation>();
        public List<GuarantorObligation> PendingRequests { get; set; } = new List<GuarantorObligation>();
        public List<GuarantorObligation> CompletedObligations { get; set; } = new List<GuarantorObligation>();
    }
}
