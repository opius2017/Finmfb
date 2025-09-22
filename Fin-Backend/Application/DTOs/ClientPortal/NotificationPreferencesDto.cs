namespace FinTech.Application.DTOs.ClientPortal
{
    public class NotificationPreferencesDto
    {
        public bool EmailNotificationsEnabled { get; set; }
        public bool SmsNotificationsEnabled { get; set; }
        public bool PushNotificationsEnabled { get; set; }
    }

    public class NotificationPreferencesUpdateDto
    {
        public bool EmailNotificationsEnabled { get; set; }
        public bool SmsNotificationsEnabled { get; set; }
        public bool PushNotificationsEnabled { get; set; }
    }
}
