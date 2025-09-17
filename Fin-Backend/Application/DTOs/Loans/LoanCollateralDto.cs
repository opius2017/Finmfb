using System;

public class LoanCollateralDto
{
    public string? Id { get; set; }
    public string? LoanApplicationId { get; set; }
    public string? LoanId { get; set; }
    public string? CollateralType { get; set; }
    public string? Description { get; set; }
    public decimal Value { get; set; }
    public DateTime ValuationDate { get; set; }
    public string? ValuationMethod { get; set; }
    public string? ValuedBy { get; set; }
    public string? OwnerName { get; set; }
    public string? OwnerRelationship { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? Location { get; set; }
    public bool IsInsured { get; set; }
    public string? InsuranceCompany { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    public DateTime? InsuranceExpiryDate { get; set; }
    public string? VerificationStatus { get; set; }
    public DateTime? VerificationDate { get; set; }
    public string? VerifiedBy { get; set; }
    public string? Notes { get; set; }
}
