using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.ClientPortal;
using FinTech.Core.Application.DTOs.Common;

namespace FinTech.Core.Application.Interfaces
{
    public interface IClientLoanService
    {
        Task<BaseResponse<List<ClientLoanDto>>> GetLoansAsync(string customerId);
        Task<BaseResponse<ClientLoanDto>> GetLoanDetailsAsync(string customerId, Guid loanId);
        Task<BaseResponse<List<LoanRepaymentScheduleDto>>> GetLoanRepaymentScheduleAsync(string customerId, Guid loanId);
        Task<BaseResponse<List<LoanTransactionDto>>> GetLoanTransactionsAsync(string customerId, Guid loanId);
        Task<BaseResponse<LoanPaymentDto>> MakeLoanPaymentAsync(string customerId, Guid loanId, LoanPaymentRequestDto paymentRequest);
        Task<BaseResponse<List<LoanProductDto>>> GetAvailableLoanProductsAsync(string customerId);
        Task<BaseResponse<LoanApplicationDto>> SubmitLoanApplicationAsync(string customerId, LoanApplicationRequestDto applicationRequest);
        Task<BaseResponse<List<LoanApplicationDto>>> GetLoanApplicationsAsync(string customerId);
        Task<BaseResponse<LoanApplicationDto>> GetLoanApplicationDetailsAsync(string customerId, Guid applicationId);
    }
}
