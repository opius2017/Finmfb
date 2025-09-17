using System;

public class CreateLoanRepaymentDto
{
    public string? LoanId { get; set; }
    public decimal Amount { get; set; }
    public string? Reference { get; set; }
    public string? Description { get; set; }
    public DateTime? PaymentDate { get; set; }
}
