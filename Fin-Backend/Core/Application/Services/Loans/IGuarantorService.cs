using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Loans;

namespace FinTech.Core.Application.Services.Loans
{
    /// <summary>
    /// Service for managing loan guarantors
    /// </summary>
    public interface IGuarantorService
    {
        /// <summary>
        /// Add guarantor to loan application
        /// </summary>
        Task<GuarantorDto> AddGuarantorAsync(AddGuarantorRequest request);

        /// <summary>
        /// Check guarantor eligibility (free equity validation)
        /// </summary>
        Task<GuarantorEligibilityDto> CheckGuarantorEligibilityAsync(string memberId, decimal guaranteeAmount);

        /// <summary>
        /// Send digital consent request to guarantor
        /// </summary>
        Task<ConsentRequestDto> SendConsentRequestAsync(string guarantorId, string loanApplicationId);

        /// <summary>
        /// Process guarantor consent (approve/reject)
        /// </summary>
        Task<GuarantorDto> ProcessConsentAsync(ProcessConsentRequest request);

        /// <summary>
        /// Lock guarantor equity for approved loan
        /// </summary>
        Task<EquityLockDto> LockGuarantorEquityAsync(string guarantorId, string loanId, decimal amount);

        /// <summary>
        /// Unlock guarantor equity when loan is closed
        /// </summary>
        Task<EquityLockDto> UnlockGuarantorEquityAsync(string guarantorId, string loanId);

        /// <summary>
        /// Get guarantor dashboard data
        /// </summary>
        Task<GuarantorDashboardDto> GetGuarantorDashboardAsync(string memberId);

        /// <summary>
        /// Get guarantors for a loan
        /// </summary>
        Task<List<GuarantorDto>> GetLoanGuarantorsAsync(string loanApplicationId);

        /// <summary>
        /// Get loans guaranteed by a member
        /// </summary>
        Task<List<GuaranteedLoanDto>> GetGuaranteedLoansAsync(string memberId);

        /// <summary>
        /// Remove guarantor from loan application
        /// </summary>
        Task<bool> RemoveGuarantorAsync(string guarantorId);
    }
}
