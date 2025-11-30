using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class SavingsMultiplierCheckResult
    {
        public bool IsEligible { get; set; }
        public List<string> Reasons { get; set; } = new List<string>();
        
        public decimal RequiredMultiplier { get; set; }
        public decimal RequiredSavings { get; set; }
        public decimal ActualSavings { get; set; }
        public decimal MaximumEligibleAmount { get; set; }
    }
}
