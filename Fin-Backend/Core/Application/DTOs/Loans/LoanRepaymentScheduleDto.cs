namespace FinTech.Core.Application.DTOs.Loans
{
    public class LoanRepaymentScheduleDto
    {
        public string? Id { get; set; }
        public string? LoanId { get; set; }
        public int InstallmentNumber { get; set; }
        public DateTime DueDate { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal FeesAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal OutstandingAmount { get; set; }
        public string? Status { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
}
