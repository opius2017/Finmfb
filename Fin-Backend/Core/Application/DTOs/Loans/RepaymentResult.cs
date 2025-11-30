using System;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class RepaymentResult
    {
        public bool Success { get; set; }
        public string TransactionId { get; set; }
        public string LoanId { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal PrincipalPaid { get; set; }
        public decimal InterestPaid { get; set; }
        public decimal PenaltyPaid { get; set; }
        public decimal RemainingBalance { get; set; }
        public DateTime? NextPaymentDate { get; set; }
        public decimal NextPaymentAmount { get; set; }
        public string ReceiptUrl { get; set; }
        public bool IsLoanFullyPaid { get; set; }
        public string Message { get; set; }
    }
}
