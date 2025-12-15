using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.ClientPortal;
using FinTech.Core.Application.DTOs.Common;
using FinTech.Core.Application.DTOs.Auth;

namespace FinTech.Core.Application.Interfaces
{
    public interface IClientSecurityService
    {
        Task<BaseResponse<TwoFactorStatusDto>> GetTwoFactorStatusAsync(Guid customerId);
        Task<BaseResponse<TwoFactorStatusDto>> EnableTwoFactorAsync(Guid customerId, EnableTwoFactorDto twoFactorDto);
        Task<BaseResponse<bool>> DisableTwoFactorAsync(Guid customerId, DisableTwoFactorDto twoFactorDto);
        Task<BaseResponse<bool>> ChangePasswordAsync(Guid customerId, ChangePasswordDto passwordDto);
        Task<BaseResponse<List<SecurityActivityDto>>> GetSecurityActivitiesAsync(Guid customerId, int page = 1, int pageSize = 20);
    }
}
