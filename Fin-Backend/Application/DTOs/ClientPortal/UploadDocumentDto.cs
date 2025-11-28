using Microsoft.AspNetCore.Http;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    public class UploadDocumentDto
    {
        public string DocumentType { get; set; }
        public string DocumentName { get; set; }
        public IFormFile File { get; set; }
    }
}
