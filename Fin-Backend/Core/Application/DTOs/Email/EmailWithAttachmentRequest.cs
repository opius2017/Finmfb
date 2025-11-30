namespace FinTech.Core.Application.DTOs.Email;

public class EmailWithAttachmentRequest : EmailRequest
{
    public List<EmailAttachment> Attachments { get; set; } = new();
}

public class EmailAttachment
{
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/octet-stream";
    public string? ContentId { get; set; }
    public bool IsInline { get; set; }
}
