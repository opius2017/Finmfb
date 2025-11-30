namespace FinTech.Core.Application.DTOs.Loans
{
    public class ThresholdCheckResult
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal RequestedAmount { get; set; }
        public decimal MaxLoanAmount { get; set; }
        public decimal TotalDisbursed { get; set; }
        public decimal RemainingAmount { get; set; }
        public bool CanAccommodate { get; set; }
        public decimal Shortfall { get; set; }
        public (int Month, int Year)? SuggestedMonth { get; set; }
        public string Message { get; set; }
    }
}
