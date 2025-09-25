using System;

public class LoanGuarantorDto
{
    public string? Id { get; set; }
    public string? LoanApplicationId { get; set; }
    public string? LoanId { get; set; }
    public string? FullName { get; set; }
    public string? Relationship { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? IdentificationType { get; set; }
    public string? IdentificationNumber { get; set; }
    public DateTime? IdentificationExpiryDate { get; set; }
    public string? EmploymentStatus { get; set; }
    public string? Employer { get; set; }
    public decimal? MonthlyIncome { get; set; }
    public string? VerificationStatus { get; set; }
    public DateTime? VerificationDate { get; set; }
    public string? VerifiedBy { get; set; }
    public string? Notes { get; set; }
}
