using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.ClientPortal;
using FinTech.Core.Application.DTOs.Common;

namespace FinTech.Core.Application.Interfaces
{
    public interface IClientTransactionService
    {
        Task<BaseResponse<List<TransactionDto>>> GetTransactionsAsync(Guid customerId, TransactionFilterDto filter);
        Task<BaseResponse<TransactionDto>> GetTransactionDetailsAsync(Guid customerId, Guid transactionId);
    }
}
