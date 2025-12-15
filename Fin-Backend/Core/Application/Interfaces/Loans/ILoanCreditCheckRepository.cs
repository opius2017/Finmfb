using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Loans;

namespace FinTech.Core.Application.Interfaces.Loans
{
    public interface ILoanCreditCheckRepository
    {
        Task<IEnumerable<LoanCreditCheck>> GetByLoanIdAsync(string loanId);
        Task<LoanCreditCheck> AddAsync(LoanCreditCheck creditCheck);
        Task<IEnumerable<LoanCreditCheck>> GetCreditChecksByStatusAsync(CreditCheckResult status);
        Task<IEnumerable<LoanCreditCheck>> GetByCustomerIdAsync(string customerId);
        Task<LoanCreditCheck> UpdateAsync(LoanCreditCheck creditCheck);
        Task<IEnumerable<LoanCreditCheck>> GetAllAsync();
    }
}
