using System;

public class LoanTransactionDto
{
    public string? Id { get; set; }
    public string? LoanId { get; set; }
    public string? TransactionType { get; set; }
    public decimal PrincipalAmount { get; set; }
    public decimal InterestAmount { get; set; }
    public decimal FeesAmount { get; set; }
    public decimal PenaltyAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Reference { get; set; }
    public string? Description { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? Status { get; set; }
}
