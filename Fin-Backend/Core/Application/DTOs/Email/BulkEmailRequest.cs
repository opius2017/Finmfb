namespace FinTech.Core.Application.DTOs.Email;

public class BulkEmailRequest
{
    public List<string> Recipients { get; set; } = new();
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;
    public string? From { get; set; }
    public string? FromName { get; set; }
    public int BatchSize { get; set; } = 50;
    public int DelayBetweenBatchesMs { get; set; } = 1000;
    public Dictionary<string, object>? TemplateData { get; set; }
}
