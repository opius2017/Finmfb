using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.ClientPortal;
using FinTech.Core.Application.DTOs.Common;

namespace FinTech.Core.Application.Interfaces
{
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
}
