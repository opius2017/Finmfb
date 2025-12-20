using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.ClientPortal;
using FinTech.Core.Application.DTOs.Common;
using FinTech.Core.Application.DTOs.Auth;

namespace FinTech.Core.Application.Interfaces
{
    public interface IClientPaymentService
    {
        Task<PaymentResult> ProcessBillPaymentAsync(string customerId, string fromAccountId, string billerId, decimal amount, string reference, bool isRecurring = false);
        Task<PaymentResult> ProcessTransferAsync(string customerId, string fromAccountId, string toAccountId, decimal amount, string reference, bool isRecurring = false);
        Task<PaymentResult> ProcessExternalTransferAsync(string customerId, string fromAccountId, string beneficiaryId, decimal amount, string reference, bool isRecurring = false);
        Task<BaseResponse<List<BillPaymentDto>>> GetRecentBillPaymentsAsync(string customerId, int count = 5);
        Task<BaseResponse<List<TransferDto>>> GetRecentTransfersAsync(string customerId, int count = 5);
        Task<BaseResponse<List<RecurringPaymentDto>>> GetRecurringPaymentsAsync(string customerId);
        Task<BaseResponse<RecurringPaymentDto>> GetRecurringPaymentDetailsAsync(string customerId, string paymentId);
        Task<BaseResponse<RecurringPaymentDto>> CreateRecurringPaymentAsync(string customerId, RecurringPaymentCreateDto paymentDto);
        Task<BaseResponse<RecurringPaymentDto>> UpdateRecurringPaymentAsync(string customerId, string paymentId, RecurringPaymentUpdateDto paymentDto);
        Task<BaseResponse<bool>> CancelRecurringPaymentAsync(string customerId, string paymentId);
        Task<BaseResponse<List<BillerDto>>> GetAvailableBillersAsync();
        Task<BaseResponse<List<SavedPayeeDto>>> GetSavedPayeesAsync(string customerId);
        Task<BaseResponse<SavedPayeeDto>> CreateSavedPayeeAsync(string customerId, SavedPayeeCreateDto payeeDto);
        Task<BaseResponse<SavedPayeeDto>> UpdateSavedPayeeAsync(string customerId, string payeeId, SavedPayeeUpdateDto payeeDto);
        Task<BaseResponse<bool>> DeleteSavedPayeeAsync(string customerId, string payeeId);
    }
}
