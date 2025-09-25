public class CreateLoanApplicationDto
{
    public string? LoanProductId { get; set; }
    public string? CustomerId { get; set; }
    public decimal RequestedAmount { get; set; }
    public int RequestedTerm { get; set; }
    public string? Purpose { get; set; }
    public string? PurposeDescription { get; set; }
    public string? Notes { get; set; }
}
