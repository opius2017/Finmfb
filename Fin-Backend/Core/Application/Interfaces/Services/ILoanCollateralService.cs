using FinTech.Core.Domain.Entities.Loans;

namespace FinTech.Core.Application.Interfaces.Services;

public interface ILoanCollateralService
{
    Task<LoanCollateral?> GetByIdAsync(string id);
    Task<IEnumerable<LoanCollateral>> GetByLoanIdAsync(string loanId);
    Task<string> CreateAsync(LoanCollateral collateral);
    Task UpdateAsync(LoanCollateral collateral);
    Task DeleteAsync(string id);
    Task<decimal> GetTotalCollateralValueAsync(string loanId);
    Task<bool> ValidateCollateralCoverageAsync(string loanId, decimal loanAmount);
}
