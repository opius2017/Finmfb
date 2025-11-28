namespace FinTech.Core.Application.Common.Models
{
    public class ClientPortalSettings
    {
        public int MaxFailedLoginAttempts { get; set; }
        public int LockoutDurationMinutes { get; set; }
        public string? BaseUrl { get; set; }
    }
}
