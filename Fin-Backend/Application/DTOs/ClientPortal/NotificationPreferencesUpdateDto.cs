namespace FinTech.Core.Application.DTOs.ClientPortal
{
    public class NotificationPreferencesUpdateDto
    {
        public bool? TransactionAlertsEnabled { get; set; }
        public decimal? TransactionAlertThreshold { get; set; }
        public bool? BalanceAlertsEnabled { get; set; }
        public decimal? LowBalanceThreshold { get; set; }
        public bool? LoanPaymentRemindersEnabled { get; set; }
        public int? DaysBeforeLoanPaymentDue { get; set; }
        public bool? BillPaymentRemindersEnabled { get; set; }
        public int? DaysBeforeBillPaymentDue { get; set; }
        public bool? SecurityAlertsEnabled { get; set; }
        public bool? ProductOffersEnabled { get; set; }
        public bool? AccountStatusChangesEnabled { get; set; }
        public bool? EmailNotificationsEnabled { get; set; }
        public bool? SmsNotificationsEnabled { get; set; }
        public bool? PushNotificationsEnabled { get; set; }
        public bool? InAppNotificationsEnabled { get; set; }
    }
}
