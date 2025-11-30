namespace FinTech.Core.Application.DTOs.Loans
{
    public class MonthlyThresholdResult
    {
        public string ThresholdId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal MaxLoanAmount { get; set; }
        public decimal TotalDisbursed { get; set; }
        public decimal RemainingAmount { get; set; }
        public string Message { get; set; }
    }
}
