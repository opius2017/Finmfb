using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using FinTech.Core.Application.DTOs.ClientPortal;
using FinTech.Core.Application.DTOs.Common;
using FinTech.Core.Application.DTOs.Auth;
using FinTech.Core.Application.Interfaces;

namespace FinTech.Core.Application.Services.ClientPortal
{
    public class ClientSecurityService : IClientSecurityService
    {
        public Task<BaseResponse<TwoFactorStatusDto>> GetTwoFactorStatusAsync(Guid customerId)
        {
             return Task.FromResult(new BaseResponse<TwoFactorStatusDto> { Success = true, Data = new TwoFactorStatusDto() });
        }
        
        public Task<BaseResponse<TwoFactorStatusDto>> EnableTwoFactorAsync(Guid customerId, EnableTwoFactorDto twoFactorDto)
        {
             return Task.FromResult(new BaseResponse<TwoFactorStatusDto> { Success = true, Data = new TwoFactorStatusDto() });
        }
        
        public Task<BaseResponse<bool>> DisableTwoFactorAsync(Guid customerId, DisableTwoFactorDto twoFactorDto)
        {
             return Task.FromResult(new BaseResponse<bool> { Success = true, Data = true });
        }

        public Task<BaseResponse<bool>> ChangePasswordAsync(Guid customerId, ChangePasswordDto passwordDto)
        {
            return Task.FromResult(new BaseResponse<bool> { Success = true, Data = true });
        }
        
        public Task<BaseResponse<bool>> EnableMfaAsync(Guid customerId, EnableTwoFactorDto mfaDto)
        {
            return Task.FromResult(new BaseResponse<bool> { Success = true, Data = true });
        }
        
        public Task<BaseResponse<bool>> DisableMfaAsync(Guid customerId, DisableTwoFactorDto mfaDto)
        {
            return Task.FromResult(new BaseResponse<bool> { Success = true, Data = true });
        }

        public Task<BaseResponse<List<SecurityActivityDto>>> GetSecurityActivitiesAsync(Guid customerId, int page = 1, int pageSize = 20)
        {
             return Task.FromResult(new BaseResponse<List<SecurityActivityDto>> { Success = true, Data = new List<SecurityActivityDto>() });
        }
    }
}
