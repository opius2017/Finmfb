namespace FinTech.Core.Application.DTOs.Auth
{
    public class SecurityActivityDto
    {
        public string ActivityId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
        public string? DeviceInfo { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Details { get; set; }
    }
}
