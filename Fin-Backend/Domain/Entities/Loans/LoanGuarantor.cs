using System;
using FinTech.Domain.Common;

namespace FinTech.Domain.Entities.Loans
{
    /// <summary>
    /// Represents a guarantor for a loan
    /// </summary>
    public class LoanGuarantor : AuditableEntity
    {
        public string Id { get; set; }
        public string LoanId { get; set; }
        public string GuarantorType { get; set; }
        public string FullName { get; set; }
        public string Relationship { get; set; }
        public string Occupation { get; set; }
        public string Employer { get; set; }
        public string IdentificationType { get; set; }
        public string IdentificationNumber { get; set; }
        public DateTime? IdentificationExpiryDate { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public decimal MonthlyIncome { get; set; }
        public string BankName { get; set; }
        public string BankAccountNumber { get; set; }
        public string BVN { get; set; }
        public DateTime GuaranteeDate { get; set; }
        public decimal GuaranteedAmount { get; set; }
        public GuarantorStatus Status { get; set; }
        public DateTime? VerificationDate { get; set; }
        public string VerifiedBy { get; set; }
        public string Notes { get; set; }
        
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