namespace FinTech.Core.Application.DTOs.Loans
{
    public class CreateLoanCollateralDto
    {
        public string? CollateralType { get; set; }
        public string? Description { get; set; }
        public decimal? Value { get; set; }
        public string? Owner { get; set; }
        public string? DocumentUrl { get; set; }
        public DateTime? ValuationDate { get; set; }
        public string? ValuedBy { get; set; }
        public string? Location { get; set; }
        public string? Notes { get; set; }
    }
}
