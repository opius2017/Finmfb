public class LoanProductFeeDto
{
    public string? Id { get; set; }
    public string? LoanProductId { get; set; }
    public string? FeeType { get; set; }
    public string? FeeName { get; set; }
    public decimal Amount { get; set; }
    public bool IsPercentage { get; set; }
    public bool IsActive { get; set; }
}
