using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Loans;

namespace FinTech.Application.Interfaces.Loans
{
    public interface ILoanCreditCheckRepository
    {
        Task<IEnumerable<LoanCreditCheck>> GetCreditChecksByLoanIdAsync(string loanId);
        Task AddCreditCheckAsync(LoanCreditCheck creditCheck);
    }
}
