using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.ClientPortal;
using FinTech.Core.Application.DTOs.Common;

namespace FinTech.Core.Application.Interfaces
{
    public interface IClientLoanService
    {
        Task<BaseResponse<List<ClientLoanDto>>> GetLoansAsync(Guid customerId);
        Task<BaseResponse<ClientLoanDto>> GetLoanDetailsAsync(Guid customerId, Guid loanId);
        Task<BaseResponse<List<LoanRepaymentScheduleDto>>> GetLoanRepaymentScheduleAsync(Guid customerId, Guid loanId);
        Task<BaseResponse<List<LoanTransactionDto>>> GetLoanTransactionsAsync(Guid customerId, Guid loanId);
        Task<BaseResponse<LoanPaymentDto>> MakeLoanPaymentAsync(Guid customerId, Guid loanId, LoanPaymentRequestDto paymentRequest);
        Task<BaseResponse<List<LoanProductDto>>> GetAvailableLoanProductsAsync(Guid customerId);
        Task<BaseResponse<LoanApplicationDto>> SubmitLoanApplicationAsync(Guid customerId, LoanApplicationRequestDto applicationRequest);
        Task<BaseResponse<List<LoanApplicationDto>>> GetLoanApplicationsAsync(Guid customerId);
        Task<BaseResponse<LoanApplicationDto>> GetLoanApplicationDetailsAsync(Guid customerId, Guid applicationId);
    }
}
