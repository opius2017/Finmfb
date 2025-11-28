namespace FinTech.Core.Application.DTOs.Loans
{
    public class LoanApplicationRequest
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public int Term { get; set; }
        public string Purpose { get; set; }

        public LoanApplicationRequest()
        {
            Currency = string.Empty;
            Purpose = string.Empty;
        }
    }
}
