namespace FinTech.Core.Application.DTOs.Loans;

public class LoanRegisterEntryDto
{
    public Guid Id { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string LoanNumber { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;
    public string MemberNumber { get; set; } = string.Empty;
    public decimal LoanAmount { get; set; }
    public decimal InterestRate { get; set; }
    public int TenureMonths { get; set; }
    public DateTime DisbursementDate { get; set; }
    public DateTime MaturityDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal OutstandingBalance { get; set; }
    public string? Purpose { get; set; }
    public string? Collateral { get; set; }
    public DateTime RegisteredAt { get; set; }
    public string RegisteredBy { get; set; } = string.Empty;
}
