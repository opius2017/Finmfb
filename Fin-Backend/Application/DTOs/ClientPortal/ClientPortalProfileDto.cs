using System;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    // Profile DTOs
    public class ClientPortalProfileDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid CustomerId { get; set; }
        public string PreferredLanguage { get; set; }
        public string TimeZone { get; set; }
        public bool DarkModeEnabled { get; set; }
        public bool NewsletterSubscribed { get; set; }
        public bool MarketingNotificationsEnabled { get; set; }
        public string PushNotificationToken { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public int LoginCount { get; set; }
        public DateTime? OnboardingCompletedDate { get; set; }
        public NotificationPreferencesDto NotificationPreferences { get; set; }
        public DashboardPreferencesDto DashboardPreferences { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastModifiedOn { get; set; }
    }
}
