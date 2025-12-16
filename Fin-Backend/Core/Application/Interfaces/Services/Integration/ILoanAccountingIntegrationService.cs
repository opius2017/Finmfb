using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using FinTech.Core.Domain.Entities.Loans;

namespace FinTech.Core.Application.Interfaces.Services.Integration
{
    public interface ILoanAccountingIntegrationService
    {
        Task ProcessLoanDisbursementAsync(LoanAccount loanAccount, LoanTransaction transaction, string tenantId, CancellationToken cancellationToken);
        Task ProcessLoanRepaymentAsync(LoanAccount loanAccount, LoanTransaction transaction, string tenantId, CancellationToken cancellationToken);
        Task ProcessInterestAccrualAsync(IEnumerable<LoanAccount> loanAccounts, DateTime accrualDate, string tenantId, CancellationToken cancellationToken);
        Task ProcessLoanWriteOffAsync(LoanAccount loanAccount, decimal writeOffAmount, string tenantId, CancellationToken cancellationToken);
        Task ProcessLoanFeeChargeAsync(LoanAccount loanAccount, decimal feeAmount, string feeType, string tenantId, CancellationToken cancellationToken);

        // Primitive type overloads for event handlers
        Task ProcessLoanDisbursementAsync(string loanId, decimal amount, string reference, string description);
        Task ProcessLoanRepaymentAsync(string loanId, decimal principalAmount, decimal interestAmount, string reference, string description);
        Task ProcessLoanWriteOffAsync(string loanId, decimal amount, string reference, string description);
        Task ProcessLoanInterestAccrualAsync(string loanId, decimal amount, string reference, string description);
        Task ProcessLoanFeeChargeAsync(string loanId, decimal amount, string feeType, string reference, string description);
    }
}
