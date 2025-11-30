namespace FinTech.Core.Application.DTOs.Email;

public class TemplatedEmailRequest
{
    public string To { get; set; } = string.Empty;
    public string? ToName { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public Dictionary<string, object> TemplateData { get; set; } = new();
    public string? Subject { get; set; }
    public string? From { get; set; }
    public string? FromName { get; set; }
    public List<string>? Cc { get; set; }
    public List<string>? Bcc { get; set; }
    public int Priority { get; set; } = 3;
}
