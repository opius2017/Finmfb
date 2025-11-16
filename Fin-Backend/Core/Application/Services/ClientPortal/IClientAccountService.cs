using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.ClientPortal;
using FinTech.Core.Application.DTOs.Common;
using FinTech.Core.Application.Common;

namespace FinTech.Core.Application.Services.ClientPortal
{
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
}