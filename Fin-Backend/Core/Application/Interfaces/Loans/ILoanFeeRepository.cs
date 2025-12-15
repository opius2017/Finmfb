using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Loans;

namespace FinTech.Core.Application.Interfaces.Loans
{
    public interface ILoanFeeRepository
    {
        Task<IEnumerable<LoanFee>> GetByLoanIdAsync(string loanId);
        Task<LoanFee> AddAsync(LoanFee fee);
    }
}
