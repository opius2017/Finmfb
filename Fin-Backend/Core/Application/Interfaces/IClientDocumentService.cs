using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.ClientPortal;
using FinTech.Core.Application.DTOs.Common;

namespace FinTech.Core.Application.Interfaces
{
    public interface IClientDocumentService
    {
        Task<BaseResponse<List<ClientDocumentDto>>> GetDocumentsAsync(Guid customerId);
        Task<BaseResponse<ClientDocumentDto>> GetDocumentDetailsAsync(Guid customerId, Guid documentId);
        Task<BaseResponse<byte[]>> DownloadDocumentAsync(Guid customerId, Guid documentId);
        Task<BaseResponse<ClientDocumentDto>> UploadDocumentAsync(Guid customerId, ClientDocumentUploadDto documentDto);
        Task<BaseResponse<bool>> DeleteDocumentAsync(Guid customerId, Guid documentId);
        Task<BaseResponse<List<DocumentTypeDto>>> GetDocumentTypesAsync();
    }
}
