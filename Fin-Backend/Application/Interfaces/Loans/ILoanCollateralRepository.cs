using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.Loans;

namespace FinTech.Application.Interfaces.Loans
{
    public interface ILoanCollateralRepository
    {
        Task<IEnumerable<LoanCollateral>> GetCollateralsByLoanIdAsync(string loanId);
        Task AddCollateralAsync(LoanCollateral collateral);
    }
}
