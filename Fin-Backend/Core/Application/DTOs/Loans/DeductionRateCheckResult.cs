using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class DeductionRateCheckResult
    {
        public bool IsEligible { get; set; }
        public List<string> Reasons { get; set; } = new List<string>();
        
        public decimal NetSalary { get; set; }
        public decimal ExistingDeductions { get; set; }
        public decimal ProposedEMI { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal DeductionRatePercentage { get; set; }
        public decimal MaxAllowedRate { get; set; }
        public decimal RemainingHeadroom { get; set; }
        public decimal MaximumAffordableEMI { get; set; }
    }
}
