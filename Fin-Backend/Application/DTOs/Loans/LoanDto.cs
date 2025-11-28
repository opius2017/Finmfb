using System;

public class LoanDto
{
    public string? Id { get; set; }
    public string? LoanNumber { get; set; }
    public string? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? LoanProductId { get; set; }
    public string? LoanProductName { get; set; }
    public string? LoanApplicationId { get; set; }
    public decimal PrincipalAmount { get; set; }
    public decimal DisbursedAmount { get; set; }
    public decimal InterestRate { get; set; }
    public int Term { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? MaturityDate { get; set; }
    public DateTime? DisbursementDate { get; set; }
    public string? Status { get; set; }
    public string? RepaymentFrequency { get; set; }
    public decimal PrincipalOutstanding { get; set; }
    public decimal InterestOutstanding { get; set; }
    public decimal FeesOutstanding { get; set; }
    public decimal PenaltiesOutstanding { get; set; }
    public decimal TotalOutstanding { get; set; }
    public decimal PrincipalPaid { get; set; }
    public decimal InterestPaid { get; set; }
    public decimal FeesPaid { get; set; }
    public decimal PenaltiesPaid { get; set; }
    public decimal TotalPaid { get; set; }
    public int? DaysOverdue { get; set; }
    public decimal? OverdueAmount { get; set; }
    public DateTime? NextPaymentDueDate { get; set; }
    public decimal? NextPaymentAmount { get; set; }
}
