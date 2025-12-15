using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.ClientPortal;
using FinTech.Core.Application.DTOs.Common;
using FinTech.Core.Application.Interfaces;

namespace FinTech.Core.Application.Services.ClientPortal
{
    public class ClientTransactionService : IClientTransactionService
    {
        public Task<BaseResponse<List<TransactionDto>>> GetTransactionsAsync(Guid customerId, TransactionFilterDto filter)
        {
            return Task.FromResult(new BaseResponse<List<TransactionDto>> { Success = true, Data = new List<TransactionDto>() });
        }

        public Task<BaseResponse<TransactionDto>> GetTransactionDetailsAsync(Guid customerId, Guid transactionId)
        {
            return Task.FromResult(new BaseResponse<TransactionDto> { Success = true, Data = new TransactionDto() });
        }
    }
}
