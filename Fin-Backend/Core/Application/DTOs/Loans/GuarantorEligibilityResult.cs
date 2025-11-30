using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class GuarantorEligibilityResult
    {
        public bool IsEligible { get; set; }
        public List<string> Reasons { get; set; } = new List<string>();
        
        public string GuarantorMemberId { get; set; }
        public string GuarantorName { get; set; }
        public decimal TotalSavings { get; set; }
        public decimal FreeEquity { get; set; }
        public decimal LockedEquity { get; set; }
        
        public List<GuarantorObligation> ExistingGuarantorObligations { get; set; } = new List<GuarantorObligation>();
    }
}
