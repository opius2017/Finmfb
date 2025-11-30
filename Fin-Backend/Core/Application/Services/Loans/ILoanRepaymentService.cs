using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Loans;
using FinTech.Core.Domain.Entities.Loans;

namespace FinTech.Core.Application.Services.Loans
{
    /// <summary>
    /// Interface for loan repayment services
    /// </summary>
    public interface ILoanRepaymentService
    {
        /// <summary>
        /// Processes a loan repayment
        /// </summary>
        Task<RepaymentResult> ProcessRepaymentAsync(RepaymentRequest request);

        /// <summary>
        /// Processes partial payment
        /// </summary>
        Task<RepaymentResult> ProcessPartialPaymentAsync(RepaymentRequest request);

        /// <summary>
        /// Updates repayment schedule after payment
        /// </summary>
        Task UpdateRepaymentScheduleAsync(string loanId, PaymentAllocation allocation);

        /// <summary>
        /// Generates repayment receipt
        /// </summary>
        Task<RepaymentReceipt> GenerateRepaymentReceiptAsync(
            Loan loan,
            LoanTransaction transaction,
            PaymentAllocation allocation);

        /// <summary>
        /// Gets repayment history for a loan
        /// </summary>
        Task<RepaymentHistory> GetLoanRepaymentHistoryAsync(string loanId);

        /// <summary>
        /// Gets repayment schedule for a loan
        /// </summary>
        Task<List<RepaymentScheduleItem>> GetRepaymentScheduleAsync(string loanId);
    }
}
