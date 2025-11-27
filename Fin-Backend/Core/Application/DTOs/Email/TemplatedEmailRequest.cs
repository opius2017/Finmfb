using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Email
{
    public class TemplatedEmailRequest
    {
        public string ToEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string TemplateName { get; set; } = string.Empty;
        public Dictionary<string, string> TemplateParameters { get; set; } = new();
        public List<string>? CcEmails { get; set; }
        public List<string>? BccEmails { get; set; }
    }
}
