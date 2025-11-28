namespace FinTech.Core.Application.DTOs.ClientPortal
{
    public class UpdateClientPortalProfileDto
    {
        public string PreferredLanguage { get; set; }
        public string TimeZone { get; set; }
        public bool? DarkModeEnabled { get; set; }
        public bool? NewsletterSubscribed { get; set; }
        public bool? MarketingNotificationsEnabled { get; set; }
        public string PushNotificationToken { get; set; }
        public NotificationPreferencesUpdateDto NotificationPreferences { get; set; }
        public DashboardPreferencesUpdateDto DashboardPreferences { get; set; }
    }
}
