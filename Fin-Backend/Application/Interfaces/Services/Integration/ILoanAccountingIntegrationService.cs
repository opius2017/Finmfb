using System.Threading.Tasks;

namespace FinTech.Core.Application.Services.Integration
{
    public interface ILoanAccountingIntegrationService
    {
        Task ProcessLoanDisbursementAsync(int loanId, decimal amount, string reference, string description);
        Task ProcessLoanRepaymentAsync(int loanId, decimal principalAmount, decimal interestAmount, string reference, string description);
        Task ProcessLoanWriteOffAsync(int loanId, decimal amount, string reference, string description);
        Task ProcessLoanInterestAccrualAsync(int loanId, decimal amount, string reference, string description);
        Task ProcessLoanFeeChargeAsync(int loanId, decimal amount, string feeType, string reference, string description);
    }
}
