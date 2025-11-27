namespace FinTech.Core.Application.DTOs.Email
{
    public class EmailRequest
    {
        public string ToEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsHtml { get; set; } = true;
        public List<string>? CcEmails { get; set; }
        public List<string>? BccEmails { get; set; }
    }
}
