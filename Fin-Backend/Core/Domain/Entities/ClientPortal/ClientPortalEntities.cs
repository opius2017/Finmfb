using FinTech.Core.Domain.Common;
using FinTech.Core.Domain.Entities.Common;
using System;
using System.ComponentModel.DataAnnotations;

namespace FinTech.Core.Domain.Entities.ClientPortal
{
    /// <summary>
    /// Represents client portal session tracking
    /// </summary>
    public class ClientPortalSession : BaseEntity, IAuditable
    {
        [Required]
        public Guid UserId { get; set; }
        
        [Required]
        [MaxLength(256)]
        public string SessionToken { get; set; } = string.Empty;
        
        [Required]
        public DateTime StartTime { get; set; }
        
        public DateTime? EndTime { get; set; }
        
        [MaxLength(45)]
        public string? IpAddress { get; set; }
        
        [MaxLength(512)]
        public string? UserAgent { get; set; }
        
        public bool IsActive { get; set; }
        
        [MaxLength(100)]
        public string? Location { get; set; }
        
        public DateTime? LastActivity { get; set; }
    }

    /// <summary>
    /// Represents client portal activity tracking
    /// </summary>
    public class ClientPortalActivity : BaseEntity, IAuditable
    {
        [Required]
        public Guid UserId { get; set; }
        
        [Required]
        public Guid SessionId { get; set; }
        public ClientPortalSession? Session { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string ActivityType { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [Required]
        public DateTime Timestamp { get; set; }
        
        [MaxLength(45)]
        public string? IpAddress { get; set; }
        
        public bool IsSuccessful { get; set; }
        
        [MaxLength(1000)]
        public string? AdditionalData { get; set; }
    }

    /// <summary>
    /// Represents saved payee for quick payments
    /// </summary>
    public class SavedPayee : BaseEntity, IAuditable
    {
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
    }

    /// <summary>
    /// Represents client documents uploaded to portal
    /// </summary>
    public class ClientDocument : BaseEntity, IAuditable
    {
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
    }
}