using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Loans;

namespace FinTech.Core.Application.Services
{
    /// <summary>
    /// Service for managing guarantor operations
    /// </summary>
    public interface IGuarantorService
    {
        /// <summary>
        /// Validate if a member is eligible to be a guarantor
        /// </summary>
        Task<GuarantorEligibilityResult> ValidateGuarantorEligibilityAsync(Guid memberId, decimal guaranteedAmount);
        
        /// <summary>
        /// Request consent from a guarantor
        /// </summary>
        Task<GuarantorConsent> RequestConsentAsync(GuarantorConsentRequest request);
        
        /// <summary>
        /// Approve guarantor consent
        /// </summary>
        Task<GuarantorConsent> ApproveConsentAsync(string consentToken, string notes = null);
        
        /// <summary>
        /// Decline guarantor consent
        /// </summary>
        Task<GuarantorConsent> DeclineConsentAsync(string consentToken, string reason);
        
        /// <summary>
        /// Lock guarantor equity when loan is approved
        /// </summary>
        Task LockGuarantorEquityAsync(Guid guarantorId, decimal amount, Guid loanId);
        
        /// <summary>
        /// Release guarantor equity when loan is closed
        /// </summary>
        Task ReleaseGuarantorEquityAsync(Guid guarantorId, decimal amount, Guid loanId);
        
        /// <summary>
        /// Get all guarantor obligations for a member
        /// </summary>
        Task<List<GuarantorObligation>> GetGuarantorObligationsAsync(Guid memberId);
        
        /// <summary>
        /// Get pending consent requests for a guarantor
        /// </summary>
        Task<List<GuarantorConsent>> GetPendingConsentRequestsAsync(Guid guarantorMemberId);
        
        /// <summary>
        /// Check and expire old consent requests
        /// </summary>
        Task ExpireOldConsentRequestsAsync();
    }
    
    /// <summary>
    /// Request to create a guarantor consent
    /// </summary>
    public class GuarantorConsentRequest
    {
        public Guid ApplicationId { get; set; }
        public Guid GuarantorMemberId { get; set; }
        public Guid ApplicantMemberId { get; set; }
        public decimal GuaranteedAmount { get; set; }
        public string Message { get; set; }
    }
    
    /// <summary>
    /// Result of guarantor eligibility check
    /// </summary>
    public class GuarantorEligibilityResult
    {
        public bool IsEligible { get; set; }
        public List<string> Reasons { get; set; } = new List<string>();
        public decimal FreeEquity { get; set; }
        public decimal LockedEquity { get; set; }
        public decimal RequiredEquity { get; set; }
        public int ActiveGuaranteeCount { get; set; }
        
        public void AddReason(string reason)
        {
            Reasons.Add(reason);
            IsEligible = false;
        }
    }
    
    /// <summary>
    /// Guarantor obligation details
    /// </summary>
    public class GuarantorObligation
    {
        public Guid LoanId { get; set; }
        public string LoanNumber { get; set; }
        public string BorrowerName { get; set; }
        public decimal GuaranteedAmount { get; set; }
        public decimal OutstandingAmount { get; set; }
        public string LoanStatus { get; set; }
        public DateTime GuaranteeDate { get; set; }
        public bool IsActive { get; set; }
    }
}
