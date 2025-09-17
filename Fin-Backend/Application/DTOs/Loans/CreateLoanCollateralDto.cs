namespace FinTech.Application.DTOs.Loans
{
    public class CreateLoanCollateralDto
    {
        public string? CollateralType { get; set; }
        public string? Description { get; set; }
        public decimal? Value { get; set; }
        public string? Owner { get; set; }
        public string? DocumentUrl { get; set; }
    }
}