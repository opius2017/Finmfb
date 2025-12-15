using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FinTech.Core.Application.DTOs.ClientPortal;
using FinTech.Core.Application.DTOs.Common;
using FinTech.Core.Application.Interfaces;

namespace FinTech.Core.Application.Services.ClientPortal
{
    public class ClientDocumentService : IClientDocumentService
    {
        public Task<BaseResponse<ClientDocumentDto>> UploadDocumentAsync(Guid customerId, ClientDocumentUploadDto documentDto)
        {
            return Task.FromResult(new BaseResponse<ClientDocumentDto> { Success = true, Data = new ClientDocumentDto() });
        }
        
        public Task<BaseResponse<List<ClientDocumentDto>>> GetDocumentsAsync(Guid customerId)
        {
            return Task.FromResult(new BaseResponse<List<ClientDocumentDto>> { Success = true, Data = new List<ClientDocumentDto>() });
        }
        
        public Task<BaseResponse<ClientDocumentDto>> GetDocumentDetailsAsync(Guid customerId, Guid documentId)
        {
             return Task.FromResult(new BaseResponse<ClientDocumentDto> { Success = true, Data = new ClientDocumentDto() });
        }
        
        public Task<BaseResponse<byte[]>> DownloadDocumentAsync(Guid customerId, Guid documentId)
        {
            return Task.FromResult(new BaseResponse<byte[]> { Success = true, Data = Array.Empty<byte>() });
        }
        
        public Task<BaseResponse<bool>> DeleteDocumentAsync(Guid customerId, Guid documentId)
        {
             return Task.FromResult(new BaseResponse<bool> { Success = true, Data = true });
        }
        
        public Task<BaseResponse<List<DocumentTypeDto>>> GetDocumentTypesAsync()
        {
             return Task.FromResult(new BaseResponse<List<DocumentTypeDto>> { Success = true, Data = new List<DocumentTypeDto>() });
        }
    }
}
