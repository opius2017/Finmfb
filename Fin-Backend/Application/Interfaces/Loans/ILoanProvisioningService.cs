using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Application.DTOs.Loans;

namespace FinTech.Application.Interfaces.Loans
{
    public interface ILoanProvisioningService
    {
        Task<LoanProvisioningDto> CalculateProvisioningAsync(string loanId);
        Task<IEnumerable<LoanProvisioningDto>> GetAllProvisioningsAsync();
        Task<LoanProvisioningDto> GetProvisioningByLoanIdAsync(string loanId);
    }
}
