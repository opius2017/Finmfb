namespace FinTech.Core.Application.DTOs.Loans
{
    /// <summary>
    /// Represents the impact of a loan on deduction rate headroom
    /// </summary>
    public class DeductionRateImpact
    {
        public decimal NetSalary { get; set; }
        public decimal ExistingDeductions { get; set; }
        public decimal ProposedEMI { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal DeductionRatePercentage { get; set; }
        public decimal MaxAllowedDeductionRate { get; set; }
        public decimal RemainingHeadroom { get; set; }
        public bool IsWithinLimit { get; set; }
    }
}
