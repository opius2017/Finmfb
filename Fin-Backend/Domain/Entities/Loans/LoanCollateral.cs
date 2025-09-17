using System;
using FinTech.Domain.Common;

namespace FinTech.Domain.Entities.Loans
{
    /// <summary>
    /// Represents a collateral for a loan
    /// </summary>
    public class LoanCollateral : AuditableEntity
    {
        public string Id { get; set; }
        public string LoanId { get; set; }
        public string CollateralType { get; set; }
        public string Description { get; set; }
        public decimal ValueAmount { get; set; }
        public string Currency { get; set; }
        public decimal LoanToValueRatio { get; set; }
        public string ValuationMethod { get; set; }
        public DateTime ValuationDate { get; set; }
        public string ValuedBy { get; set; }
        public DateTime ExpiryDate { get; set; }
        public CollateralStatus Status { get; set; }
        public string Location { get; set; }
        public string OwnerName { get; set; }
        public string OwnerRelationshipToClient { get; set; }
        public string RegistrationNumber { get; set; }
        public bool IsInsured { get; set; }
        public string InsurancePolicyNumber { get; set; }
        public string InsuranceCompany { get; set; }
        public DateTime? InsuranceExpiryDate { get; set; }
        public string Notes { get; set; }
        
        // Navigation property
        public virtual Loan Loan { get; set; }
        public virtual ICollection<LoanCollateralDocument> Documents { get; set; }
    }
    
    public enum CollateralStatus
    {
        Pending,
        Accepted,
        Rejected,
        Released,
        Liquidated
    }
}