using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.ClientPortal;
using FinTech.Core.Application.DTOs.Common;
using FinTech.Core.Application.DTOs.Security;
using FinTech.Core.Application.Common;
using FinTech.Core.Application.Common.Models;

namespace FinTech.Core.Application.Services
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
    
    public interface IClientAccountService
    {
        Task<BaseResponse<List<ClientAccountDto>>> GetAccountsAsync(Guid customerId);
        Task<BaseResponse<ClientAccountDto>> GetAccountDetailsAsync(Guid customerId, Guid accountId);
        Task<BaseResponse<List<TransactionDto>>> GetAccountTransactionsAsync(Guid customerId, Guid accountId, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20);
        Task<BaseResponse<AccountStatementDto>> GetAccountStatementAsync(Guid customerId, Guid accountId, DateTime startDate, DateTime endDate);
        Task<BaseResponse<byte[]>> ExportAccountStatementAsync(Guid customerId, Guid accountId, DateTime startDate, DateTime endDate, string format);
        Task<BaseResponse<List<AccountCategoryDto>>> GetTransactionCategoriesAsync(Guid customerId);
        Task<BaseResponse<bool>> UpdateTransactionCategoryAsync(Guid customerId, Guid transactionId, Guid categoryId);
    }
    
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
    
    public interface IClientProfileService
    {
        Task<BaseResponse<ClientProfileDto>> GetProfileAsync(Guid customerId);
        Task<BaseResponse<ClientProfileDto>> UpdateProfileAsync(Guid customerId, ClientProfileUpdateDto profileDto);
        Task<BaseResponse<List<ClientAddressDto>>> GetAddressesAsync(Guid customerId);
        Task<BaseResponse<ClientAddressDto>> AddAddressAsync(Guid customerId, ClientAddressCreateDto addressDto);
        Task<BaseResponse<ClientAddressDto>> UpdateAddressAsync(Guid customerId, Guid addressId, ClientAddressUpdateDto addressDto);
        Task<BaseResponse<bool>> DeleteAddressAsync(Guid customerId, Guid addressId);
        Task<BaseResponse<List<ClientContactDto>>> GetContactsAsync(Guid customerId);
        Task<BaseResponse<ClientContactDto>> AddContactAsync(Guid customerId, ClientContactCreateDto contactDto);
        Task<BaseResponse<ClientContactDto>> UpdateContactAsync(Guid customerId, Guid contactId, ClientContactUpdateDto contactDto);
        Task<BaseResponse<bool>> DeleteContactAsync(Guid customerId, Guid contactId);
        Task<BaseResponse<NotificationPreferencesDto>> GetNotificationPreferencesAsync(Guid customerId);
        Task<BaseResponse<NotificationPreferencesDto>> UpdateNotificationPreferencesAsync(Guid customerId, NotificationPreferencesUpdateDto preferencesDto);
    }
    
    public interface IClientDocumentService
    {
        Task<BaseResponse<List<ClientDocumentDto>>> GetDocumentsAsync(Guid customerId);
        Task<BaseResponse<ClientDocumentDto>> GetDocumentDetailsAsync(Guid customerId, Guid documentId);
        Task<BaseResponse<byte[]>> DownloadDocumentAsync(Guid customerId, Guid documentId);
        Task<BaseResponse<ClientDocumentDto>> UploadDocumentAsync(Guid customerId, ClientDocumentUploadDto documentDto);
        Task<BaseResponse<bool>> DeleteDocumentAsync(Guid customerId, Guid documentId);
        Task<BaseResponse<List<DocumentTypeDto>>> GetDocumentTypesAsync();
    }
    
    public interface IClientSavingsGoalService
    {
        Task<BaseResponse<List<SavingsGoalDto>>> GetSavingsGoalsAsync(Guid customerId);
        Task<BaseResponse<SavingsGoalDto>> GetSavingsGoalDetailsAsync(Guid customerId, Guid goalId);
        Task<BaseResponse<SavingsGoalDto>> CreateSavingsGoalAsync(Guid customerId, SavingsGoalCreateDto goalDto);
        Task<BaseResponse<SavingsGoalDto>> UpdateSavingsGoalAsync(Guid customerId, Guid goalId, SavingsGoalUpdateDto goalDto);
        Task<BaseResponse<bool>> DeleteSavingsGoalAsync(Guid customerId, Guid goalId);
        Task<BaseResponse<List<SavingsGoalTransactionDto>>> GetSavingsGoalTransactionsAsync(Guid customerId, Guid goalId);
        Task<BaseResponse<SavingsGoalTransactionDto>> AddFundsToGoalAsync(Guid customerId, Guid goalId, AddFundsToGoalDto fundsDto);
        Task<BaseResponse<SavingsGoalTransactionDto>> WithdrawFundsFromGoalAsync(Guid customerId, Guid goalId, WithdrawFundsFromGoalDto fundsDto);
    }
    
    public interface IClientSessionService
    {
        Task<BaseResponse<List<ClientSessionDto>>> GetActiveSessionsAsync(Guid customerId);
        Task<BaseResponse<bool>> RevokeSessionAsync(Guid customerId, Guid sessionId);
        Task<BaseResponse<List<ClientDeviceDto>>> GetDevicesAsync(Guid customerId);
        Task<BaseResponse<bool>> RevokeDeviceAsync(Guid customerId, Guid deviceId);
        Task<BaseResponse<bool>> UpdateDeviceTrustStatusAsync(Guid customerId, Guid deviceId, bool isTrusted);
    }
    
    public interface IClientSecurityService
    {
        Task<BaseResponse<TwoFactorStatusDto>> GetTwoFactorStatusAsync(Guid customerId);
        Task<BaseResponse<TwoFactorStatusDto>> EnableTwoFactorAsync(Guid customerId, EnableTwoFactorDto twoFactorDto);
        Task<BaseResponse<bool>> DisableTwoFactorAsync(Guid customerId, DisableTwoFactorDto twoFactorDto);
        Task<BaseResponse<bool>> ChangePasswordAsync(Guid customerId, ChangePasswordDto passwordDto);
        Task<BaseResponse<List<SecurityActivityDto>>> GetSecurityActivitiesAsync(Guid customerId, int page = 1, int pageSize = 20);
    }
    
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(byte[] fileData, string fileName, string contentType, string containerName);
        Task<byte[]> DownloadFileAsync(string filePath, string containerName);
        Task DeleteFileAsync(string filePath, string containerName);
        bool IsValidFileType(string fileName, string[] allowedExtensions);
        bool IsValidFileSize(long fileSize, long maxFileSize);
    }
}
