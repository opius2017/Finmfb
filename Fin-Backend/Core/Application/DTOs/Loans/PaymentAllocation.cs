namespace FinTech.Core.Application.DTOs.Loans
{
    /// <summary>
    /// Represents how a payment is allocated across different components
    /// </summary>
    public class PaymentAllocation
    {
        public decimal TotalPayment { get; set; }
        public decimal PenaltyPaid { get; set; }
        public decimal InterestPaid { get; set; }
        public decimal PrincipalPaid { get; set; }
        public decimal Overpayment { get; set; }
    }
}
