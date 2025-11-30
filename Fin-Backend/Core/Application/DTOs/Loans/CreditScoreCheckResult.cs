using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class CreditScoreCheckResult
    {
        public bool IsEligible { get; set; }
        public List<string> Reasons { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        
        public int CreditScore { get; set; }
        public int MinimumScore { get; set; }
    }
}
