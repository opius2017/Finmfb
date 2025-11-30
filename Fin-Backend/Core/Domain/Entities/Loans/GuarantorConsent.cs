using System;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Core.Domain.Entities.Loans
{
    /// <summary>
    /// Represents a guarantor consent request and response
    /// </summary>
    public class GuarantorConsent : AuditableEntity
    {
        public Guid ApplicationId { get; private set; }
        public Guid GuarantorMemberId { get; private set; }
        public Guid ApplicantMemberId { get; private set; }
        public decimal GuaranteedAmount { get; private set; }
        public string ConsentToken { get; private set; }
        public GuarantorConsentStatus Status { get; private set; }
        public DateTime RequestedAt { get; private set; }
        public DateTime? RespondedAt { get; private set; }
        public DateTime? ExpiresAt { get; private set; }
        public string DeclineReason { get; private set; }
        public string Notes { get; private set; }
        
        // Navigation properties
        public virtual LoanApplication Application { get; private set; }
        public virtual Member GuarantorMember { get; private set; }
        public virtual Member ApplicantMember { get; private set; }
        
        private GuarantorConsent() { } // For EF Core
        
        public GuarantorConsent(
            Guid applicationId,
            Guid guarantorMemberId,
            Guid applicantMemberId,
            decimal guaranteedAmount,
            int expiryHours = 72)
        {
            ApplicationId = applicationId;
            GuarantorMemberId = guarantorMemberId;
            ApplicantMemberId = applicantMemberId;
            GuaranteedAmount = guaranteedAmount;
            ConsentToken = GenerateConsentToken();
            Status = GuarantorConsentStatus.Pending;
            RequestedAt = DateTime.UtcNow;
            ExpiresAt = DateTime.UtcNow.AddHours(expiryHours);
        }
        
        /// <summary>
        /// Approve the guarantor consent
        /// </summary>
        public void Approve(string? notes = null)
        {
            if (Status != GuarantorConsentStatus.Pending)
                throw new InvalidOperationException($"Cannot approve consent in {Status} status");
            
            if (DateTime.UtcNow > ExpiresAt)
                throw new InvalidOperationException("Consent request has expired");
            
            Status = GuarantorConsentStatus.Approved;
            RespondedAt = DateTime.UtcNow;
            Notes = notes;
        }
        
        /// <summary>
        /// Decline the guarantor consent
        /// </summary>
        public void Decline(string reason)
        {
            if (Status != GuarantorConsentStatus.Pending)
                throw new InvalidOperationException($"Cannot decline consent in {Status} status");
            
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Decline reason is required", nameof(reason));
            
            Status = GuarantorConsentStatus.Declined;
            RespondedAt = DateTime.UtcNow;
            DeclineReason = reason;
        }
        
        /// <summary>
        /// Mark consent as expired
        /// </summary>
        public void MarkAsExpired()
        {
            if (Status != GuarantorConsentStatus.Pending)
                return;
            
            if (DateTime.UtcNow > ExpiresAt)
            {
                Status = GuarantorConsentStatus.Expired;
            }
        }
        
        /// <summary>
        /// Revoke approved consent (before loan disbursement)
        /// </summary>
        public void Revoke(string reason)
        {
            if (Status != GuarantorConsentStatus.Approved)
                throw new InvalidOperationException("Can only revoke approved consents");
            
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Revoke reason is required", nameof(reason));
            
            Status = GuarantorConsentStatus.Revoked;
            DeclineReason = reason;
        }
        
        /// <summary>
        /// Check if consent is valid
        /// </summary>
        public bool IsValid()
        {
            return Status == GuarantorConsentStatus.Approved && 
                   DateTime.UtcNow <= ExpiresAt;
        }
        
        /// <summary>
        /// Generate a unique consent token
        /// </summary>
        private string GenerateConsentToken()
        {
            return $"GC-{Guid.NewGuid():N}".ToUpper();
        }
    }
    
    /// <summary>
    /// Guarantor consent status
    /// </summary>
    public enum GuarantorConsentStatus
    {
        Pending = 1,
        Approved = 2,
        Declined = 3,
        Expired = 4,
        Revoked = 5
    }
}
