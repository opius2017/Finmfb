using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.Interfaces.Repositories.Loans;
using FinTech.Core.Domain.Entities.Loans;
using Microsoft.Extensions.Logging;

namespace FinTech.Infrastructure.Repositories.Loans
{
    public class LoanCollateralRepository : ILoanCollateralRepository
    {
        private readonly ILogger<LoanCollateralRepository> _logger;
        private static readonly List<LoanCollateral> _collaterals = new();

        public LoanCollateralRepository(ILogger<LoanCollateralRepository> logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<LoanCollateral>> GetAllAsync()
        {
            return await Task.FromResult(_collaterals);
        }

        public async Task<LoanCollateral> GetByIdAsync(string id)
        {
            var collateral = _collaterals.Find(c => c.Id.ToString() == id);
            return await Task.FromResult(collateral);
        }

        public async Task<IEnumerable<LoanCollateral>> GetByLoanIdAsync(string loanId)
        {
            var collaterals = _collaterals.FindAll(c => c.LoanId == loanId);
            return await Task.FromResult(collaterals);
        }

        public async Task<LoanCollateral> AddAsync(LoanCollateral collateral)
        {
            _collaterals.Add(collateral);
            return await Task.FromResult(collateral);
        }

        public async Task<LoanCollateral> UpdateAsync(LoanCollateral collateral)
        {
            var idx = _collaterals.FindIndex(c => c.Id == collateral.Id);
            if (idx >= 0) _collaterals[idx] = collateral;
            return await Task.FromResult(collateral);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var idx = _collaterals.FindIndex(c => c.Id.ToString() == id);
            if (idx >= 0) { _collaterals.RemoveAt(idx); return await Task.FromResult(true); }
            return await Task.FromResult(false);
        }
    }
}
