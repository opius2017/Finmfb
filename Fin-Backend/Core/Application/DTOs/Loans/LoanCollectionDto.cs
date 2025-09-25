using System;

public class LoanCollectionDto
{
    public string? Id { get; set; }
    public string? LoanId { get; set; }
    public string? LoanNumber { get; set; }
    public string? CustomerName { get; set; }
    public DateTime OverdueDate { get; set; }
    public int DaysOverdue { get; set; }
    public decimal OverdueAmount { get; set; }
    public string? Status { get; set; }
    public string? AssignedTo { get; set; }
    public string? AssignedToName { get; set; }
    public DateTime? ResolutionDate { get; set; }
}
