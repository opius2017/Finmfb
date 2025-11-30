using System;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class RepaymentReceipt
    {
        public string ReceiptNumber { get; set; }
        public string TransactionId { get; set; }
        public string LoanSerialNumber { get; set; }
        public string MemberName { get; set; }
        public string MemberNumber { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal PrincipalPaid { get; set; }
        public decimal InterestPaid { get; set; }
        public decimal PenaltyPaid { get; set; }
        public decimal RemainingBalance { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionReference { get; set; }
        public string ReceiptUrl { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}
