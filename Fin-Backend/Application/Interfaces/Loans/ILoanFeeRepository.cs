using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Loans;

namespace FinTech.Core.Application.Interfaces.Loans
{
    public interface ILoanFeeRepository
    {
        Task<IEnumerable<LoanFee>> GetFeesByLoanIdAsync(string loanId);
        Task AddFeeAsync(LoanFee fee);
    }
}
