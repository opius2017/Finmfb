using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Email
{
    public class BulkEmailRequest
    {
        public List<string> ToEmails { get; set; } = new();
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsHtml { get; set; } = true;
        public List<string>? BccEmails { get; set; }
    }
}
