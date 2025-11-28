namespace FinTech.Core.Application.DTOs.Auth
{
    public class SecurityPreferencesDto
    {
        public bool EmailNotificationsEnabled { get; set; }
        public bool LoginNotificationsEnabled { get; set; }
        public bool UnusualActivityNotificationsEnabled { get; set; }
    }
}
