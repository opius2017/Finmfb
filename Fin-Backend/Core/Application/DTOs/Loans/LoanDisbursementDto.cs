namespace FinTech.Core.Application.DTOs.Loans
{
    public class LoanDisbursementDto
    {
        public decimal Amount { get; set; }
        public string? DisbursedTo { get; set; }
        public string? Reference { get; set; }
        public string? Description { get; set; }
    }
}
