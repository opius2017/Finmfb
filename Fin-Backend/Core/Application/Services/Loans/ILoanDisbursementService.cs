using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Loans;
using FinTech.Core.Domain.Entities.Loans;

namespace FinTech.Core.Application.Services.Loans
{
    /// <summary>
    /// Interface for loan disbursement services
    /// </summary>
    public interface ILoanDisbursementService
    {
        /// <summary>
        /// Initiates cash loan disbursement workflow
        /// </summary>
        Task<DisbursementResult> DisburseCashLoanAsync(DisbursementRequest request);

        /// <summary>
        /// Generates loan agreement document
        /// </summary>
        Task<LoanAgreementDocument> GenerateLoanAgreementAsync(
            Loan loan,
            Member member,
            string serialNumber);

        /// <summary>
        /// Processes bank transfer for loan disbursement
        /// </summary>
        Task<BankTransferResult> ProcessBankTransferAsync(
            Member member,
            decimal amount,
            string reference);

        /// <summary>
        /// Tracks disbursement transaction
        /// </summary>
        Task<TransactionTrackingResult> TrackDisbursementAsync(string transactionId);

        /// <summary>
        /// Sends disbursement confirmation notification
        /// </summary>
        Task SendDisbursementNotificationAsync(
            string memberId,
            DisbursementResult disbursement);

        /// <summary>
        /// Gets disbursement history for a member
        /// </summary>
        Task<DisbursementHistory> GetMemberDisbursementHistoryAsync(string memberId);
    }
}
