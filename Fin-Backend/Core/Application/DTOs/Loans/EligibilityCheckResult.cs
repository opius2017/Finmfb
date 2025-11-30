using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Loans
{
    /// <summary>
    /// Result of comprehensive eligibility check
    /// </summary>
    public class EligibilityCheckResult
    {
        public bool IsEligible { get; set; }
        public List<string> Reasons { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        
        public decimal MaximumEligibleAmount { get; set; }
        public decimal RecommendedAmount { get; set; }

        // Individual check results
        public SavingsMultiplierCheckResult SavingsMultiplierCheck { get; set; }
        public MembershipDurationCheckResult MembershipDurationCheck { get; set; }
        public DeductionRateCheckResult DeductionRateCheck { get; set; }
        public DebtToIncomeCheckResult DebtToIncomeCheck { get; set; }
        public ExistingLoanCheckResult ExistingLoanCheck { get; set; }
        public CreditScoreCheckResult CreditScoreCheck { get; set; }
    }
}
