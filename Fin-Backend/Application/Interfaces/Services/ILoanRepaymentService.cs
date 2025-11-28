using FinTech.Core.Domain.Entities.Loans;

namespace FinTech.Core.Application.Interfaces.Services;

public interface ILoanRepaymentService
{
    Task<string> ProcessRepaymentAsync(string loanId, decimal amount, string paymentMethod, string reference);
    Task<IEnumerable<LoanRepayment>> GetRepaymentsByLoanIdAsync(string loanId);
    Task<LoanRepayment?> GetRepaymentByIdAsync(string id);
    Task<decimal> GetTotalRepaymentsAsync(string loanId);
    Task<decimal> CalculateOutstandingBalanceAsync(string loanId);
    Task ReverseRepaymentAsync(string repaymentId, string reason);
}
