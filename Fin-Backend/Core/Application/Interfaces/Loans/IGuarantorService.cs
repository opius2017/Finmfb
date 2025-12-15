using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Loans;
using FinTech.Core.Application.DTOs.Loans;

namespace FinTech.Core.Application.Interfaces.Loans
{
    /// <summary>
    /// Service for managing guarantor operations
    /// </summary>
    public interface IGuarantorService
    {
        Task<GuarantorDto> AddGuarantorAsync(AddGuarantorRequest request);
        Task<GuarantorEligibilityDto> CheckGuarantorEligibilityAsync(string memberId, decimal guaranteeAmount);
        Task<ConsentRequestDto> SendConsentRequestAsync(string guarantorId, string loanApplicationId);
        Task<GuarantorDto> ProcessConsentAsync(ProcessConsentRequest request);
        Task<EquityLockDto> LockGuarantorEquityAsync(string guarantorId, string loanId, decimal amount);
        Task<EquityLockDto> UnlockGuarantorEquityAsync(string guarantorId, string loanId);
        Task<GuarantorDashboardDto> GetGuarantorDashboardAsync(string memberId);
        Task<List<GuarantorDto>> GetLoanGuarantorsAsync(string loanApplicationId);
        Task<List<GuaranteedLoanDto>> GetGuaranteedLoansAsync(string memberId);
        Task<bool> RemoveGuarantorAsync(string guarantorId);
    }
}
