namespace FinTech.Core.Application.DTOs.Email;

public class EmailResponse
{
    public bool Success { get; set; }
    public string? MessageId { get; set; }
    public string? Message { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public string? ErrorMessage { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
