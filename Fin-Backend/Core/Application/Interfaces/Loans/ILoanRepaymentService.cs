using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Loans;
using FinTech.Core.Domain.Entities.Loans;

namespace FinTech.Core.Application.Interfaces.Loans
{
    public interface ILoanRepaymentService
    {
        Task<RepaymentResult> ProcessRepaymentAsync(RepaymentRequest request);
        Task<RepaymentResult> ProcessPartialPaymentAsync(RepaymentRequest request);
        Task UpdateRepaymentScheduleAsync(string loanId, PaymentAllocation allocation);
        Task<RepaymentReceipt> GenerateRepaymentReceiptAsync(Loan loan, LoanTransaction transaction, PaymentAllocation allocation);
        Task<RepaymentHistory> GetLoanRepaymentHistoryAsync(string loanId);
        Task<List<RepaymentScheduleItem>> GetRepaymentScheduleAsync(string loanId);
    }
}
