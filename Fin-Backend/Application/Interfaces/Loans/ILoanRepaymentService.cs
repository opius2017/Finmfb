using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Loans;

namespace FinTech.Core.Application.Interfaces.Loans
{
    public interface ILoanRepaymentService
    {
        Task<IEnumerable<LoanRepaymentDto>> GetRepaymentsByLoanIdAsync(string loanId);
        Task<LoanRepaymentDto> RecordRepaymentAsync(string loanId, LoanRepaymentDto repaymentDto);
    }
}
