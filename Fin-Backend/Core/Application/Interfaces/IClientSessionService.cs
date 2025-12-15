using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.ClientPortal;
using FinTech.Core.Application.DTOs.Common;

namespace FinTech.Core.Application.Interfaces
{
    public interface IClientSessionService
    {
        Task<BaseResponse<List<ClientSessionDto>>> GetActiveSessionsAsync(Guid customerId);
        Task<BaseResponse<bool>> RevokeSessionAsync(Guid customerId, Guid sessionId);
        Task<BaseResponse<List<ClientDeviceDto>>> GetDevicesAsync(Guid customerId);
        Task<BaseResponse<bool>> RevokeDeviceAsync(Guid customerId, Guid deviceId);
        Task<BaseResponse<bool>> UpdateDeviceTrustStatusAsync(Guid customerId, Guid deviceId, bool isTrusted);
    }
}
