namespace FinTech.Core.Application.Settings
{
    public class NotificationSettings
    {
        public bool EmailEnabled { get; set; } = true;
        public bool SmsEnabled { get; set; } = true;
        public bool PushEnabled { get; set; } = true;
        public string EmailFromAddress { get; set; } = "noreply@finmfb.com";
        public string EmailFromName { get; set; } = "FinMFB";
    }
}
