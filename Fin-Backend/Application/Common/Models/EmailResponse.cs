namespace FinTech.Core.Application.Common.Models
{
    public class EmailResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ProviderId { get; set; }
    }
}
