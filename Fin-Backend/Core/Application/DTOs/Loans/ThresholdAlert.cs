namespace FinTech.Core.Application.DTOs.Loans
{
    public class ThresholdAlert
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string Severity { get; set; } // WARNING, CRITICAL
        public string Message { get; set; }
        public decimal RemainingAmount { get; set; }
    }
}
