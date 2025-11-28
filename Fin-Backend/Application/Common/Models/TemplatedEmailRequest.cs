namespace FinTech.Core.Application.Common.Models
{
    public class TemplatedEmailRequest
    {
        public string To { get; set; } = string.Empty;
        public string TemplateId { get; set; } = string.Empty;
        public object? Model { get; set; }
        public Dictionary<string, string>? Headers { get; set; }
    }
}
