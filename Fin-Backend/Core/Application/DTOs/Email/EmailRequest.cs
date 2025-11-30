namespace FinTech.Core.Application.DTOs.Email;

public class EmailRequest
{
    public string To { get; set; } = string.Empty;
    public string? ToName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;
    public string? From { get; set; }
    public string? FromName { get; set; }
    public List<string>? Cc { get; set; }
    public List<string>? Bcc { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
    public int Priority { get; set; } = 3; // 1=High, 3=Normal, 5=Low
}
