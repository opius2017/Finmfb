using System;
using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Loans
{
    /// <summary>
    /// Represents a guarantor for a loan
    /// </summary>
    public class LoanGuarantor : AuditableEntity
    {
        public string LoanId { get; set; } = string.Empty;
        public string GuarantorType { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty;
        public string Occupation { get; set; } = string.Empty;
        public string Employer { get; set; } = string.Empty;
        public string IdentificationType { get; set; } = string.Empty;
        public string IdentificationNumber { get; set; } = string.Empty;
        public DateTime? IdentificationExpiryDate { get; set; }
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public decimal MonthlyIncome { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string BankAccountNumber { get; set; } = string.Empty;
        public string BVN { get; set; } = string.Empty;
        public DateTime GuaranteeDate { get; set; }
        public decimal GuaranteedAmount { get; set; }
        public GuarantorStatus Status { get; set; }
        public DateTime? VerificationDate { get; set; }
        public string VerifiedBy { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        // Missing properties required by Configuration/Repository
        public string GuarantorCustomerId { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
        public string ApprovedBy { get; set; } = string.Empty;
        public string RejectedBy { get; set; } = string.Empty;
        public string RejectionReason { get; set; } = string.Empty;
        
        // Navigation property
        public virtual Loan Loan { get; set; }
        public virtual ICollection<LoanGuarantorDocument> Documents { get; set; }
    }
    
    public enum GuarantorStatus
    {
        Pending,
        Verified,
        Rejected,
        Released
    }
}
