namespace FinTech.Core.Domain.Enums
{
    public enum NotificationType
    {
        AccountAlert,
        TransactionAlert,
        SecurityAlert,
        LoanAlert,
        PaymentReminder,
        BillPaymentAlert,
        SystemMessage,
        MarketingOffer,
        AccountStatusChange,
        SessionActivity,
        FeatureAnnouncement
    }

    public enum NotificationPriority
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    public enum NotificationChannel
    {
        InApp,
        Email,
        SMS,
        Push
    }

    public enum NotificationDeliveryStatus
    {
        Pending,
        Sent,
        Delivered,
        Failed,
        Expired
    }
}
