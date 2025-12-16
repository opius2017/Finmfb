using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Entities.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTech.Core.Domain.Entities.ClientPortal
{


    // ClientPortalActivity moved to ClientPortalActivity.cs

    /// Represents saved payee for quick payments
    /// </summary>
    public class SavedPayee : BaseEntity, IAuditable
    {
        public Guid ClientPortalProfileId { get; set; }
        [Required]
        public Guid UserId { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string PayeeName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string PayeeType { get; set; } = string.Empty; // Individual, Business, Utility, etc.
        
        [MaxLength(34)]
        public string? AccountNumber { get; set; }
        
        [MaxLength(11)]
        public string? RoutingNumber { get; set; }
        
        [MaxLength(500)]
        public string? Address { get; set; }
        
        [MaxLength(100)]
        public string? Email { get; set; }
        
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        [MaxLength(500)]
        public string? Notes { get; set; }
        
        public DateTime LastUsed { get; set; }
        
        public int UsageCount { get; set; }

        // FinTech Best Practice: Add missing properties for payment processing
        [MaxLength(200)]
        public string? Name { get; set; }
        
        [MaxLength(200)]
        public string? BankName { get; set; }
        
        [MaxLength(20)]
        public string? BankCode { get; set; }
        
        public bool IsFavorite { get; set; } = false;

        [NotMapped]
        public Guid CustomerId { get { return UserId; } set { UserId = value; } }

        public Guid? BillerId { get; set; }

        [MaxLength(100)]
        public string? CustomerReferenceNumber { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        public virtual ClientPortalProfile? ClientPortalProfile { get; set; }
        [NotMapped] public string? Reference { get => CustomerReferenceNumber; set => CustomerReferenceNumber = value; }
    }

    /// <summary>
    /// Represents saved transfer templates for recurring transfers
    /// </summary>
    public class SavedTransferTemplate : BaseEntity, IAuditable
    {
        [Required]
        public Guid UserId { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string TemplateName { get; set; } = string.Empty;
        
        [Required]
        public Guid FromAccountId { get; set; }
        
        [Required]
        public Guid ToAccountId { get; set; }
        
        public decimal? DefaultAmount { get; set; }
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [MaxLength(50)]
        public string? TransferFrequency { get; set; } // One-time, Weekly, Monthly, etc.
        
        public DateTime? NextScheduledDate { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public bool IsRecurring { get; set; }
        
        public int UsageCount { get; set; }
        
        public DateTime LastUsed { get; set; }

        [NotMapped]
        public Guid CustomerId { get { return UserId; } set { UserId = value; } }

        [MaxLength(50)]
        public string? SourceAccountNumber { get; set; }

        [MaxLength(50)]
        public string? DestinationAccountNumber { get; set; }

        [MaxLength(100)]
        public string? DestinationBankName { get; set; }

        [MaxLength(20)]
        public string? DestinationBankCode { get; set; }

        [MaxLength(200)]
        public string? BeneficiaryName { get; set; }

        [NotMapped]
        public decimal Amount { get { return DefaultAmount ?? 0; } set { DefaultAmount = value; } }

        [MaxLength(50)]
        public string? TransferType { get; set; }

        public Guid ClientPortalProfileId { get; set; }
        public virtual ClientPortalProfile? ClientPortalProfile { get; set; }
        [NotMapped] public string? FromAccountNumber { get => SourceAccountNumber; set => SourceAccountNumber = value; }
        [NotMapped] public string? ToAccountNumber { get => DestinationAccountNumber; set => DestinationAccountNumber = value; }
        [NotMapped] public string? ToBankName { get => DestinationBankName; set => DestinationBankName = value; }
        [NotMapped] public string? ToBankCode { get => DestinationBankCode; set => DestinationBankCode = value; }
        public string Currency { get; set; } = "NGN";
        public string Reference { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents client documents uploaded to portal
    /// </summary>
    public class ClientDocument : BaseEntity, IAuditable
    {
        public Guid ClientPortalProfileId { get; set; }
        [Required]
        public Guid UserId { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string DocumentName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string DocumentType { get; set; } = string.Empty; // ID, Statement, Contract, etc.
        
        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string FileExtension { get; set; } = string.Empty;
        
        public long FileSizeBytes { get; set; }
        
        [MaxLength(100)]
        public string? MimeType { get; set; }
        
        public bool IsVerified { get; set; }
        
        public DateTime? VerifiedDate { get; set; }
        
        public Guid? VerifiedBy { get; set; }
        
        public DateTime ExpiryDate { get; set; }
        
        public bool IsExpired { get; set; }
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Verified, Rejected, Expired

        public virtual ClientPortalProfile? ClientPortalProfile { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string StorageProvider { get; set; } = string.Empty;
        public string StorageReference { get; set; } = string.Empty;
    }
}