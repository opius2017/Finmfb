using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Application.DTOs.Loans;

namespace FinTech.Application.Interfaces.Loans
{
    public interface ILoanCollateralService
    {
        Task<IEnumerable<LoanCollateralDto>> GetAllCollateralsAsync();
        Task<IEnumerable<LoanCollateralDto>> GetCollateralsByLoanIdAsync(string loanId);
        Task<LoanCollateralDto> AddCollateralAsync(string loanId, CreateLoanCollateralDto collateralDto);
    }
}
