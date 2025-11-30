namespace FinTech.Core.Application.DTOs.Loans
{
    public class RepaymentRequest
    {
        public string LoanId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } // CASH, BANK_TRANSFER, PAYROLL_DEDUCTION
        public string TransactionReference { get; set; }
        public string ProcessedBy { get; set; }
        public string Notes { get; set; }
    }
}
