using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.ClientPortal;
using FinTech.Core.Application.DTOs.Common;
using FinTech.Core.Application.Common;
using FinTech.Core.Application.Common.Models;

namespace FinTech.Core.Application.Services.ClientPortal
{
    public interface IClientPaymentService
    {
        Task<PaymentResult> ProcessBillPaymentAsync(Guid customerId, Guid fromAccountId, Guid billerId, decimal amount, string reference, bool isRecurring = false);
        Task<PaymentResult> ProcessTransferAsync(Guid customerId, Guid fromAccountId, Guid toAccountId, decimal amount, string reference, bool isRecurring = false);
        Task<PaymentResult> ProcessExternalTransferAsync(Guid customerId, Guid fromAccountId, Guid beneficiaryId, decimal amount, string reference, bool isRecurring = false);
        Task<BaseResponse<List<BillPaymentDto>>> GetRecentBillPaymentsAsync(Guid customerId, int count = 5);
        Task<BaseResponse<List<TransferDto>>> GetRecentTransfersAsync(Guid customerId, int count = 5);
        Task<BaseResponse<List<RecurringPaymentDto>>> GetRecurringPaymentsAsync(Guid customerId);
        Task<BaseResponse<RecurringPaymentDto>> GetRecurringPaymentDetailsAsync(Guid customerId, Guid paymentId);
        Task<BaseResponse<RecurringPaymentDto>> CreateRecurringPaymentAsync(Guid customerId, RecurringPaymentCreateDto paymentDto);
        Task<BaseResponse<RecurringPaymentDto>> UpdateRecurringPaymentAsync(Guid customerId, Guid paymentId, RecurringPaymentUpdateDto paymentDto);
        Task<BaseResponse<bool>> CancelRecurringPaymentAsync(Guid customerId, Guid paymentId);
        Task<BaseResponse<List<BillerDto>>> GetAvailableBillersAsync();
        Task<BaseResponse<List<SavedPayeeDto>>> GetSavedPayeesAsync(Guid customerId);
        Task<BaseResponse<SavedPayeeDto>> CreateSavedPayeeAsync(Guid customerId, SavedPayeeCreateDto payeeDto);
        Task<BaseResponse<SavedPayeeDto>> UpdateSavedPayeeAsync(Guid customerId, Guid payeeId, SavedPayeeUpdateDto payeeDto);
        Task<BaseResponse<bool>> DeleteSavedPayeeAsync(Guid customerId, Guid payeeId);
    }
}