namespace FinTech.Core.Application.DTOs.Loans
{
    public class DisbursementRequest
    {
        public string LoanApplicationId { get; set; }
        public string DisbursedBy { get; set; }
        public string BankTransferReference { get; set; }
        public string Notes { get; set; }
    }
}
