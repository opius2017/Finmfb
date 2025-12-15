using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.ClientPortal;
using FinTech.Core.Application.DTOs.Common;

namespace FinTech.Core.Application.Interfaces
{
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
}
