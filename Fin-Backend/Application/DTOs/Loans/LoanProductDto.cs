using System.Collections.Generic;

public class LoanProductDto
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? Currency { get; set; }
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public int MinTerm { get; set; }
    public int MaxTerm { get; set; }
    public decimal InterestRate { get; set; }
    public string? InterestCalculationMethod { get; set; }
    public string? RepaymentFrequency { get; set; }
    public int GracePeriod { get; set; }
    public bool AllowEarlyRepayment { get; set; }
    public decimal? EarlyRepaymentFeePercentage { get; set; }
    public bool IsSecured { get; set; }
    public decimal CollateralCoverageRatio { get; set; }
    public string? CustomerSegment { get; set; }
    public bool RequiresGuarantor { get; set; }
    public int MinNumberOfGuarantors { get; set; }
    public bool IsActive { get; set; }
    public List<LoanProductFeeDto> Fees { get; set; } = new List<LoanProductFeeDto>();
    public List<LoanProductDocumentDto> RequiredDocuments { get; set; } = new List<LoanProductDocumentDto>();
}
