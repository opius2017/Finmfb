namespace FinTech.Core.Application.DTOs.Loans
{
    public class MonthlyThresholdInfo
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal MaxLoanAmount { get; set; }
        public decimal TotalDisbursed { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal UtilizationPercentage { get; set; }
        public bool IsActive { get; set; }
        public int QueuedApplicationsCount { get; set; }
    }
}
