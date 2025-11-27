using System;

namespace FinTech.Core.Application.DTOs.Email
{
    public class EmailAttachment
    {
        public string FileName { get; set; } = string.Empty;
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = "application/octet-stream";
    }
}
