using System;
using System.Collections.Generic;

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

    // Notification Preferences DTOs
    public class NotificationPreferencesDto
    {
        public Guid Id { get; set; }
        public bool TransactionAlertsEnabled { get; set; }
        public decimal TransactionAlertThreshold { get; set; }
        public bool BalanceAlertsEnabled { get; set; }
        public decimal LowBalanceThreshold { get; set; }
        public bool LoanPaymentRemindersEnabled { get; set; }
        public int DaysBeforeLoanPaymentDue { get; set; }
        public bool BillPaymentRemindersEnabled { get; set; }
        public int DaysBeforeBillPaymentDue { get; set; }
        public bool SecurityAlertsEnabled { get; set; }
        public bool ProductOffersEnabled { get; set; }
        public bool AccountStatusChangesEnabled { get; set; }
        public bool EmailNotificationsEnabled { get; set; }
        public bool SmsNotificationsEnabled { get; set; }
        public bool PushNotificationsEnabled { get; set; }
        public bool InAppNotificationsEnabled { get; set; }
    }

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

    // Dashboard Preferences DTOs
    public class DashboardPreferencesDto
    {
        public Guid Id { get; set; }
        public bool ShowAccountBalances { get; set; }
        public bool ShowRecentTransactions { get; set; }
        public bool ShowUpcomingPayments { get; set; }
        public bool ShowLoanStatus { get; set; }
        public bool ShowSavingsGoals { get; set; }
        public bool ShowQuickActions { get; set; }
        public bool ShowFinancialInsights { get; set; }
        public string Layout { get; set; }
        public string[] VisibleWidgets { get; set; }
        public string[] WidgetOrder { get; set; }
    }

    public class DashboardPreferencesUpdateDto
    {
        public bool? ShowAccountBalances { get; set; }
        public bool? ShowRecentTransactions { get; set; }
        public bool? ShowUpcomingPayments { get; set; }
        public bool? ShowLoanStatus { get; set; }
        public bool? ShowSavingsGoals { get; set; }
        public bool? ShowQuickActions { get; set; }
        public bool? ShowFinancialInsights { get; set; }
        public string Layout { get; set; }
        public string[] VisibleWidgets { get; set; }
        public string[] WidgetOrder { get; set; }
    }

    // Session DTOs
    public class ClientPortalSessionDto
    {
        public Guid Id { get; set; }
        public string SessionId { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public string IpAddress { get; set; }
        public string DeviceType { get; set; }
        public string Browser { get; set; }
        public string OperatingSystem { get; set; }
        public string Location { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuccessful { get; set; }
        public string FailureReason { get; set; }
    }

    public class ClientPortalActivityDto
    {
        public Guid Id { get; set; }
        public string ActivityType { get; set; }
        public string Page { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
        public DateTime Timestamp { get; set; }
    }

    // Document DTOs
    public class ClientDocumentDto
    {
        public Guid Id { get; set; }
        public string DocumentType { get; set; }
        public string DocumentName { get; set; }
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public long FileSize { get; set; }
        public bool IsSharedByBank { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool RequiresSignature { get; set; }
        public bool IsSigned { get; set; }
        public DateTime? SignedDate { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class UploadDocumentDto
    {
        public string DocumentType { get; set; }
        public string DocumentName { get; set; }
        public byte[] FileContent { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
    }

    // Support Ticket DTOs
    public class ClientSupportTicketDto
    {
        public Guid Id { get; set; }
        public string TicketNumber { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public DateTime? ClosedDate { get; set; }
        public string Resolution { get; set; }
        public int CustomerSatisfactionRating { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<ClientSupportMessageDto> Messages { get; set; }
    }

    public class CreateSupportTicketDto
    {
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Priority { get; set; }
        public string InitialMessage { get; set; }
    }

    public class ClientSupportMessageDto
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public bool IsFromClient { get; set; }
        public string SenderName { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class AddSupportMessageDto
    {
        public string Message { get; set; }
    }

    // Savings Goal DTOs
    public class SavingsGoalDto
    {
        public Guid Id { get; set; }
        public string GoalName { get; set; }
        public string Description { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public string Currency { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime TargetDate { get; set; }
        public string GoalCategory { get; set; }
        public string Status { get; set; }
        public string RecurrencePattern { get; set; }
        public decimal AutoTransferAmount { get; set; }
        public string SourceAccountNumber { get; set; }
        public string DestinationAccountNumber { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<SavingsGoalTransactionDto> Transactions { get; set; }
    }

    public class CreateSavingsGoalDto
    {
        public string GoalName { get; set; }
        public string Description { get; set; }
        public decimal TargetAmount { get; set; }
        public string Currency { get; set; } = "NGN";
        public DateTime StartDate { get; set; }
        public DateTime TargetDate { get; set; }
        public string GoalCategory { get; set; }
        public string RecurrencePattern { get; set; }
        public decimal AutoTransferAmount { get; set; }
        public string SourceAccountNumber { get; set; }
        public string DestinationAccountNumber { get; set; }
    }

    public class SavingsGoalTransactionDto
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string TransactionType { get; set; }
        public string SourceAccountNumber { get; set; }
        public string DestinationAccountNumber { get; set; }
        public string Reference { get; set; }
        public string Status { get; set; }
        public bool IsAutomatic { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    // Payee DTOs
    public class SavedPayeeDto
    {
        public Guid Id { get; set; }
        public string PayeeName { get; set; }
        public string AccountNumber { get; set; }
        public string BankName { get; set; }
        public string BankCode { get; set; }
        public string PayeeType { get; set; }
        public string Reference { get; set; }
        public string Category { get; set; }
        public bool IsFavorite { get; set; }
        public int TransactionCount { get; set; }
        public DateTime? LastUsed { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class CreateSavedPayeeDto
    {
        public string PayeeName { get; set; }
        public string AccountNumber { get; set; }
        public string BankName { get; set; }
        public string BankCode { get; set; }
        public string PayeeType { get; set; }
        public string Reference { get; set; }
        public string Category { get; set; }
        public bool IsFavorite { get; set; }
    }

    // Transfer Template DTOs
    public class SavedTransferTemplateDto
    {
        public Guid Id { get; set; }
        public string TemplateName { get; set; }
        public string FromAccountNumber { get; set; }
        public string ToAccountNumber { get; set; }
        public string ToBankName { get; set; }
        public string ToBankCode { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Reference { get; set; }
        public string Category { get; set; }
        public bool IsFavorite { get; set; }
        public int UsageCount { get; set; }
        public DateTime? LastUsed { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class CreateSavedTransferTemplateDto
    {
        public string TemplateName { get; set; }
        public string FromAccountNumber { get; set; }
        public string ToAccountNumber { get; set; }
        public string ToBankName { get; set; }
        public string ToBankCode { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "NGN";
        public string Reference { get; set; }
        public string Category { get; set; }
        public bool IsFavorite { get; set; }
    }

    // Notification DTOs
    public class ClientNotificationDto
    {
        public Guid Id { get; set; }
        public string NotificationType { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Action { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public string DeliveryChannels { get; set; }
        public string DeliveryStatus { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsActionable { get; set; }
        public bool IsDismissed { get; set; }
        public int Priority { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
