using System;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Loans
{
    /// <summary>
    /// Represents a document associated with a loan
    /// </summary>
    public class LoanDocument : AuditableEntity
    {
    public new string? Id { get; set; }
    public string? LoanId { get; set; }
    public string? DocumentType { get; set; }
    public string? DocumentName { get; set; }
    public string? FileName { get; set; } // Added to match config
    public string? Description { get; set; }
    public string? FilePath { get; set; }
    public string? FileType { get; set; }
    public string? ContentType { get; set; } // Added to match config
    public long FileSize { get; set; }
    public DocumentStatus Status { get; set; }
    public DateTime UploadDate { get; set; }
    public string? UploadedBy { get; set; }
    public DateTime? VerificationDate { get; set; }
    public string? VerifiedBy { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Notes { get; set; }
    public string? RejectedBy { get; set; } // Added to match config
    public string? RejectionReason { get; set; } // Added to match config
        
        // Navigation property
    public virtual Loan? Loan { get; set; }
    }
    
    public enum DocumentStatus
    {
        Pending,
        Verified,
        Rejected,
        Expired
    }
}

namespace FinTech.Core.Domain.Entities.Loans
{
    /// <summary>
    /// Represents a document associated with a loan collateral
    /// </summary>
    public class LoanCollateralDocument : AuditableEntity
    {
    public new string? Id { get; set; }
    public string? CollateralId { get; set; }
    public string? DocumentType { get; set; }
    public string? DocumentName { get; set; }
    public string? Description { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DocumentStatus Status { get; set; }
        public DateTime UploadDate { get; set; }
        public string UploadedBy { get; set; } = string.Empty;
        public DateTime? VerificationDate { get; set; }
        public string VerifiedBy { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        
        // Navigation property
        public virtual LoanCollateral Collateral { get; set; }
    }
}

namespace FinTech.Core.Domain.Entities.Loans
{
    /// <summary>
    /// Represents a document associated with a loan guarantor
    /// </summary>
    public class LoanGuarantorDocument : AuditableEntity
    {
        public new string Id { get; set; } = Guid.NewGuid().ToString();
        public string GuarantorId { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DocumentStatus Status { get; set; }
        public DateTime UploadDate { get; set; }
        public string UploadedBy { get; set; } = string.Empty;
        public DateTime? VerificationDate { get; set; }
        public string VerifiedBy { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        
        // Navigation property
        public virtual LoanGuarantor Guarantor { get; set; }
    }
}

namespace FinTech.Core.Domain.Entities.Loans
{
    /// <summary>
    /// Represents a required document for a loan product
    /// </summary>
    public class LoanProductDocument : AuditableEntity
    {
        public new string Id { get; set; } = Guid.NewGuid().ToString();
        public string LoanProductId { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public string ApplicableFor { get; set; } = string.Empty; // Individual, Business, Group, All
        
        // Navigation property
        public virtual LoanProduct LoanProduct { get; set; }
    }
}
