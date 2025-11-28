using System;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    public class ClientDocumentDto
    {
        public Guid Id { get; set; }
        public string DocumentType { get; set; }
        public string DocumentName { get; set; }
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public long FileSize { get; set; }
        public bool IsSharedByBank { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool RequiresSignature { get; set; }
        public bool IsSigned { get; set; }
        public DateTime? SignedDate { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
