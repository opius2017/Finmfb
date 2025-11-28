using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    public class LoanDocumentUploadDto
    {
        [Required]
        public string DocumentType { get; set; }
        
        [Required]
        public string FileName { get; set; }
        
        [Required]
        public string FileContent { get; set; } // Base64 encoded
        
        public string MimeType { get; set; }
        public long FileSize { get; set; }
    }
}