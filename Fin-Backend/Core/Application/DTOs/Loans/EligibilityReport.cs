using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class EligibilityReport
    {
        public bool IsEligible { get; set; }
        public string OverallStatus { get; set; }
        public DateTime GeneratedAt { get; set; }
        
        public List<string> Reasons { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public List<string> CheckDetails { get; set; } = new List<string>();
        
        public decimal MaximumEligibleAmount { get; set; }
        public decimal RecommendedAmount { get; set; }
    }
}
